using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace RackingSystem.Controllers
{
    [Authorize(AuthenticationSchemes = "MyAuthCookie")]
    public class LogController : Controller
    {
        private static readonly object Gate = new object();

        private readonly ILogger<LogController> _logger;
        private readonly IMemoryCache _cache;

        public LogController(ILogger<LogController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpPost]
        public IActionResult ClientError([FromForm] string? source, [FromForm] string? message, [FromForm] string? stack, [FromForm] string? page)
        {
            source = OneLine(source, 100);
            message = OneLine(message, 500);
            page = OneLine(page, 200);
            stack = Cut(stack, 2000);

            // the same error is written once a minute, so a repeated click cannot fill the file
            string key = $"clienterror|{source}|{page}|{message}";
            lock (Gate)
            {
                if (_cache.TryGetValue(key, out _))
                {
                    return Ok();
                }
                _cache.Set(key, true, TimeSpan.FromMinutes(1));
            }

            string user = User.Identity?.Name ?? "unknown";
            string agent = OneLine(Request.Headers.UserAgent.ToString(), 200);
            string detail = stack == "" ? "" : Environment.NewLine + stack;
            _logger.LogError("ClientError [{Source}] page={Page} user={User} browser={Agent}: {Message}{Detail}",
                source, page, user, agent, message, detail);

            return Ok();
        }

        private static string Cut(string? value, int max)
        {
            value ??= "";
            return value.Length > max ? value.Substring(0, max) : value;
        }

        // no line breaks, so one report cannot look like several log lines
        private static string OneLine(string? value, int max)
        {
            return Cut(value, max).Replace("\r", " ").Replace("\n", " ");
        }
    }
}
