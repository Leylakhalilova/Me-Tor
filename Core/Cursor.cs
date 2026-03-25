namespace MiniNotepad.Core;

public class EditorCursor
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;

    public void Move(int dx, int dy, int maxLines, int currentLineLength)
    {
        Y = Math.Clamp(Y + dy, 0, maxLines - 1);
        X = Math.Clamp(X + dx, 0, currentLineLength);
    }
}
