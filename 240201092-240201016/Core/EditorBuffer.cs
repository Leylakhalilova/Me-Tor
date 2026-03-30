using System.Text;
using MiniNotepad.Utils;

namespace MiniNotepad.Core;

public class EditorBuffer
{
    public List<StringBuilder> Lines { get; private set; } = new() { new StringBuilder() };
    public bool IsModified { get; set; } = false;
    public string? CurrentFilePath { get; set; }
    
    public UndoManager UndoManager { get; } = new();

    // Search results management
    public List<SearchMatch> Matches { get; private set; } = new();
    public int CurrentMatchIndex { get; set; } = -1;
    public string? SearchTerm { get; set; }

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
        ClearSearch();
    }

    public void ClearSearch()
    {
        Matches.Clear();
        CurrentMatchIndex = -1;
        SearchTerm = null;
    }

    public void UpdateSearch(string term, bool wholeWord = false)
    {
        SearchTerm = term;
        Matches = SearchAlgorithms.FindAll(Lines, term, wholeWord);
        if (Matches.Count > 0) CurrentMatchIndex = 0;
        else CurrentMatchIndex = -1;
    }

    public void ReplaceAll(string oldText, string newText)
    {
        if (string.IsNullOrEmpty(oldText)) return;

        var previousState = GetLines();
        bool replaced = false;

        for (int i = 0; i < Lines.Count; i++)
        {
            string line = Lines[i].ToString();
            string pattern = @"\b" + System.Text.RegularExpressions.Regex.Escape(oldText) + @"\b";
            if (System.Text.RegularExpressions.Regex.IsMatch(line, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                // Case-insensitive whole word replacement
                string replacedLine = System.Text.RegularExpressions.Regex.Replace(
                    line, 
                    pattern, 
                    newText, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
                
                if (line != replacedLine)
                {
                    Lines[i] = new StringBuilder(replacedLine);
                    replaced = true;
                }
            }
        }

        if (replaced)
        {
            UndoManager.PushAction(0, 0, null, ActionType.ReplaceAll, previousState);
            IsModified = true;
            ClearSearch();
        }
    }

    public List<string> GetLines()
    {
        return Lines.Select(sb => sb.ToString()).ToList();
    }

    public string GetSelectedText(EditorCursor cursor)
    {
        if (!cursor.IsSelecting) return "";

        int startY = cursor.SelectionStartY;
        int startX = cursor.SelectionStartX;
        int endY = cursor.Y;
        int endX = cursor.X;

        if (startY > endY || (startY == endY && startX > endX))
        {
            (startY, endY) = (endY, startY);
            (startX, endX) = (endX, startX);
        }

        StringBuilder sb = new StringBuilder();
        for (int i = startY; i <= endY; i++)
        {
            string line = Lines[i].ToString();
            int s = (i == startY) ? startX : 0;
            int e = (i == endY) ? endX : line.Length;
            
            if (e > s)
            {
                sb.Append(line.Substring(s, e - s));
            }
            if (i < endY)
            {
                sb.Append(Environment.NewLine);
            }
        }
        return sb.ToString();
    }

    public void DeleteSelection(EditorCursor cursor)
    {
        if (!cursor.IsSelecting) return;

        var previousState = GetLines();
        string deletedText = GetSelectedText(cursor);

        int startY = cursor.SelectionStartY;
        int startX = cursor.SelectionStartX;
        int endY = cursor.Y;
        int endX = cursor.X;

        if (startY > endY || (startY == endY && startX > endX))
        {
            (startY, endY) = (endY, startY);
            (startX, endX) = (endX, startX);
        }

        // Perform deletion
        if (startY == endY)
        {
            Lines[startY].Remove(startX, endX - startX);
        }
        else
        {
            string firstLineStart = Lines[startY].ToString().Substring(0, startX);
            string lastLineEnd = Lines[endY].ToString().Substring(endX);
            
            Lines[startY] = new StringBuilder(firstLineStart + lastLineEnd);
            
            // Remove lines in between and the end line
            Lines.RemoveRange(startY + 1, endY - startY);
        }

        UndoManager.PushAction(startX, startY, deletedText, ActionType.DeleteSelection, previousState);
        cursor.Y = startY;
        cursor.X = startX;
        cursor.ClearSelection();
        IsModified = true;
        ClearSearch();
    }

    public void InsertText(EditorCursor cursor, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var previousState = GetLines();
        
        string[] newLines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        
        string currentLine = Lines[cursor.Y].ToString();
        string leftPart = currentLine.Substring(0, cursor.X);
        string rightPart = currentLine.Substring(cursor.X);

        if (newLines.Length == 1)
        {
            Lines[cursor.Y].Insert(cursor.X, newLines[0]);
            cursor.X += newLines[0].Length;
        }
        else
        {
            Lines[cursor.Y] = new StringBuilder(leftPart + newLines[0]);
            for (int i = 1; i < newLines.Length - 1; i++)
            {
                Lines.Insert(cursor.Y + i, new StringBuilder(newLines[i]));
            }
            string lastPart = newLines[newLines.Length - 1] + rightPart;
            Lines.Insert(cursor.Y + newLines.Length - 1, new StringBuilder(lastPart));
            
            cursor.Y += newLines.Length - 1;
            cursor.X = newLines[newLines.Length - 1].Length;
        }

        UndoManager.PushAction(cursor.X, cursor.Y, text, ActionType.Paste, previousState);
        IsModified = true;
        ClearSearch();
    }

    public void Clear()
    {
        Lines.Clear();
        Lines.Add(new StringBuilder());
        CurrentFilePath = null;
        IsModified = false;
        ClearSearch();
    }

    public void InsertChar(int x, int y, char c)
    {
        Lines[y].Insert(x, c);
        UndoManager.PushAction(x, y, c.ToString(), ActionType.InsertChar);
        IsModified = true;
        ClearSearch();
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
        ClearSearch();
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
        ClearSearch();
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

        // 1. Skip trailing whitespace
        while (startX > 0 && char.IsWhiteSpace(line[startX - 1]))
        {
            startX--;
        }
        // 2. Skip the word (contiguous non-whitespace)
        while (startX > 0 && !char.IsWhiteSpace(line[startX - 1]))
        {
            startX--;
        }
        
        if (startX == endX && endX > 0)
        {
             startX = endX - 1;
        }

        string deletedText = line.Substring(startX, endX - startX);
        Lines[cursor.Y].Remove(startX, endX - startX);
        UndoManager.PushAction(startX, cursor.Y, deletedText, ActionType.DeleteWord);
        cursor.X = startX;
        IsModified = true;
        ClearSearch();
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
        ClearSearch();
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
                cursor.Y = action.Y;
                break;
            case ActionType.ReplaceAll:
            case ActionType.Paste:
            case ActionType.DeleteSelection:
                if (action.PreviousState != null)
                {
                    Lines.Clear();
                    foreach (var line in action.PreviousState)
                    {
                        Lines.Add(new StringBuilder(line));
                    }
                    cursor.X = action.X;
                    cursor.Y = action.Y;
                }
                break;
        }
        IsModified = true;
        ClearSearch();
    }
}
