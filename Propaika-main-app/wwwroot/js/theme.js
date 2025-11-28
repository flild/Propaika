(function () {
    const html = document.documentElement;
    const THEME_KEY = "propaika-theme";

    function applyTheme(theme) {
        if (theme !== "light" && theme !== "dark") {
            theme = "dark";
        }
        html.setAttribute("data-bs-theme", theme);
        localStorage.setItem(THEME_KEY, theme);

        document.getElementById("theme-light")?.classList.toggle("active", theme === "light");
        document.getElementById("theme-dark")?.classList.toggle("active", theme === "dark");
    }

    // Инициализация
    const saved = localStorage.getItem(THEME_KEY);
    applyTheme(saved || "dark");

    document.getElementById("theme-light")?.addEventListener("click", () => applyTheme("light"));
    document.getElementById("theme-dark")?.addEventListener("click", () => applyTheme("dark"));
})();
