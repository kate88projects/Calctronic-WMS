// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sends an error caught on the page to the server log (C:\RackingSystemLogs), with the real message and stack.
function reportClientError(source, error) {
    try {
        const body = new URLSearchParams({
            source: source,
            message: error && error.message ? error.message : String(error),
            stack: error && error.stack ? error.stack : '',
            page: window.location.pathname
        });
        fetch('/Log/ClientError', { method: 'POST', body: body, credentials: 'same-origin' }).catch(function () { });
    }
    catch (e) { }
}
