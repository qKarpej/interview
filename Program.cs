using System;
using System.Threading.Tasks;
using WebPageAnalyzer.Services;

namespace WebPageAnalyzer
{
    /// Entry point for the Web Page Analyzer console application.
    /// The app accepts a URL from the user, fetches the page,
    /// and performs a series of analyses on it.
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Web Page Analyzer ===\n");

            // Step 1: Get URL from user (either from command-line args or interactive input)
            string url = GetUrl(args);

            // Step 2: Validate that the URL has a correct format before making any HTTP requests
            if (!UrlValidator.IsValidUrl(url))
            {
                Console.WriteLine($"Error: '{url}' is not a valid URL.");
                Console.WriteLine("Please provide a URL starting with http:// or https://");
                return;
            }

            // Step 3: Fetch the page HTML content
            Console.WriteLine($"Fetching: {url}\n");
            var fetcher = new PageFetcher();
            var fetchResult = await fetcher.FetchPageAsync(url);

            // If fetching failed (network error, 404, timeout, etc.), show the error and stop
            if (!fetchResult.Success)
            {
                Console.WriteLine($"Error: {fetchResult.ErrorMessage}");
                return;
            }

            // Step 4: Analyze the fetched HTML
            var analyzer = new PageAnalyzer();
            var report = analyzer.Analyze(url, fetchResult.Html!);

            // Step 5: Display the analysis results
            ReportPrinter.Print(report);
        }

        /// Gets the URL either from command-line arguments or by asking the user interactively.
        private static string GetUrl(string[] args)
        {
            if (args.Length > 0)
            {
                return args[0];
            }

            Console.Write("Enter a URL to analyze: ");
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }
    }
}
