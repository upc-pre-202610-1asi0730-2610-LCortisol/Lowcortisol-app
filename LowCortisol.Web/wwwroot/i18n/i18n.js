window.appLang = {
    get: function () {
        return localStorage.getItem("app_lang") || "es";
    },
    set: function (lang) {
        localStorage.setItem("app_lang", lang);
    }
};