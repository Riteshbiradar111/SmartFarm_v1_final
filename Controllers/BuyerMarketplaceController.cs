using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    // This controller manages the marketplace shopping portal, farmer profiles list, pre-order list, and wishlist triggers.
    // It is written with clear, linear logic and step-by-step comments for easy explanation during interviews.
    public class BuyerMarketplaceController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public BuyerMarketplaceController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // Helper to check user session and get active Buyer
        private Buyer? GetActiveBuyer()
        {
            var username = HttpContext.Session.GetString("UserUsername");
            if (string.IsNullOrEmpty(username)) return null;

            return _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.User.Username == username);
        }

        // GET: /BuyerMarketplace
        public IActionResult Index(MarketplaceSearchViewModel search)
        {
            _context.EnsureMarketplaceColumnsExist();
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Start base query for available listings
            IQueryable<CropListing> query = _context.CropListings
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                .Where(l => l.Status == "Available");

            // 2. Apply search text (Crop Name or keyword)
            if (!string.IsNullOrEmpty(search.CropName))
            {
                string kw = search.CropName.Trim().ToLower();
                query = query.Where(l => l.Harvest.CropCycle.Crop.CropName.ToLower().Contains(kw));
            }

            // 3. Apply Farmer's name filter
            if (!string.IsNullOrEmpty(search.FarmerName))
            {
                string fn = search.FarmerName.Trim().ToLower();
                query = query.Where(l => l.Harvest.CropCycle.LandPlot.Farm.Farmer.FullName.ToLower().Contains(fn));
            }

            // 4. Apply Village location filter
            if (!string.IsNullOrEmpty(search.Village))
            {
                string vil = search.Village.Trim().ToLower();
                query = query.Where(l => l.Harvest.CropCycle.LandPlot.Farm.Village.ToLower().Contains(vil));
            }

            // 5. Apply District location filter
            if (!string.IsNullOrEmpty(search.District))
            {
                string dst = search.District.Trim().ToLower();
                query = query.Where(l => l.Harvest.CropCycle.LandPlot.Farm.District.ToLower().Contains(dst));
            }

            // 6. Apply Category filter (e.g. Grains, Vegetables, Fruits, Cash Crops)
            if (!string.IsNullOrEmpty(search.Category))
            {
                string cat = search.Category.Trim().ToLower();
                query = query.Where(l => l.Harvest.CropCycle.Crop.Season.ToLower().Contains(cat) ||
                                     l.Harvest.CropCycle.Crop.Description.ToLower().Contains(cat));
            }

            // 7. Apply Max Price filter
            if (search.MaxPrice.HasValue)
            {
                query = query.Where(l => l.PricePerUnit <= search.MaxPrice.Value);
            }

            // 8. Apply Min Quantity available filter
            if (search.MinQuantity.HasValue)
            {
                query = query.Where(l => l.AvailableQuantity >= search.MinQuantity.Value);
            }

            // 9. Execute query
            var listings = query.OrderByDescending(l => l.ListedDate).ToList();

            // 10. Extract all saved crop IDs in the buyer's wishlist to show the red heart on the UI
            var savedCropIds = _context.Wishlists
                .Where(w => w.BuyerId == buyer.BuyerId && w.CropId.HasValue)
                .Select(w => w.CropId!.Value)
                .ToList();

            ViewBag.SavedCropIds = savedCropIds;
            ViewBag.SearchModel = search;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(listings);
        }

        // GET: /BuyerMarketplace/Details/{id}
        public IActionResult Details(int id)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Query the specific produce listing
            var listing = _context.CropListings
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                .FirstOrDefault(l => l.ListingId == id);

            if (listing == null)
            {
                TempData["ErrorMessage"] = "Produce listing not found.";
                return RedirectToAction("Index");
            }

            // 2. Check if this crop type is already saved in the buyer's wishlist
            var cropId = listing.Harvest.CropCycle.CropId;
            bool isSaved = _context.Wishlists
                .Any(w => w.BuyerId == buyer.BuyerId && w.CropId == cropId);

            // 3. Check if they have already subscribed to "Notify Me" notifications
            bool isSubscribed = _context.Wishlists
                .Any(w => w.BuyerId == buyer.BuyerId && w.CropId == cropId && w.NotifyWhenAvailable);

            ViewBag.IsSaved = isSaved;
            ViewBag.IsSubscribed = isSubscribed;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(listing);
        }

        // GET: /BuyerMarketplace/HarvestReady
        // Redirects pre-order endpoint to main produce marketplace
        public IActionResult HarvestReady()
        {
            return RedirectToAction("Index");
        }

        // GET: /BuyerMarketplace/FarmerProfiles
        // Displays list directory of registered farmers
        public IActionResult FarmerProfiles()
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Fetch directory list of farmers
            var farmers = _context.Farmers
                .Include(f => f.User)
                .OrderBy(f => f.FullName)
                .ToList();

            // 2. Fetch list of saved farmer IDs in buyer's wishlist
            var savedFarmerIds = _context.Wishlists
                .Where(w => w.BuyerId == buyer.BuyerId && w.FarmerId.HasValue)
                .Select(w => w.FarmerId!.Value)
                .ToList();

            ViewBag.SavedFarmerIds = savedFarmerIds;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(farmers);
        }

        // POST: /BuyerMarketplace/ToggleWishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleWishlist(int? cropId, int? farmerId, string returnUrl)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Look for existing saved record
            var existing = _context.Wishlists.FirstOrDefault(w =>
                w.BuyerId == buyer.BuyerId &&
                w.CropId == cropId &&
                w.FarmerId == farmerId);

            if (existing != null)
            {
                // Remove if already in wishlist
                _context.Wishlists.Remove(existing);
                TempData["SuccessMessage"] = "Removed from your Wishlist.";
            }
            else
            {
                // Create new saved item
                var item = new Wishlist
                {
                    BuyerId = buyer.BuyerId,
                    CropId = cropId,
                    FarmerId = farmerId,
                    CreatedDate = DateTime.Now
                };
                _context.Wishlists.Add(item);
                TempData["SuccessMessage"] = "Added to your Wishlist successfully!";
            }

            _context.SaveChanges();

            // Redirect back to page where action happened
            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        // POST: /BuyerMarketplace/ToggleNotifyMe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleNotifyMe(int cropId, string returnUrl)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Fetch wishlist item or create if not present
            var item = _context.Wishlists.FirstOrDefault(w => w.BuyerId == buyer.BuyerId && w.CropId == cropId);
            if (item == null)
            {
                item = new Wishlist
                {
                    BuyerId = buyer.BuyerId,
                    CropId = cropId,
                    CreatedDate = DateTime.Now
                };
                _context.Wishlists.Add(item);
            }

            // Toggle subscription status
            item.NotifyWhenAvailable = !item.NotifyWhenAvailable;
            _context.SaveChanges();

            TempData["SuccessMessage"] = item.NotifyWhenAvailable
                ? "Notification registered! We will alert you as soon as this crop is listed."
                : "Unsubscribed from stock notification updates.";

            if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index");
        }
    }
}
