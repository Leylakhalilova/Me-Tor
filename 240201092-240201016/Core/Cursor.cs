using System.Text;

namespace MiniNotepad.Core;

public class EditorCursor
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;

    public int SelectionStartX { get; set; } = -1;
    public int SelectionStartY { get; set; } = -1;
    public bool IsSelecting => SelectionStartX != -1 && SelectionStartY != -1;

    public void StartSelection()
    {
        if (!IsSelecting)
        {
            SelectionStartX = X;
            SelectionStartY = Y;
        }
    }

    public void ClearSelection()
    {
        SelectionStartX = -1;
        SelectionStartY = -1;
    }

    public bool IsInsideSelection(int x, int y)
    {
        if (!IsSelecting) return false;

        int startY = SelectionStartY;
        int startX = SelectionStartX;
        int endY = Y;
        int endX = X;

        // Swap start and end if needed to make startY <= endY
        if (startY > endY || (startY == endY && startX > endX))
        {
            int tempY = startY;
            startY = endY;
            endY = tempY;

            int tempX = startX;
            startX = endX;
            endX = tempX;
        }

        if (y < startY || y > endY) return false;
        if (y > startY && y < endY) return true;
        if (startY == endY) return x >= startX && x < endX;
        if (y == startY) return x >= startX;
        if (y == endY) return x < endX;

        return false;
    }

    public void Move(int dx, int dy, List<StringBuilder> lines)
    {
        if (dx != 0)
        {
            X += dx;
            if (X < 0)
            {
                if (Y > 0)
                {
                    Y--;
                    X = lines[Y].Length;
                }
                else
                {
                    X = 0;
                }
            }
            else if (X > lines[Y].Length)
            {
                if (Y < lines.Count - 1)
                {
                    Y++;
                    X = 0;
                }
                else
                {
                    X = lines[Y].Length;
                }
            }
        }

        if (dy != 0)
        {
            Y = Math.Clamp(Y + dy, 0, lines.Count - 1);
            X = Math.Min(X, lines[Y].Length);
        }
    }

    public void MoveToPreviousWord(List<StringBuilder> lines)
    {
        if (X == 0 && Y == 0) return;

        if (X == 0)
        {
            Y--;
            X = lines[Y].Length;
            return;
        }

        string line = lines[Y].ToString();
        int newX = X;

        // 1. Skip whitespace
        while (newX > 0 && char.IsWhiteSpace(line[newX - 1]))
        {
            newX--;
        }
        // 2. Skip non-whitespace
        while (newX > 0 && !char.IsWhiteSpace(line[newX - 1]))
        {
            newX--;
        }
        X = newX;
    }

    public void MoveToNextWord(List<StringBuilder> lines)
    {
        if (Y == lines.Count - 1 && X == lines[Y].Length) return;

        string line = lines[Y].ToString();
        if (X >= line.Length)
        {
            Y++;
            X = 0;
            return;
        }

        int newX = X;
        // 1. Skip current non-whitespace
        while (newX < line.Length && !char.IsWhiteSpace(line[newX]))
        {
            newX++;
        }
        // 2. Skip whitespace
        while (newX < line.Length && char.IsWhiteSpace(line[newX]))
        {
            newX++;
        }
        X = newX;
    }
}
