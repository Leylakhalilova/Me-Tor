using System.Text;

namespace MiniNotepad.Utils;

public struct SearchMatch
{
    public int LineIndex { get; set; }
    public int ColumnIndex { get; set; }
    public int Length { get; set; }

    public SearchMatch(int lineIndex, int columnIndex, int length)
    {
        LineIndex = lineIndex;
        ColumnIndex = columnIndex;
        Length = length;
    }
}

public static class SearchAlgorithms
{
    public static List<SearchMatch> FindAll(List<StringBuilder> lines, string searchTerm)
    {
        var matches = new List<SearchMatch>();
        if (string.IsNullOrEmpty(searchTerm)) return matches;

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i].ToString();
            string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(searchTerm) + @"\b";
            var matchesInLine = System.Text.RegularExpressions.Regex.Matches(
                line, 
                pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            foreach (System.Text.RegularExpressions.Match match in matchesInLine)
            {
                matches.Add(new SearchMatch(i, match.Index, match.Length));
            }
        }
        return matches;
    }
}
