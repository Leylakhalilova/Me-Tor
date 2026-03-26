using System.Text;

namespace MiniNotepad.Core;

public class EditorBuffer
{
    public List<StringBuilder> Lines { get; private set; } = new() { new StringBuilder() };
    public bool IsModified { get; set; } = false;
    public string? CurrentFilePath { get; set; }

    public void Load(IEnumerable<string> content, string? path)
    {
        Lines.Clear();
        foreach (var line in content)
        {
            Lines.Add(new StringBuilder(line));
        }
        if (Lines.Count == 0) Lines.Add(new StringBuilder());
        CurrentFilePath = path;
        IsModified = false;
    }

    public List<string> GetLines()
    {
        return Lines.Select(sb => sb.ToString()).ToList();
    }

    public void Clear()
    {
        Lines.Clear();
        Lines.Add(new StringBuilder());
        CurrentFilePath = null;
        IsModified = false;
    }

    public void InsertChar(int x, int y, char c)
    {
        Lines[y].Insert(x, c);
        IsModified = true;
    }

    public void DeleteChar(int x, int y)
    {
        if (x > 0)
        {
            Lines[y].Remove(x - 1, 1);
            IsModified = true;
        }
        else if (y > 0)
        {
            // Backspace at start of line: merge with previous line
            var currentLine = Lines[y].ToString();
            var prevLineLength = Lines[y - 1].Length;
            Lines[y - 1].Append(currentLine);
            Lines.RemoveAt(y);
            IsModified = true;
        }
    }

    public void NewLine(int x, int y)
    {
        var currentLine = Lines[y].ToString();
        var leftPart = currentLine.Substring(0, x);
        var rightPart = currentLine.Substring(x);

        Lines[y] = new StringBuilder(leftPart);
        Lines.Insert(y + 1, new StringBuilder(rightPart));
        IsModified = true;
    }
}
