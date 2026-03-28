namespace MiniNotepad.Core;

public enum ActionType { InsertChar, DeleteChar, SplitLine, MergeLines, DeleteWord }

public record EditorAction(int X, int Y, string? Text, ActionType Type);

public class UndoManager
{
    private readonly Stack<EditorAction> _undoStack = new();

    public void PushAction(int x, int y, string? text, ActionType type)
    {
        _undoStack.Push(new EditorAction(x, y, text, type));
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
