using System.Text;
using MiniNotepad.Core;
using MiniNotepad.UI;

namespace MiniNotepad;

class Program
{
    static void Main(string[] args)
    {
        // UTF-8 desteği
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = true;

        var buffer = new EditorBuffer();
        var cursor = new EditorCursor();
        var renderer = new Renderer();

        bool running = true;
        renderer.Render(buffer, cursor);

        while (running)
        {
            var keyInfo = Console.ReadKey(true);

            // Temel navigasyon
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    cursor.Move(0, -1, buffer.Lines.Count, buffer.Lines[Math.Max(0, cursor.Y - 1)].Length);
                    break;
                case ConsoleKey.DownArrow:
                    cursor.Move(0, 1, buffer.Lines.Count, buffer.Lines[Math.Min(buffer.Lines.Count - 1, cursor.Y + 1)].Length);
                    break;
                case ConsoleKey.LeftArrow:
                    cursor.Move(-1, 0, buffer.Lines.Count, buffer.Lines[cursor.Y].Length);
                    break;
                case ConsoleKey.RightArrow:
                    cursor.Move(1, 0, buffer.Lines.Count, buffer.Lines[cursor.Y].Length);
                    break;
                case ConsoleKey.Enter:
                    buffer.NewLine(cursor.X, cursor.Y);
                    cursor.X = 0;
                    cursor.Y++;
                    break;
                case ConsoleKey.Backspace:
                    if (cursor.X > 0)
                    {
                        buffer.DeleteChar(cursor.X, cursor.Y);
                        cursor.X--;
                    }
                    else if (cursor.Y > 0)
                    {
                        int prevLineLen = buffer.Lines[cursor.Y - 1].Length;
                        buffer.DeleteChar(cursor.X, cursor.Y);
                        cursor.Y--;
                        cursor.X = prevLineLen;
                    }
                    break;
                case ConsoleKey.Escape:
                    if (buffer.IsModified)
                    {
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Değişiklikleri kaydetmek istiyor musunuz? (y/n): ");
                        var choice = Console.ReadKey().Key;
                        if (choice == ConsoleKey.Y) { /* Save Logic */ }
                    }
                    running = false;
                    break;
                default:
                    // Karakter ekleme
                    if (!char.IsControl(keyInfo.KeyChar))
                    {
                        buffer.InsertChar(cursor.X, cursor.Y, keyInfo.KeyChar);
                        cursor.X++;
                    }
                    break;
            }

            renderer.Render(buffer, cursor);
        }
    }
}
