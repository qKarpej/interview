using System;
using System.Linq;
using WebPageAnalyzer.Models;

namespace WebPageAnalyzer.Services
{
    /// Formats and prints the AnalysisReport to the console in a readable way.
    public static class ReportPrinter
    {
        public static void Print(AnalysisReport report)
        {
            PrintSection("PAGE INFO", () =>
            {
                PrintLine("URL", report.Url);
                PrintLine("Title", report.Title);
                PrintLine("Language", report.Language ?? "Not specified");
                PrintLine("Meta Description", report.MetaDescription ?? "Not found");
                PrintLine("Meta Keywords", report.MetaKeywords ?? "Not found");
            });

            PrintSection("CONTENT STATISTICS", () =>
            {
                PrintLine("Word Count", report.WordCount.ToString("N0"));
                PrintLine("Paragraphs", report.ParagraphCount.ToString());
                PrintLine("Images", report.ImageCount.ToString());
                PrintLine("Total Links", report.LinkCount.ToString());
                PrintLine("Internal Links", report.InternalLinkCount.ToString());
                PrintLine("External Links", report.ExternalLinkCount.ToString());
            });

            PrintSection("HEADING STRUCTURE", () =>
            {
                if (report.HeadingCounts.Count == 0)
                {
                    Console.WriteLine("  No headings found.");
                }
                else
                {
                    foreach (var heading in report.HeadingCounts)
                    {
                        PrintLine(heading.Key.ToUpper(), heading.Value.ToString());
                    }
                }
            });

            PrintSection("SEO & QUALITY", () =>
            {
                // Pass/Fail indicators for quick visual scanning
                PrintCheck("Has Title", report.HasTitle);
                PrintCheck("Has Meta Description", report.HasMetaDescription);
                PrintCheck("Has Favicon", report.HasFavicon);
                PrintCheck("Has Viewport (Mobile)", report.HasViewport);
                PrintLine("Images Without Alt", report.ImagesWithoutAlt.ToString());
            });

            PrintSection("TOP 10 WORDS", () =>
            {
                if (report.TopWords.Count == 0)
                {
                    Console.WriteLine("  No words found.");
                }
                else
                {
                    int rank = 1;
                    foreach (var (word, count) in report.TopWords)
                    {
                        Console.WriteLine($"  {rank,-4} {word,-20} {count} occurrences");
                        rank++;
                    }
                }
            });
        }

        /// Prints a section with a header and separator lines.
        private static void PrintSection(string title, Action content)
        {
            Console.WriteLine($"\n--- {title} ---");
            content();
        }

        /// Prints a single key-value line with consistent formatting.
        private static void PrintLine(string label, string value)
        {
            Console.WriteLine($"  {label.PadRight(22)} {value}");
        }

        /// Prints a pass/fail indicator with a checkmark or cross.
        private static void PrintCheck(string label, bool passed)
        {
            // Ternary operator: condition ? valueIfTrue : valueIfFalse
            string status = passed ? "[PASS]" : "[FAIL]";
            Console.WriteLine($"  {label.PadRight(22)} {status}");
        }
    }
}
