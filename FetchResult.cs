namespace WebPageAnalyzer.Models
{
    /// Wraps the result of fetching a web page.
    public class FetchResult
    {
        ///True if the page was fetched successfully.
        public bool Success { get; set; }

        ///The raw HTML string of the page (null if fetch failed).
        public string? Html { get; set; }

        /// Error description if fetch failed (null if successful).
        public string? ErrorMessage { get; set; }
        public static FetchResult Ok(string html) =>
            new FetchResult { Success = true, Html = html };

        public static FetchResult Fail(string errorMessage) =>
            new FetchResult { Success = false, ErrorMessage = errorMessage };
    }
}
