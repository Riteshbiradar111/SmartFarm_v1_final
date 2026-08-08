
(function () {
    'use strict';

    function initSidebar() {
        var sidebar = document.getElementById('sidebar');
        var dashMain = document.getElementById('dashMain');
        var toggleBtn = document.getElementById('sidebarToggleBtn');

        if (!sidebar || !toggleBtn) return;

        toggleBtn.addEventListener('click', function () {
            var isCollapsed = sidebar.classList.toggle('collapsed');
            if (dashMain) dashMain.classList.toggle('sidebar-collapsed', isCollapsed);
            document.body.classList.toggle('sb-collapsed', isCollapsed);
            var icon = toggleBtn.querySelector('.material-icons-round');
            if (icon) icon.textContent = isCollapsed ? 'chevron_right' : 'chevron_left';
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSidebar);
    } else {
        initSidebar();
    }
})();

/**
 * Topbar interactions: profile panel toggle
 */
(function () {
    'use strict';

    function initTopbar() {
        var topbarUser = document.querySelector('.topbar-user');
        var profilePanel = document.getElementById('profilePanel');

        if (!topbarUser || !profilePanel) return;

        function closeProfile() {
            profilePanel.classList.remove('open');
            profilePanel.setAttribute('aria-hidden', 'true');
        }

        // Toggle profile panel when clicking on user profile
        topbarUser.addEventListener('click', function (ev) {
            ev.stopPropagation();
            var isOpen = profilePanel.classList.toggle('open');
            profilePanel.setAttribute('aria-hidden', (!isOpen).toString());
        });

        // Close when clicking outside
        document.addEventListener('click', function (ev) {
            var target = ev.target;
            if (!profilePanel.contains(target) && !topbarUser.contains(target)) {
                closeProfile();
            }
        });

        // Close on Escape
        document.addEventListener('keydown', function (ev) {
            if (ev.key === 'Escape') closeProfile();
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTopbar);
    } else {
        initTopbar();
    }
})();

/**
 * Sidebar active state logic
 * Highlights the current page in the sidebar navigation
 */
(function () {
    'use strict';

    function initSidebarActiveState() {
        var currentPath = window.location.pathname.toLowerCase();
        var navItems = document.querySelectorAll('.sidebar-nav .nav-item');

        navItems.forEach(function (item) {
            // Remove active class from all items first
            item.classList.remove('active');

            // Get the href and normalize it
            var href = item.getAttribute('href');
            if (!href) return;

            // Convert href to absolute path
            var link = href.startsWith('~/') ? href.substring(1) : href;
            link = link.toLowerCase();

            // Check if current path matches this link
            if (currentPath === link || currentPath.startsWith(link + '/') || currentPath.startsWith(link + '?')) {
                item.classList.add('active');
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSidebarActiveState);
    } else {
        initSidebarActiveState();
    }
})();
