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
        UndoManager.PushAction(x, y, c.ToString(), ActionType.InsertChar);
        IsModified = true;
    }

    public void DeleteChar(int x, int y)
    {
        if (x > 0)
        {
            char c = Lines[y][x - 1];
            Lines[y].Remove(x - 1, 1);
            UndoManager.PushAction(x - 1, y, c.ToString(), ActionType.DeleteChar);
            IsModified = true;
        }
        else if (y > 0)
        {
            // Backspace at start of line: merge with previous line
            var currentLine = Lines[y].ToString();
            var prevLineLength = Lines[y - 1].Length;
            Lines[y - 1].Append(currentLine);
            Lines.RemoveAt(y);
            UndoManager.PushAction(prevLineLength, y - 1, null, ActionType.MergeLines);
            IsModified = true;
        }
    }

    public void DeleteForward(int x, int y)
    {
        if (x < Lines[y].Length)
        {
            char c = Lines[y][x];
            Lines[y].Remove(x, 1);
            UndoManager.PushAction(x, y, c.ToString(), ActionType.DeleteChar);
            IsModified = true;
        }
        else if (y < Lines.Count - 1)
        {
            // Delete at end of line: merge with next line
            var nextLine = Lines[y + 1].ToString();
            Lines[y].Append(nextLine);
            Lines.RemoveAt(y + 1);
            UndoManager.PushAction(x, y, null, ActionType.MergeLines);
            IsModified = true;
        }
    }

    public void DeleteWord(EditorCursor cursor)
    {
        if (cursor.X == 0)
        {
            if (cursor.Y > 0)
            {
                int oldX = cursor.X;
                int oldY = cursor.Y;
                // Move cursor to end of previous line
                cursor.Y--;
                cursor.X = Lines[cursor.Y].Length;
                DeleteChar(oldX, oldY);
            }
            return;
        }

        int endX = cursor.X;
        int startX = cursor.X;
        string line = Lines[cursor.Y].ToString();

        // 1. Skip contiguous non-whitespace (the word)
        while (startX > 0 && !char.IsWhiteSpace(line[startX - 1]))
        {
            startX--;
        }

        // 2. Skip contiguous whitespace
        while (startX > 0 && char.IsWhiteSpace(line[startX - 1]))
        {
            startX--;
        }
        
        // If startX is still endX (e.g. cursor was at start of whitespace sequence), 
        // we already decremented startX in step 1.
        
        if (startX == endX && endX > 0)
        {
             startX = endX - 1;
        }

        string deletedText = line.Substring(startX, endX - startX);
        Lines[cursor.Y].Remove(startX, endX - startX);
        UndoManager.PushAction(startX, cursor.Y, deletedText, ActionType.DeleteWord);
        cursor.X = startX;
        IsModified = true;
    }

    public void NewLine(int x, int y)
    {
        var currentLine = Lines[y].ToString();
        var leftPart = currentLine.Substring(0, x);
        var rightPart = currentLine.Substring(x);

        Lines[y] = new StringBuilder(leftPart);
        Lines.Insert(y + 1, new StringBuilder(rightPart));
        UndoManager.PushAction(x, y, null, ActionType.SplitLine);
        IsModified = true;
    }

    public void Undo(EditorCursor cursor)
    {
        var action = UndoManager.PopAction();
        if (action == null) return;

        switch (action.Type)
        {
            case ActionType.InsertChar:
                Lines[action.Y].Remove(action.X, action.Text!.Length);
                cursor.X = action.X;
                cursor.Y = action.Y;
                break;
            case ActionType.DeleteChar:
            case ActionType.DeleteWord:
                Lines[action.Y].Insert(action.X, action.Text!);
                cursor.X = action.X + action.Text!.Length;
                cursor.Y = action.Y;
                break;
            case ActionType.SplitLine:
                // Undo split line: merge line Y and Y+1
                var lineToMerge = Lines[action.Y + 1].ToString();
                Lines[action.Y].Append(lineToMerge);
                Lines.RemoveAt(action.Y + 1);
                cursor.X = action.X;
                cursor.Y = action.Y;
                break;
            case ActionType.MergeLines:
                // Undo merge lines: split line Y at X
                var lineToSplit = Lines[action.Y].ToString();
                var left = lineToSplit.Substring(0, action.X);
                var right = lineToSplit.Substring(action.X);
                Lines[action.Y] = new StringBuilder(left);
                Lines.Insert(action.Y + 1, new StringBuilder(right));
                cursor.X = action.X;
                cursor.Y = action.Y; // Move back to where the split happened
                break;
        }
        IsModified = true;
    }
}
