(function () {
	function setTheme(theme) {
		document.documentElement.classList.toggle('dark', theme === 'dark');
		localStorage.setItem('theme', theme);
	}

	function currentTheme() {
		return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
	}

	function wireUp() {
		var themeToggle = document.getElementById('theme-toggle');
		if (themeToggle) {
			themeToggle.addEventListener('click', function () {
				setTheme(currentTheme() === 'dark' ? 'light' : 'dark');
			});
		}

		var mobileMenuToggle = document.getElementById('mobile-menu-toggle');
		var mobileMenu = document.getElementById('mobile-menu');
		if (mobileMenuToggle && mobileMenu) {
			mobileMenuToggle.addEventListener('click', function () {
				var isHidden = mobileMenu.hasAttribute('hidden');
				if (isHidden) {
					mobileMenu.removeAttribute('hidden');
				} else {
					mobileMenu.setAttribute('hidden', '');
				}
				mobileMenuToggle.setAttribute('aria-expanded', String(isHidden));
			});
		}
	}

	// Interactive Server re-renders can replace these elements; re-wire after each enhanced navigation.
	document.addEventListener('DOMContentLoaded', wireUp);
	if (window.Blazor) {
		document.addEventListener('enhancedload', wireUp);
	}
})();
