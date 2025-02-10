using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WebApplication1.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public int StatusCode { get; set; } // New property for status code
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode; // Set the status code
            LogError(statusCode); // Log the error for debugging
        }

        private void LogError(int statusCode)
        {
            // Log the error based on the status code
            switch (statusCode)
            {
                case 404:
                    _logger.LogWarning("404 Not Found: Request ID {RequestId}", RequestId);
                    break;
                case 403:
                    _logger.LogWarning("403 Forbidden: Request ID {RequestId}", RequestId);
                    break;
                case 500:
                    _logger.LogError("500 Internal Server Error: Request ID {RequestId}", RequestId);
                    break;
                default:
                    _logger.LogError("Unexpected error: {StatusCode} - Request ID {RequestId}", statusCode, RequestId);
                    break;
            }
        }
    }
}