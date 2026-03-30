namespace MiniNotepad.Core;

public enum ActionType { InsertChar, DeleteChar, SplitLine, MergeLines, DeleteWord, ReplaceAll, Paste, DeleteSelection }

public record EditorAction(int X, int Y, string? Text, ActionType Type, List<string>? PreviousState = null);

public class UndoManager
{
    private readonly Stack<EditorAction> _undoStack = new();

    public void PushAction(int x, int y, string? text, ActionType type, List<string>? previousState = null)
    {
        _undoStack.Push(new EditorAction(x, y, text, type, previousState));
    }

    public EditorAction? PopAction()
    {
        return _undoStack.Count > 0 ? _undoStack.Pop() : null;
    }

    public void Clear()
    {
        _undoStack.Clear();
    }
}
