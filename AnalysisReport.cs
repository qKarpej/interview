using System.Collections.Generic;

namespace WebPageAnalyzer.Models
{
    /// Holds all the analysis results for a single web page.
    public class AnalysisReport
    {
        // --- Basic page info ---
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? Language { get; set; }

        // --- Content statistics ---
        public int WordCount { get; set; }
        public int ParagraphCount { get; set; }
        public int ImageCount { get; set; }
        public int LinkCount { get; set; }
        public int InternalLinkCount { get; set; }
        public int ExternalLinkCount { get; set; }

        // --- Heading structure ---
        public Dictionary<string, int> HeadingCounts { get; set; } = new Dictionary<string, int>();

        // --- SEO / Quality checks ---
        public bool HasTitle { get; set; }
        public bool HasMetaDescription { get; set; }
        public bool HasFavicon { get; set; }
        public bool HasViewport { get; set; }
        public int ImagesWithoutAlt { get; set; }

        // --- Top words found on the page ---
        // List of tuples: (word, frequency).
        public List<(string Word, int Count)> TopWords { get; set; } = new List<(string, int)>();
    }
}
