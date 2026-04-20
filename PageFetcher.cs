using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebPageAnalyzer.Models;

namespace WebPageAnalyzer.Services
    /// Responsible for fetching a web page's HTML content via HTTP.
    /// Handles all the things that can go wrong: timeouts, DNS failures,
    /// HTTP error codes (404, 500, etc.), and unexpected exceptions.
    public class PageFetcher
    {
        // HttpClient is the .NET equivalent of Java's HttpClient / OkHttp.
        // We create one instance and reuse it (best practice in .NET).
        private readonly HttpClient _httpClient;

        public PageFetcher()
        {
            _httpClient = new HttpClient
            {
                // Set a 15-second timeout so we don't hang forever on slow/dead sites
                Timeout = TimeSpan.FromSeconds(15)
            };

            // Set a User-Agent header so websites don't reject us as a bot.
            // Some sites return 403 Forbidden without a proper User-Agent.
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "WebPageAnalyzer/1.0 (Student Project)");
        }

        /// <summary>
        /// Fetches the HTML content of the given URL.
        /// Returns a FetchResult with either the HTML or an error message.
        /// 
        /// "async Task<FetchResult>" means:
        /// - async: this method uses await internally
        /// - Task<FetchResult>: it returns a FetchResult, but asynchronously
        /// </summary>
        public async Task<FetchResult> FetchPageAsync(string url)
        {
            try
            {
                // Send GET request and wait for the response.
                // "await" pauses this method until the response arrives,
                // without blocking the thread (similar to Kotlin's suspend).
                var response = await _httpClient.GetAsync(url);

                // Check if HTTP status code indicates success (200-299).
                // If not (e.g., 404 Not Found, 500 Server Error), return failure.
                if (!response.IsSuccessStatusCode)
                {
                    return FetchResult.Fail(
                        $"HTTP error {(int)response.StatusCode} ({response.ReasonPhrase})");
                }

                // Read the response body as a string (the HTML content)
                var html = await response.Content.ReadAsStringAsync();
                return FetchResult.Ok(html);
            }
            catch (TaskCanceledException)
            {
                // This exception is thrown when the request exceeds our timeout
                return FetchResult.Fail("Request timed out after 15 seconds.");
            }
            catch (HttpRequestException ex)
            {
                // Covers network-level errors: DNS resolution failure,
                // connection refused, SSL certificate problems, etc.
                return FetchResult.Fail($"Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Catch-all for anything unexpected
                return FetchResult.Fail($"Unexpected error: {ex.Message}");
            }
        }
    }
}
