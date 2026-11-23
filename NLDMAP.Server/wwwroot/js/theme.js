window.nldmapTheme = {
    get: function () {
        const value = localStorage.getItem("nldmap-theme");

        if (value === null) {
            return null;
        }

        return value === "dark";
    },

    set: function (isDarkMode) {
        const theme = isDarkMode ? "dark" : "light";

        localStorage.setItem("nldmap-theme", theme);

        document.cookie =
            `nldmap-theme=${theme}; path=/; max-age=31536000; SameSite=Lax`;
    }
};