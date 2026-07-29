using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    // This controller manages order placements (checkout), order details, Flipkart-style progress timelines, and printing invoices.
    // It is written with clear, linear logic and step-by-step comments for easy explanation during interviews.
    public class BuyerOrderController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public BuyerOrderController(SmartFarmDbContext context)
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

        // GET: /BuyerOrder
        public IActionResult Index()
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Fetch all orders for this buyer
            var orders = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(o => o.Farmer)
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // 2. Separate into lists based on timeline status
            // Purchase Requests: Pending farmer approval
            ViewBag.Requests = orders.Where(o => o.Status == "Pending" || o.Status == "Request Sent" || o.Status == "PENDING_FARMER_ACCEPTANCE" || o.Status == "REQUESTED").ToList();

            // Active Orders: Accepted, paid, preparing, or in transit
            ViewBag.ActiveOrders = orders.Where(o => o.Status == "Accepted" || o.Status == "Farmer Accepted" || o.Status == "Paid" || o.Status == "Preparing Produce" || o.Status == "Ready for Pickup" || o.Status == "In Transit").ToList();

            // Declined Orders: Requests declined by farmer with mandatory reason
            ViewBag.DeclinedOrders = orders.Where(o => o.Status == "Declined" || o.Status == "Rejected").ToList();

            // Completed Orders: Archive of successfully delivered goods
            ViewBag.CompletedOrders = orders.Where(o => o.Status == "Completed" || o.Status == "Delivered").ToList();

            // Compute buyer sequential order map
            var allBuyerOrdersAscending = _context.CropOrders
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.OrderId)
                .Select(o => o.OrderId)
                .ToList();

            var orderNumberMap = new Dictionary<int, int>();
            for (int i = 0; i < allBuyerOrdersAscending.Count; i++)
            {
                orderNumberMap[allBuyerOrdersAscending[i]] = i + 1;
            }
            ViewBag.OrderNumberMap = orderNumberMap;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View();
        }

        // GET: /BuyerOrder/Details/{id}
        public IActionResult Details(int id)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Query the specific order details including nested properties
            var order = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(o => o.Farmer)
                .Include(o => o.Buyer)
                .FirstOrDefault(o => o.OrderId == id && o.BuyerId == buyer.BuyerId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            var allBuyerOrdersAscending = _context.CropOrders
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.OrderId)
                .Select(o => o.OrderId)
                .ToList();

            int seqNo = allBuyerOrdersAscending.IndexOf(order.OrderId) + 1;
            if (seqNo <= 0) seqNo = order.OrderId;
            ViewBag.BuyerOrderSeqNumber = seqNo;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(order);
        }

        // GET: /BuyerOrder/PlaceOrder
        // Renders checkout screen
        public IActionResult PlaceOrder(int? listingId, int? harvestId, decimal quantity)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Guard against empty selections
            if (!listingId.HasValue && !harvestId.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid checkout attempt.";
                return RedirectToAction("Index", "BuyerMarketplace");
            }

            // 2. Map checkout details depending on whether it is standard or pre-order
            if (listingId.HasValue)
            {
                // Standard Purchase
                var listing = _context.CropListings
                    .Include(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                    .Include(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.LandPlot)
                                .ThenInclude(p => p.Farm)
                                    .ThenInclude(f => f.Farmer)
                    .FirstOrDefault(l => l.ListingId == listingId.Value);

                if (listing == null || listing.Status != "Available")
                {
                    TempData["ErrorMessage"] = "This crop is not currently listed for sale by the farmer.";
                    return RedirectToAction("Index", "BuyerMarketplace");
                }

                // Check available quantity limit
                if (quantity > listing.AvailableQuantity)
                {
                    TempData["ErrorMessage"] = $"Requested quantity exceeds available stock ({listing.AvailableQuantity} {listing.Unit}).";
                    return RedirectToAction("Details", "BuyerMarketplace", new { id = listingId.Value });
                }

                ViewBag.ItemTitle = listing.Harvest.CropCycle.Crop.CropName;
                ViewBag.Rate = listing.PricePerUnit;
                ViewBag.Unit = listing.Unit;
                ViewBag.FarmerName = listing.Harvest.CropCycle.LandPlot.Farm.Farmer.FullName;
                ViewBag.Type = "Standard Purchase";
                ViewBag.ListingId = listing.ListingId;
                ViewBag.HarvestId = (int?)null;
            }
            else
            {
                // Pre-Order on Harvest Ready Crop
                var harvest = _context.Harvests
                    .Include(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                    .Include(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                    .FirstOrDefault(h => h.HarvestId == harvestId!.Value);

                if (harvest == null)
                {
                    TempData["ErrorMessage"] = "Harvest record not found.";
                    return RedirectToAction("HarvestReady", "BuyerMarketplace");
                }

                // Enforce active crop listing validation for pre-orders
                var hasListing = _context.CropListings.Any(cl => cl.HarvestId == harvestId.Value && cl.Status == "Available");
                if (!hasListing)
                {
                    TempData["ErrorMessage"] = "This crop is not currently listed for sale by the farmer.";
                    return RedirectToAction("HarvestReady", "BuyerMarketplace");
                }

                // Check available quantity limit
                if (quantity > harvest.ActualQuantity)
                {
                    TempData["ErrorMessage"] = $"Requested prebook quantity exceeds harvested stock ({harvest.ActualQuantity} {harvest.Unit}).";
                    return RedirectToAction("HarvestReady", "BuyerMarketplace");
                }

                // Standard crop prebook price estimation (e.g. wheat flat rate or mock estimation if listing does not exist yet)
                decimal mockEstimatedPrice = 35.00m; // Default estimated fallback price

                ViewBag.ItemTitle = harvest.CropCycle.Crop.CropName + " (Prebook)";
                ViewBag.Rate = mockEstimatedPrice;
                ViewBag.Unit = harvest.Unit;
                ViewBag.FarmerName = harvest.CropCycle.LandPlot.Farm.Farmer.FullName;
                ViewBag.Type = "Harvest Prebook Request";
                ViewBag.ListingId = (int?)null;
                ViewBag.HarvestId = harvest.HarvestId;
            }

            // 3. Compute billing metrics
            decimal rate = ViewBag.Rate;
            decimal subtotal = quantity * rate;
            decimal gst = subtotal * 0.05m;
            decimal total = subtotal + gst;

            ViewBag.Quantity = quantity;
            ViewBag.Subtotal = subtotal;
            ViewBag.GST = gst;
            ViewBag.Total = total;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(new CropOrderViewModel { ListingId = listingId, HarvestId = harvestId, Quantity = quantity });
        }

        // POST: /BuyerOrder/PlaceOrder
        // Validates order and redirects to payment page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CropOrderViewModel model)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid checkout data parameters.";
                return RedirectToAction("Index", "BuyerMarketplace");
            }

            try
            {
                int farmerId = 0;
                decimal pricePerUnit = 0.00m;
                string? unit = null;
                string farmerName = "";
                string itemTitle = "";

                // 1. Process listing purchase request
                if (model.ListingId.HasValue)
                {
                    var listing = _context.CropListings
                        .Include(l => l.Harvest)
                            .ThenInclude(h => h.CropCycle)
                                .ThenInclude(c => c.Crop)
                        .Include(l => l.Harvest)
                            .ThenInclude(h => h.CropCycle)
                                .ThenInclude(c => c.LandPlot)
                                    .ThenInclude(p => p.Farm)
                                        .ThenInclude(f => f.Farmer)
                        .FirstOrDefault(l => l.ListingId == model.ListingId.Value);

                    if (listing == null || listing.Status != "Available")
                    {
                        TempData["ErrorMessage"] = "This crop is not currently listed for sale by the farmer.";
                        return RedirectToAction("Index", "BuyerMarketplace");
                    }

                    if (model.Quantity > listing.AvailableQuantity)
                    {
                        TempData["ErrorMessage"] = $"Requested quantity exceeds available stock ({listing.AvailableQuantity} {listing.Unit}).";
                        return RedirectToAction("Details", "BuyerMarketplace", new { id = listing.ListingId });
                    }

                    farmerId = listing.Harvest.CropCycle.LandPlot.Farm.FarmerId;
                    pricePerUnit = listing.PricePerUnit;
                    unit = listing.Unit;
                    farmerName = listing.Harvest.CropCycle.LandPlot.Farm.Farmer.FullName;
                    itemTitle = listing.Harvest.CropCycle.Crop.CropName;

                    // Calculate totals
                    decimal subtotal = model.Quantity * pricePerUnit;
                    decimal gstAmount = subtotal * 0.05m;
                    decimal grandTotal = subtotal + gstAmount;

                    // Generate invoice number
                    Random rand = new Random();
                    string invNo = $"INV-{DateTime.Now.Year}-{rand.Next(10000, 99999)}";

                    // Save order in PENDING_FARMER_ACCEPTANCE status
                    var order = new CropOrder
                    {
                        ListingId = model.ListingId.Value,
                        BuyerId = buyer.BuyerId,
                        FarmerId = farmerId,
                        Quantity = model.Quantity,
                        PricePerUnit = pricePerUnit,
                        TotalAmount = grandTotal,
                        Status = "PENDING_FARMER_ACCEPTANCE",
                        OrderDate = DateTime.Now,
                        InvoiceNumber = invNo,
                        GST = 5.00m,
                        DeliveryAddress = model.DeliveryAddress,
                        SpecialInstructions = model.SpecialInstructions
                    };

                    _context.CropOrders.Add(order);
                    _context.SaveChanges();

                    // Notify farmer
                    var farmerUser = listing.Harvest.CropCycle.LandPlot.Farm.Farmer.User;
                    if (farmerUser != null)
                    {
                        _context.Notifications.Add(new Notification
                        {
                            UserId = farmerUser.UserId,
                            Title = "New Crop Order Request",
                            Message = $"Buyer {buyer.FullName} requested an order of {model.Quantity} {unit} of {itemTitle}. Please accept the order to enable buyer payment.",
                            IsRead = false,
                            CreatedDate = DateTime.Now
                        });
                        _context.SaveChanges();
                    }

                    TempData["SuccessMessage"] = "Order request placed successfully! Awaiting farmer acceptance. Once accepted by the farmer, payment will be enabled.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Invalid order request details.";
                return RedirectToAction("Index", "BuyerMarketplace");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error processing checkout: " + ex.Message;
                return RedirectToAction("Index", "BuyerMarketplace");
            }
        }

        // GET: /BuyerOrder/PaymentDetails
        // Shows mock payment details page for demo
        public IActionResult PaymentDetails(int? orderId)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            if (orderId.HasValue)
            {
                var order = _context.CropOrders
                    .Include(o => o.Farmer)
                    .Include(o => o.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                    .FirstOrDefault(o => o.OrderId == orderId.Value && o.BuyerId == buyer.BuyerId);

                if (order == null) return NotFound();

                // Check payment enablement: ONLY allowed AFTER farmer accepts
                if (order.Status == "PENDING_FARMER_ACCEPTANCE" || order.Status == "REQUESTED")
                {
                    TempData["ErrorMessage"] = "Payment will be available once the farmer accepts your order request.";
                    return RedirectToAction("Details", new { id = orderId.Value });
                }

                // If accepted, populate ViewBag
                ViewBag.OrderId = order.OrderId;
                ViewBag.ListingId = order.ListingId;
                ViewBag.HarvestId = order.HarvestId;
                ViewBag.Quantity = order.Quantity;
                ViewBag.PricePerUnit = order.PricePerUnit;
                ViewBag.Unit = order.CropListing?.Unit ?? order.Harvest?.Unit ?? "kg";
                ViewBag.FarmerId = order.FarmerId;
                ViewBag.FarmerName = order.Farmer.FullName;
                ViewBag.ItemTitle = (order.Harvest?.CropCycle?.Crop?.CropName ?? "Crop") + " (Pre-Order)";
                ViewBag.Subtotal = order.Quantity * order.PricePerUnit;
                ViewBag.GST = order.GST;
                ViewBag.Total = order.TotalAmount;
            }
            else
            {
                // Retrieve order details from TempData
                if (TempData["PaymentTotal"] == null)
                {
                    TempData["ErrorMessage"] = "Payment session expired. Please try again.";
                    return RedirectToAction("Index", "BuyerMarketplace");
                }

                // Pass data to view (parse strings back to appropriate types)
                ViewBag.ListingId = !string.IsNullOrEmpty(TempData["PaymentListingId"]?.ToString()) ? int.Parse(TempData["PaymentListingId"]!.ToString()!) : (int?)null;
                ViewBag.HarvestId = !string.IsNullOrEmpty(TempData["PaymentHarvestId"]?.ToString()) ? int.Parse(TempData["PaymentHarvestId"]!.ToString()!) : (int?)null;
                ViewBag.Quantity = decimal.Parse(TempData["PaymentQuantity"]!.ToString()!);
                ViewBag.PricePerUnit = decimal.Parse(TempData["PaymentPricePerUnit"]!.ToString()!);
                ViewBag.Unit = TempData["PaymentUnit"]!.ToString();
                ViewBag.FarmerId = int.Parse(TempData["PaymentFarmerId"]!.ToString()!);
                ViewBag.FarmerName = TempData["PaymentFarmerName"]!.ToString();
                ViewBag.ItemTitle = TempData["PaymentItemTitle"]!.ToString();
                ViewBag.Subtotal = decimal.Parse(TempData["PaymentSubtotal"]!.ToString()!);
                ViewBag.GST = decimal.Parse(TempData["PaymentGST"]!.ToString()!);
                ViewBag.Total = decimal.Parse(TempData["PaymentTotal"]!.ToString()!);

                // Keep TempData for the next request (ConfirmPayment POST)
                TempData.Keep();
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View();
        }

        // POST: /BuyerOrder/ConfirmPayment
        // Finalizes order after simulated payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmPayment(int? orderId)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            if (orderId.HasValue)
            {
                var order = _context.CropOrders
                    .Include(o => o.Farmer)
                    .Include(o => o.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                    .FirstOrDefault(o => o.OrderId == orderId.Value && o.BuyerId == buyer.BuyerId);

                if (order == null) return NotFound();

                try
                {
                    order.Status = "Paid";

                    if (order.ListingId.HasValue)
                    {
                        var listing = _context.CropListings.FirstOrDefault(l => l.ListingId == order.ListingId.Value);
                        if (listing != null)
                        {
                            listing.BuyerId = buyer.BuyerId;
                            listing.PurchasedQuantity = (listing.PurchasedQuantity ?? 0) + order.Quantity;
                            listing.PurchaseDate = DateTime.Now;
                            listing.AvailableQuantity = Math.Max(0, listing.AvailableQuantity - order.Quantity);
                            if (listing.AvailableQuantity <= 0)
                            {
                                listing.Status = "Sold";
                            }
                        }
                    }

                    _context.SaveChanges();

                    // Notify farmer
                    if (order.Farmer != null)
                    {
                        var farmerNotif = new Notification
                        {
                            UserId = order.Farmer.UserId,
                            Title = "Order Payment Received",
                            Message = $"Buyer {buyer.FullName} has completed payment of ₹{order.TotalAmount:N2} for order #{order.OrderId}.",
                            IsRead = false,
                            CreatedDate = DateTime.Now
                        };
                        _context.Notifications.Add(farmerNotif);
                        _context.SaveChanges();
                    }

                    TempData["SuccessMessage"] = $"Payment successful! Your order #{order.OrderId} is now paid.";
                    return RedirectToAction("Details", new { id = order.OrderId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error processing payment: " + ex.Message;
                    return RedirectToAction("Details", new { id = orderId.Value });
                }
            }

            // Retrieve order details from TempData
            if (TempData["PaymentTotal"] == null)
            {
                TempData["ErrorMessage"] = "Payment session expired. Please try again.";
                return RedirectToAction("Index", "BuyerMarketplace");
            }

            // Parse TempData strings back to original types
            int? listingId = !string.IsNullOrEmpty(TempData["PaymentListingId"]?.ToString()) ? int.Parse(TempData["PaymentListingId"]!.ToString()!) : (int?)null;
            int? harvestId = !string.IsNullOrEmpty(TempData["PaymentHarvestId"]?.ToString()) ? int.Parse(TempData["PaymentHarvestId"]!.ToString()!) : (int?)null;
            decimal quantity = decimal.Parse(TempData["PaymentQuantity"]!.ToString()!);
            decimal pricePerUnit = decimal.Parse(TempData["PaymentPricePerUnit"]!.ToString()!);
            string? unit = TempData["PaymentUnit"]!.ToString();
            int farmerId = int.Parse(TempData["PaymentFarmerId"]!.ToString()!);
            decimal grandTotal = decimal.Parse(TempData["PaymentTotal"]!.ToString()!);
            string? deliveryAddress = TempData["PaymentDeliveryAddress"]?.ToString();
            string? specialInstructions = TempData["PaymentSpecialInstructions"]?.ToString();

            try
            {
                // 1. Process stock reductions and purchase tracking
                if (listingId.HasValue)
                {
                    var listing = _context.CropListings
                        .FirstOrDefault(l => l.ListingId == listingId.Value);

                    if (listing != null)
                    {
                        // Update purchase details
                        listing.BuyerId = buyer.BuyerId;
                        listing.PurchasedQuantity = (listing.PurchasedQuantity ?? 0) + quantity; // Accumulate if partial purchases
                        listing.PurchaseDate = DateTime.Now;

                        // Reduce available stock
                        listing.AvailableQuantity -= quantity;
                        if (listing.AvailableQuantity == 0)
                        {
                            listing.Status = "Sold";
                        }
                    }
                }
                else if (harvestId.HasValue)
                {
                    var harvest = _context.Harvests
                        .FirstOrDefault(h => h.HarvestId == harvestId.Value);

                    if (harvest != null)
                    {
                        harvest.ActualQuantity -= quantity;
                        if (harvest.ActualQuantity == 0)
                        {
                            harvest.Status = "Reserved";
                        }
                    }
                }

                // 2. Auto-generate invoice format: INV-YEAR-RANDOM
                Random rand = new Random();
                string invNo = $"INV-{DateTime.Now.Year}-{rand.Next(10000, 99999)}";

                // 3. Save order record to SQL Server
                var order = new CropOrder
                {
                    ListingId = listingId,
                    HarvestId = harvestId,
                    BuyerId = buyer.BuyerId,
                    FarmerId = farmerId,
                    Quantity = quantity,
                    PricePerUnit = pricePerUnit,
                    TotalAmount = grandTotal,
                    Status = "Request Sent",
                    OrderDate = DateTime.Now,
                    InvoiceNumber = invNo,
                    GST = 5.00m,
                    DeliveryAddress = deliveryAddress,
                    SpecialInstructions = specialInstructions
                };

                _context.CropOrders.Add(order);
                _context.SaveChanges();

                // 4. Generate notifications for event-driven feedback
                // Buyer notification
                var buyerNotif = new Notification
                {
                    UserId = buyer.UserId,
                    Title = "Order Placed Successfully",
                    Message = $"Your order request #{order.OrderId} for {quantity} {unit} has been submitted and payment confirmed.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(buyerNotif);

                // Farmer notification
                var farmer = _context.Farmers.FirstOrDefault(f => f.FarmerId == farmerId);
                if (farmer != null)
                {
                    var farmerNotif = new Notification
                    {
                        UserId = farmer.UserId,
                        Title = "New Purchase Request",
                        Message = $"Buyer {buyer.FullName} placed a purchase request #{order.OrderId} for {quantity} {unit} with payment confirmation.",
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    };
                    _context.Notifications.Add(farmerNotif);
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Payment successful! Your order #{order.OrderId} has been placed and payment of ₹{grandTotal:N2} is confirmed. The farmer has been notified.";
                return RedirectToAction("Details", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error processing payment: " + ex.Message;
                return RedirectToAction("Index", "BuyerMarketplace");
            }
        }

        // GET: /BuyerOrder/Invoice/{id}
        // Serves printable invoice receipt details
        public IActionResult Invoice(int id)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Fetch invoice info
            var order = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(o => o.Farmer)
                .Include(o => o.Buyer)
                .FirstOrDefault(o => o.OrderId == id && o.BuyerId == buyer.BuyerId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Declined" || order.Status == "Rejected" || order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Invoices are not generated for declined or cancelled order requests.";
                return RedirectToAction("Details", new { id = id });
            }

            return View(order);
        }

        /* REMOVED: This simulation panel should not be accessible to buyers.
         * Order status updates should only be performed by the FARMER from their dashboard.
         * Keeping this code commented for reference/testing purposes only.
         * 
        // POST: /BuyerOrder/UpdateStatusSimulate
        // Helper shortcut to update the shipment status timeline for testing/interview demo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatusSimulate(int id, string nextStatus)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            var order = _context.CropOrders.FirstOrDefault(o => o.OrderId == id && o.BuyerId == buyer.BuyerId);
            if (order != null)
            {
                // Update status
                order.Status = nextStatus;

                if (nextStatus == "Farmer Accepted")
                {
                    order.AcceptedDate = DateTime.Now;
                }
                else if (nextStatus == "Delivered")
                {
                    order.DeliveryDate = DateTime.Now;
                }

                _context.SaveChanges();

                // Log notification event
                var notification = new Notification
                {
                    UserId = buyer.UserId,
                    Title = $"Order Status Updated",
                    Message = $"Your order #{order.OrderId} is now: {nextStatus}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Shipment status simulated to: {nextStatus}";
            }

            return RedirectToAction("Details", new { id = id });
        }
        */
    }
}
