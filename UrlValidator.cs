using System;

namespace WebPageAnalyzer.Services
{
    /// Validates URL format before we attempt to fetch it.
    public static class UrlValidator
    {
        /// Checks if the given string is a valid HTTP or HTTPS URL.
        public static bool IsValidUrl(string url)
        {
            // string.IsNullOrWhiteSpace checks for null, empty, or only whitespace
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Uri.TryCreate attempts to parse the string as a URI.
            bool parsed = Uri.TryCreate(url, UriKind.Absolute, out var uri);

            // Only allow http and https schemes (not ftp, mailto, etc.)
            return parsed && (uri!.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
