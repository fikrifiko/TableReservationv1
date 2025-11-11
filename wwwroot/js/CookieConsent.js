document.addEventListener("DOMContentLoaded", function () {
    const banner = document.getElementById("cookieBanner");
    const acceptBtn = document.getElementById("cookieAccept");
    const declineBtn = document.getElementById("cookieDecline");
    if (!banner || !acceptBtn || !declineBtn) return;

    const KEY = "cookieConsent";

    function isStorageAvailable() {
        try {
            const testKey = "__test_ls__";
            window.localStorage.setItem(testKey, "1");
            window.localStorage.removeItem(testKey);
            return true;
        } catch {
            return false;
        }
    }

    function readCookie(name) {
        try {
            if (!document.cookie) return null;
            const parts = document.cookie.split("; ");
            for (let i = 0; i < parts.length; i++) {
                const part = parts[i];
                const eq = part.indexOf("=");
                if (eq === -1) continue;
                const key = decodeURIComponent(part.substring(0, eq));
                if (key === name) {
                    return decodeURIComponent(part.substring(eq + 1));
                }
            }
            return null;
        } catch {
            return null;
        }
    }

    function getConsent() {
        let raw = null;
        if (isStorageAvailable()) {
            try { raw = window.localStorage.getItem(KEY); } catch { raw = null; }
        }
        if (!raw) {
            raw = readCookie(KEY);
        }
        if (!raw) return null;
        try {
            const parsed = JSON.parse(raw);
            if (parsed && typeof parsed.analytics === "boolean") {
                return parsed;
            }
            return null;
        } catch {
            // legacy/non-JSON values: clear and fall back to null
            try { if (isStorageAvailable()) window.localStorage.removeItem(KEY); } catch { }
            return null;
        }
    }

    function setConsent(analytics) {
        const data = JSON.stringify({ analytics: !!analytics, ts: Date.now() });
        if (isStorageAvailable()) {
            try { window.localStorage.setItem(KEY, data); } catch { /* ignore */ }
        }
        try {
            const maxAge = 60 * 60 * 24 * 182; // ~6 months
            const secure = window.location.protocol === "https:" ? "; Secure" : "";
            document.cookie = KEY + "=" + encodeURIComponent(data) + "; path=/; max-age=" + maxAge + "; SameSite=Lax" + secure;
        } catch { /* ignore */ }
        try {
            window.dispatchEvent(new CustomEvent("cookieconsent", { detail: { analytics: !!analytics } }));
        } catch { /* ignore */ }
    }

    const stored = getConsent();
    if (!stored) {
        banner.classList.add("show");
    } else {
        window.__analyticsConsent = !!stored.analytics;
    }

    acceptBtn.addEventListener("click", function () {
        setConsent(true);
        banner.classList.remove("show");
        window.__analyticsConsent = true;
    });

    declineBtn.addEventListener("click", function () {
        setConsent(false);
        banner.classList.remove("show");
        window.__analyticsConsent = false;
    });
});

