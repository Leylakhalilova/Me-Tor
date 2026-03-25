namespace MiniNotepad.Core;

public record EditorAction(int X, int Y, char? Character, ActionType Type);

public enum ActionType { Insert, Delete, NewLine }

public class UndoManager
{
    private readonly Stack<EditorAction> _undoStack = new();

    public void PushAction(int x, int y, char? character, ActionType type)
    {
        _undoStack.Push(new EditorAction(x, y, character, type));
    }

    public EditorAction? PopAction()
    {
        return _undoStack.Count > 0 ? _undoStack.Pop() : null;
    }
}
