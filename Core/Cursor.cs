using System.Text;

namespace MiniNotepad.Core;

public class EditorCursor
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;

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
}
