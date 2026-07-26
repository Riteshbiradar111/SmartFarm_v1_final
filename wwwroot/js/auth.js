/* ============================================================
   SMARTFARM — AUTH PAGES JAVASCRIPT (login.html & register.html)
   - Password visibility toggle
   - Password match checker
   - Basic form validation feedback
   No SPA, no LocalStorage, no APIs. Razor-compatible.
============================================================ */

(function () {
    "use strict";

    /* ---- Helper: toggle password visibility ---- */
    function setupPasswordToggle(toggleBtnId, inputId, iconId) {
        var btn = document.getElementById(toggleBtnId);
        var input = document.getElementById(inputId);
        var icon = document.getElementById(iconId);
        if (!btn || !input || !icon) return;

        btn.addEventListener("click", function () {
            var isVisible = input.type === "text";
            input.type = isVisible ? "password" : "text";
            icon.textContent = isVisible ? "visibility_off" : "visibility";
            btn.setAttribute("aria-pressed", (!isVisible).toString());
        });
    }

    /* ---- LOGIN PAGE ---- */
    setupPasswordToggle("togglePassword", "password", "eyeIcon");

    /* ---- REGISTER PAGE ---- */
    setupPasswordToggle("toggleRegPassword", "regPassword", "regEyeIcon");
    setupPasswordToggle("toggleConfirmPassword", "confirmPassword", "confirmEyeIcon");

    /* Password match checker */
    var regPwd = document.getElementById("regPassword");
    var confirmPwd = document.getElementById("confirmPassword");
    var matchMsg = document.getElementById("passwordMatchMsg");

    function checkPasswordMatch() {
        if (!regPwd || !confirmPwd || !matchMsg) return;
        var p1 = regPwd.value;
        var p2 = confirmPwd.value;
        if (p2.length === 0) {
            matchMsg.textContent = "";
            matchMsg.className = "password-match-msg";
            return;
        }
        if (p1 === p2) {
            matchMsg.textContent = "Passwords match";
            matchMsg.className = "password-match-msg match";
        } else {
            matchMsg.textContent = "Passwords do not match";
            matchMsg.className = "password-match-msg nomatch";
        }
    }

    if (confirmPwd) confirmPwd.addEventListener("input", checkPasswordMatch);
    if (regPwd) regPwd.addEventListener("input", checkPasswordMatch);

    /* ---- Basic form submit guard ---- */
    var loginForm = document.getElementById("loginForm");
    var registerForm = document.getElementById("registerForm");

    if (loginForm) {
        loginForm.addEventListener("submit", function (e) {
            var email = document.getElementById("emailOrUsername");
            var pwd = document.getElementById("password");
            if (!email || !pwd) return;

            var val = email.value.trim();

            if (!val) {
                e.preventDefault();
                email.focus();
                return;
            }
        });
    }

    if (registerForm) {
        registerForm.addEventListener("submit", function (e) {
            var p1 = document.getElementById("regPassword");
            var p2 = document.getElementById("confirmPassword");
            var terms = document.getElementById("agreeTerms");

            if (p1 && p2 && p1.value !== p2.value) {
                e.preventDefault();
                if (matchMsg) {
                    matchMsg.textContent = "Passwords do not match — please fix before submitting.";
                    matchMsg.className = "password-match-msg nomatch";
                }
                p2.focus();
                return;
            }
            if (terms && !terms.checked) {
                e.preventDefault();
                terms.focus();
            }
        });
    }

})();
