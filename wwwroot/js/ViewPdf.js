document.addEventListener("DOMContentLoaded", function () {
    const modalElement = document.getElementById('languageModal');
    if (typeof bootstrap === "undefined") {
        return;
    }
    if (modalElement) {
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    }
    document.querySelectorAll('.language-btn').forEach(button => {
        button.addEventListener('click', function () {
            const lang = this.getAttribute('data-lang');
            window.location.href = `/File/ViewPdf?lang=${lang}`;
        });
    });
    const changeLangBtn = document.getElementById('changeLanguageBtn');
    if (changeLangBtn) {
        changeLangBtn.addEventListener('click', function () {
            const modal = new bootstrap.Modal(document.getElementById('languageModal'));
            modal.show();
        });
    }
});

