/* ============================================================
   SMARTFARM — MINIMAL JAVASCRIPT
   - Sticky navbar scroll effect
   - Mobile hamburger menu
   - FAQ accordion
   - Back to top button
   No SPA, no LocalStorage, no APIs, no complex animations.
   Compatible with Razor Views (.cshtml) migration.
============================================================ */

(function () {
    "use strict";

    /* ---- STICKY NAVBAR ---- */
    var navbar = document.getElementById("navbar");
    if (navbar) {
        window.addEventListener("scroll", function () {
            if (window.scrollY > 30) {
                navbar.classList.add("scrolled");
            } else {
                navbar.classList.remove("scrolled");
            }
        });
    }

    /* ---- MOBILE HAMBURGER MENU ---- */
    var hamburger = document.getElementById("hamburger");
    var navLinks = document.getElementById("navLinks");
    if (hamburger && navLinks) {
        hamburger.addEventListener("click", function () {
            var isOpen = navLinks.classList.toggle("open");
            hamburger.setAttribute("aria-expanded", isOpen ? "true" : "false");
            var icon = hamburger.querySelector(".material-icons");
            if (icon) icon.textContent = isOpen ? "close" : "menu";
        });

        /* Close on nav link click */
        navLinks.querySelectorAll("a").forEach(function (link) {
            link.addEventListener("click", function () {
                navLinks.classList.remove("open");
                hamburger.setAttribute("aria-expanded", "false");
                var icon = hamburger.querySelector(".material-icons");
                if (icon) icon.textContent = "menu";
            });
        });
    }

    /* ---- FAQ ACCORDION ---- */
    var faqItems = document.querySelectorAll(".faq-item");
    faqItems.forEach(function (item) {
        var btn = item.querySelector(".faq-question");
        if (!btn) return;

        btn.addEventListener("click", function () {
            var isOpen = item.classList.contains("open");

            /* Close all open items */
            faqItems.forEach(function (el) {
                el.classList.remove("open");
                var b = el.querySelector(".faq-question");
                if (b) b.setAttribute("aria-expanded", "false");
            });

            /* Toggle clicked item */
            if (!isOpen) {
                item.classList.add("open");
                btn.setAttribute("aria-expanded", "true");
            }
        });
    });

    /* ---- BACK TO TOP BUTTON ---- */
    var backToTop = document.getElementById("backToTop");
    if (backToTop) {
        window.addEventListener("scroll", function () {
            if (window.scrollY > 400) {
                backToTop.classList.add("visible");
            } else {
                backToTop.classList.remove("visible");
            }
        });

        backToTop.addEventListener("click", function () {
            window.scrollTo({ top: 0, behavior: "smooth" });
        });
    }

    /* ---- SMOOTH SCROLL FOR ANCHOR LINKS ---- */
    document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
        anchor.addEventListener("click", function (e) {
            var target = document.querySelector(this.getAttribute("href"));
            if (target) {
                e.preventDefault();
                var offset = 70; /* navbar height */
                var top = target.getBoundingClientRect().top + window.pageYOffset - offset;
                window.scrollTo({ top: top, behavior: "smooth" });
            }
        });
    });

})();
