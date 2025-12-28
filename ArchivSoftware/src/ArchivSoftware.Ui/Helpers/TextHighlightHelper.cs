using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ArchivSoftware.Ui.Helpers;

/// <summary>
/// Hilfsklasse für das Hervorheben von Suchbegriffen in Text.
/// </summary>
public static class TextHighlightHelper
{
    /// <summary>
    /// Erstellt Inline-Elemente mit hervorgehobenen Suchtreffern.
    /// </summary>
    /// <param name="text">Der Text, in dem gesucht wird.</param>
    /// <param name="term">Der Suchbegriff.</param>
    /// <returns>Inline-Elemente mit hervorgehobenen Treffern.</returns>
    public static IEnumerable<Inline> BuildHighlightedInlines(string text, string term)
    {
        // Wenn kein Text, leere Liste
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        // Wenn kein Suchbegriff, gesamten Text als normalen Run zurückgeben
        if (string.IsNullOrEmpty(term))
        {
            yield return new Run(text);
            yield break;
        }

        var currentIndex = 0;
        var searchIndex = 0;

        while (currentIndex < text.Length)
        {
            // Finde nächstes Vorkommen (case-insensitive)
            searchIndex = text.IndexOf(term, currentIndex, StringComparison.OrdinalIgnoreCase);

            if (searchIndex < 0)
            {
                // Kein weiterer Treffer - Rest des Textes als normalen Run
                yield return new Run(text.Substring(currentIndex));
                break;
            }

            // Text vor dem Treffer als normaler Run
            if (searchIndex > currentIndex)
            {
                yield return new Run(text.Substring(currentIndex, searchIndex - currentIndex));
            }

            // Treffer als hervorgehobener Run
            var matchedText = text.Substring(searchIndex, term.Length);
            yield return new Run(matchedText)
            {
                Background = new SolidColorBrush(Colors.Yellow),
                FontWeight = System.Windows.FontWeights.SemiBold
            };

            currentIndex = searchIndex + term.Length;
        }
    }

    /// <summary>
    /// Erstellt Inline-Elemente mit hervorgehobenen Suchtreffern (Systemfarbe).
    /// </summary>
    /// <param name="text">Der Text, in dem gesucht wird.</param>
    /// <param name="term">Der Suchbegriff.</param>
    /// <returns>Inline-Elemente mit hervorgehobenen Treffern.</returns>
    public static IEnumerable<Inline> BuildHighlightedInlinesSystemColor(string text, string term)
    {
        // Wenn kein Text, leere Liste
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        // Wenn kein Suchbegriff, gesamten Text als normalen Run zurückgeben
        if (string.IsNullOrEmpty(term))
        {
            yield return new Run(text);
            yield break;
        }

        var currentIndex = 0;
        var searchIndex = 0;

        while (currentIndex < text.Length)
        {
            // Finde nächstes Vorkommen (case-insensitive)
            searchIndex = text.IndexOf(term, currentIndex, StringComparison.OrdinalIgnoreCase);

            if (searchIndex < 0)
            {
                // Kein weiterer Treffer - Rest des Textes als normalen Run
                yield return new Run(text.Substring(currentIndex));
                break;
            }

            // Text vor dem Treffer als normaler Run
            if (searchIndex > currentIndex)
            {
                yield return new Run(text.Substring(currentIndex, searchIndex - currentIndex));
            }

            // Treffer als hervorgehobener Run (Systemfarben)
            var matchedText = text.Substring(searchIndex, term.Length);
            yield return new Run(matchedText)
            {
                Background = SystemColors.HighlightBrush,
                Foreground = SystemColors.HighlightTextBrush
            };

            currentIndex = searchIndex + term.Length;
        }
    }
}
