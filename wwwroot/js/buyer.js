/* =========================================
   SmartFarm Dashboard — Buyer Specific JS
   ========================================= */

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
