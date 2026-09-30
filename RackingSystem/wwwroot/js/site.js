// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// AdminLTE's accessibility script gives <main> tabindex="-1" so its "Skip to
// main content" link can focus it programmatically. But tabindex="-1" also
// makes <main> the nearest focusable ancestor when a user clicks any
// non-focusable element inside it (e.g. a plain <div onclick="...">), so
// <main> silently becomes document.activeElement on every such click. When
// that click later opens a SweetAlert2 popup, SweetAlert2's built-in
// focus-restore behavior re-focuses <main> when the popup closes, and the
// browser's default "scroll newly-focused element into view" then jumps the
// page's scroll position.
//
// Only block the fallback focus from landing on <main> itself - if the
// actual click target is (or is inside) a real focusable control, let the
// browser focus it normally. mousedown bubbles from the real target up to
// <main>, so checking e.target here (not just calling preventDefault
// unconditionally) is what keeps every input/select/button/link on every
// page clickable/focusable as usual.
document.addEventListener('DOMContentLoaded', function () {
    var main = document.querySelector('main');
    if (main) {
        main.addEventListener('mousedown', function (e) {
            var isNativelyFocusable = e.target.closest(
                'a[href], button, input, select, textarea, [tabindex], [contenteditable="true"]'
            );
            if (!isNativelyFocusable) {
                e.preventDefault();
            }
        });
    }
});
