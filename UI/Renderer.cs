using System.Text;
using MiniNotepad.Core;

namespace MiniNotepad.UI;

public class Renderer
{
    private const int ToolboxHeight = 2;
    private const int LineNumberWidth = 4;
    
    public int ScrollY { get; private set; } = 0;
    public int ScrollX { get; private set; } = 0;

    public void Render(EditorBuffer buffer, EditorCursor cursor)
    {
        UpdateScroll(cursor);
        Console.Clear();
        RenderToolbox(buffer.CurrentFilePath);
        RenderBuffer(buffer);
        UpdateCursor(cursor);
    }

    private void UpdateScroll(EditorCursor cursor)
    {
        int viewHeight = Console.WindowHeight - ToolboxHeight - 1; // -1 for status line if any
        int viewWidth = Console.WindowWidth - (LineNumberWidth + 3);

        if (cursor.Y < ScrollY) ScrollY = cursor.Y;
        if (cursor.Y >= ScrollY + viewHeight) ScrollY = cursor.Y - viewHeight + 1;

        if (cursor.X < ScrollX) ScrollX = cursor.X;
        if (cursor.X >= ScrollX + viewWidth) ScrollX = cursor.X - viewWidth + 1;
    }

    private void RenderToolbox(string? currentPath)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;
        string fileName = currentPath != null ? Path.GetFileName(currentPath) : "Yeni Dosya";
        string toolboxLine1 = $"[ {fileName} ] Dosya: Aç(CTRL+O) Kaydet(CTRL+S) Farklı Kaydet(CTRL+A)";
        string toolboxLine2 = "Düzenle: Bul(CTRL+F) Değiştir(CTRL+H) Geri Al(CTRL+Z) Kopyala(CTRL+C)";
        
        Console.WriteLine(toolboxLine1.PadRight(Console.WindowWidth));
        Console.WriteLine(toolboxLine2.PadRight(Console.WindowWidth));
        Console.ResetColor();
    }

    private void RenderBuffer(EditorBuffer buffer)
    {
        int viewHeight = Console.WindowHeight - ToolboxHeight;
        
        for (int i = 0; i < Math.Min(viewHeight, buffer.Lines.Count - ScrollY); i++)
        {
            int lineIdx = i + ScrollY;
            Console.SetCursorPosition(0, ToolboxHeight + i);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{lineIdx + 1,3} | "); // Satır numarası
            Console.ResetColor();
            
            string line = buffer.Lines[lineIdx].ToString();
            if (ScrollX < line.Length)
            {
                int len = Math.Min(line.Length - ScrollX, Console.WindowWidth - (LineNumberWidth + 3));
                Console.Write(line.Substring(ScrollX, len));
            }
        }
    }

    public void UpdateCursor(EditorCursor cursor)
    {
        int screenX = cursor.X - ScrollX + LineNumberWidth + 2;
        int screenY = cursor.Y - ScrollY + ToolboxHeight;
        
        // Ensure cursor is within visible bounds before setting position
        if (screenX >= 0 && screenX < Console.WindowWidth && screenY >= 0 && screenY < Console.WindowHeight)
        {
            Console.SetCursorPosition(screenX, screenY);
        }
    }
}
