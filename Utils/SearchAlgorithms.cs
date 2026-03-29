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
            int index = line.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase);
            while (index != -1)
            {
                matches.Add(new SearchMatch(i, index, searchTerm.Length));
                index = line.IndexOf(searchTerm, index + searchTerm.Length, StringComparison.OrdinalIgnoreCase);
            }
        }
        return matches;
    }
}
