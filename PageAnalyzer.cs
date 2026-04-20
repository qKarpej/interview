using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WebPageAnalyzer.Models;

namespace WebPageAnalyzer.Services
{
    /// The core of the application - parses HTML and extracts useful information.
    public class PageAnalyzer
    {
        /// Analyzes the given HTML string and returns a filled AnalysisReport.
        public AnalysisReport Analyze(string url, string html)
        {
            // Parse the raw HTML string into a navigable document tree
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var report = new AnalysisReport { Url = url };

            // Run each analysis step, filling in the report
            AnalyzeMetadata(doc, report);
            AnalyzeContent(doc, report);
            AnalyzeLinks(doc, url, report);
            AnalyzeHeadings(doc, report);
            AnalyzeSeoQuality(doc, report);
            AnalyzeTopWords(doc, report);

            return report;
        }

        /// Extracts metadata from the <head> section: title, description, keywords, language.
        private void AnalyzeMetadata(HtmlDocument doc, AnalysisReport report)
        {
            // Get the <title> tag text.
            report.Title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? "No title found";

            // Meta description — used by search engines as the snippet under the page title.
            report.MetaDescription = GetMetaContent(doc, "description");
            report.MetaKeywords = GetMetaContent(doc, "keywords");

            // Language can be in <html lang="en"> attribute
            report.Language = doc.DocumentNode.SelectSingleNode("//html")
                ?.GetAttributeValue("lang", null);
        }
        /// Counts paragraphs, images, and calculates word count from visible text.
        private void AnalyzeContent(HtmlDocument doc, AnalysisReport report)
        {
            // Count all <p> tags
            report.ParagraphCount = doc.DocumentNode.SelectNodes("//p")?.Count ?? 0;

            // Count all <img> tags
            report.ImageCount = doc.DocumentNode.SelectNodes("//img")?.Count ?? 0;

            // Extract visible text (strip all HTML tags) and count words.
            // InnerText gives us all text content with tags removed.
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                // HtmlEntity.DeEntitize converts HTML entities (&amp; → &, &nbsp; → space, etc.)
                var text = HtmlEntity.DeEntitize(bodyNode.InnerText);

                // Split text into words using regex.
                // \w+ matches sequences of word characters (letters, digits, underscore).
                var words = Regex.Matches(text, @"\w+");
                report.WordCount = words.Count;
            }
        }
        /// Analyzes all links on the page — counts total, internal vs external.
        /// Internal links point to the same domain, external links go elsewhere.
        private void AnalyzeLinks(HtmlDocument doc, string pageUrl, AnalysisReport report)
        {
            // Find all <a> tags that have an href attribute
            var links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links == null)
            {
                report.LinkCount = 0;
                return;
            }

            report.LinkCount = links.Count;

            // Parse the page URL to get its host (domain) for comparison.
            var pageUri = new Uri(pageUrl);

            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");

                // Try to parse each link. Some might be relative ("/about"),
                // some absolute ("https://other.com/page").
                // Uri.TryCreate with the page URL as base resolves relative URLs.
                if (Uri.TryCreate(pageUri, href, out var linkUri))
                {
                    // Compare hosts (domains) to determine internal vs external.
                    // StringComparison.OrdinalIgnoreCase makes it case-insensitive.
                    if (linkUri.Host.Equals(pageUri.Host, StringComparison.OrdinalIgnoreCase))
                        report.InternalLinkCount++;
                    else
                        report.ExternalLinkCount++;
                }
            }
        }
        /// Counts heading tags (h1 through h6) to understand the page's content structure.
        private void AnalyzeHeadings(HtmlDocument doc, AnalysisReport report)
        {
            // Check each heading level h1 through h6
            for (int i = 1; i <= 6; i++)
            {
                // $"//h{i}" creates XPath like "//h1", "//h2", etc.
                // $ before a string in C# is string interpolation (like Kotlin's "$variable").
                var headings = doc.DocumentNode.SelectNodes($"//h{i}");
                int count = headings?.Count ?? 0;

                // Only include heading levels that exist on the page
                if (count > 0)
                {
                    report.HeadingCounts[$"h{i}"] = count;
                }
            }
        }
        /// Checks common SEO and quality indicators:
        /// - Does the page have a title tag?
        /// - Does it have a meta description?
        /// - Does it have a favicon (the small icon in the browser tab)?
        /// - Does it have a viewport meta tag (needed for mobile responsiveness)?
        /// - How many images are missing alt text (bad for accessibility)?
        private void AnalyzeSeoQuality(HtmlDocument doc, AnalysisReport report)
        {
            report.HasTitle = doc.DocumentNode.SelectSingleNode("//title") != null;
            report.HasMetaDescription = GetMetaContent(doc, "description") != null;

            // Favicon can be a <link rel="icon"> or <link rel="shortcut icon">
            // The XPath contains() function checks if the rel attribute contains "icon"
            report.HasFavicon = doc.DocumentNode
                .SelectSingleNode("//link[contains(@rel, 'icon')]") != null;

            // Viewport meta tag tells mobile browsers how to scale the page
            report.HasViewport = doc.DocumentNode
                .SelectSingleNode("//meta[@name='viewport']") != null;

            // Images without alt text are bad for accessibility (screen readers)
            // and SEO. We find all <img> tags and count those missing the alt attribute.
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                report.ImagesWithoutAlt = images.Count(img =>
                    string.IsNullOrWhiteSpace(img.GetAttributeValue("alt", "")));
            }
        }

        /// Finds the most frequently used words on the page.
        /// Filters out common stop words (the, and, is, etc.) and short words.
        private void AnalyzeTopWords(HtmlDocument doc, AnalysisReport report)
        {
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode == null) return;

            var text = HtmlEntity.DeEntitize(bodyNode.InnerText).ToLower();

            // Common English stop words to exclude (these appear everywhere and aren't informative)
            var stopWords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to",
                "for", "of", "with", "by", "is", "are", "was", "were", "be",
                "have", "has", "had", "do", "does", "did", "will", "would",
                "could", "should", "may", "might", "this", "that", "these",
                "those", "it", "its", "not", "no", "from", "as", "if", "all",
                "can", "you", "your", "we", "our", "they", "their", "what",
                "which", "who", "how", "more", "about", "up", "out", "so"
            };

            // Extract words, filter, group, count, and take top 10.
            report.TopWords = Regex.Matches(text, @"\w+")
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(w => w.Length > 3 && !stopWords.Contains(w))
                .GroupBy(w => w)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => (Word: g.Key, Count: g.Count()))
                .ToList();
        }
        /// Helper method to extract the "content" attribute from a meta tag by its "name".
        private string? GetMetaContent(HtmlDocument doc, string name)
        {
            // XPath: find <meta> where name attribute matches (case-insensitive via translate)
            // translate() converts to lowercase for comparison, since HTML is case-insensitive
            var node = doc.DocumentNode.SelectSingleNode(
                $"//meta[translate(@name,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{name}']");

            return node?.GetAttributeValue("content", null);
        }
    }
}
