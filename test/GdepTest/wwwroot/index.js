(function() {
    window.addEventListener('load', async () => {
        const res = await fetch('/rest/account/external-login', { method: 'get' });
        const result = await res.json()
        var html = [];
        for (const oauth of result) {
            html.push('<button type="submit" name="Provider" value="' + oauth.AuthenticationType + '">'
                + oauth.Caption
            + '</button>');
        }
        document.getElementById('oauth').innerHTML = html.join('<br />');
    });
})();
