// Si l'utilisateur utilise retour arrière, revérifier le token côté serveur
window.addEventListener('pageshow', async function (event) {
	try {
		const nav = performance.getEntriesByType('navigation')[0];
		const isBF = (event.persisted || (nav && nav.type === 'back_forward'));
		if (isBF) {
			const res = await fetch('/api/Admin/VerifyToken', { method: 'POST' });
			if (!res.ok || res.redirected || (res.url && res.url.includes('/Home/Index'))) {
				window.location.replace('/Home/Index');
			}
		}
	} catch {
		window.location.replace('/Home/Index');
	}
});
