using System.Text;
using MiniNotepad.Core;

namespace MiniNotepad.UI;

public class Renderer
{
    private const int ToolboxHeight = 2;
    private const int LineNumberWidth = 4;

    public void Render(EditorBuffer buffer, EditorCursor cursor)
    {
        Console.Clear();
        RenderToolbox(buffer.CurrentFilePath);
        RenderBuffer(buffer);
        UpdateCursor(cursor);
    }

    private void RenderToolbox(string? currentPath)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;
        string fileName = currentPath != null ? Path.GetFileName(currentPath) : "Yeni Dosya";
        Console.WriteLine($"[ {fileName} ] Dosya: Aç(CTRL+O) Kaydet(CTRL+S) Farklı Kaydet(CTRL+A)");
        Console.WriteLine("Düzenle: Bul(CTRL+F) Değiştir(CTRL+H) Geri Al(CTRL+Z) Kopyala(CTRL+C)");
        Console.ResetColor();
    }

    private void RenderBuffer(EditorBuffer buffer)
    {
        for (int i = 0; i < buffer.Lines.Count; i++)
        {
            Console.SetCursorPosition(0, ToolboxHeight + i);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{i + 1,3} | "); // Satır numarası
            Console.ResetColor();
            Console.Write(buffer.Lines[i].ToString());
        }
    }

    public void UpdateCursor(EditorCursor cursor)
    {
        // Cursor position must account for ToolboxHeight and LineNumberWidth
        Console.SetCursorPosition(cursor.X + LineNumberWidth + 2, cursor.Y + ToolboxHeight);
    }
}
