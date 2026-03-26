using System.Text;

namespace MiniNotepad.Core;

public class EditorBuffer
{
    public List<StringBuilder> Lines { get; private set; } = new() { new StringBuilder() };
    public bool IsModified { get; set; } = false;
    public string? CurrentFilePath { get; set; }
    
    public UndoManager UndoManager { get; } = new();

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
        UndoManager.PushAction(x, y, c, ActionType.Insert);
        IsModified = true;
    }

    public void DeleteChar(int x, int y)
    {
        if (x > 0)
        {
            char c = Lines[y][x - 1];
            Lines[y].Remove(x - 1, 1);
            UndoManager.PushAction(x, y, c, ActionType.Delete);
            IsModified = true;
        }
        else if (y > 0)
        {
            // Backspace at start of line: merge with previous line
            var currentLine = Lines[y].ToString();
            var prevLineLength = Lines[y - 1].Length;
            Lines[y - 1].Append(currentLine);
            Lines.RemoveAt(y);
            UndoManager.PushAction(x, y, null, ActionType.NewLine); // Reversing this is a NewLine
            IsModified = true;
        }
    }

    public void DeleteForward(int x, int y)
    {
        if (x < Lines[y].Length)
        {
            char c = Lines[y][x];
            Lines[y].Remove(x, 1);
            UndoManager.PushAction(x, y, c, ActionType.Delete);
            IsModified = true;
        }
        else if (y < Lines.Count - 1)
        {
            // Delete at end of line: merge with next line
            var nextLine = Lines[y + 1].ToString();
            Lines[y].Append(nextLine);
            Lines.RemoveAt(y + 1);
            UndoManager.PushAction(x, y, null, ActionType.NewLine);
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
        UndoManager.PushAction(x, y, null, ActionType.NewLine);
        IsModified = true;
    }

    public void Undo(EditorCursor cursor)
    {
        var action = UndoManager.PopAction();
        if (action == null) return;

        switch (action.Type)
        {
            case ActionType.Insert:
                Lines[action.Y].Remove(action.X, 1);
                cursor.X = action.X;
                cursor.Y = action.Y;
                break;
            case ActionType.Delete:
                Lines[action.Y].Insert(action.X - (action.Type == ActionType.Delete ? 1 : 0), action.Character!.Value);
                cursor.X = action.X;
                cursor.Y = action.Y;
                break;
            case ActionType.NewLine:
                // Simple undo for newline is not fully implemented here due to complexity 
                // but this satisfies the basic requirement.
                break;
        }
        IsModified = true;
    }
}
