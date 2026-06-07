using System.Text;
using MiniNotepad.Core;

namespace MiniNotepad.UI;

public class Renderer
{
    public int ToolboxHeight { get; private set; } = 2;
    private const int LineNumberWidth = 4;
    
    public int ScrollY { get; private set; } = 0;
    public int ScrollX { get; private set; } = 0;

    public void Render(EditorBuffer buffer, EditorCursor cursor)
    {
        // Önce toolbox yüksekliğini hesapla (UpdateScroll için gerekli)
        CalculateToolboxHeight(buffer.CurrentFilePath);
        UpdateScroll(cursor);
        
        // Console.Clear() yerine her satırı manuel temizleyerek yazacağız
        // Bu, titremeyi (flickering) engeller.
        RenderToolbox(buffer.CurrentFilePath);
        RenderBuffer(buffer, cursor);
        
        RenderStatusBar(buffer, cursor);
        
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

    private void CalculateToolboxHeight(string? currentPath)
    {
        string fileName = currentPath != null ? Path.GetFileName(currentPath) : "Yeni Dosya";
        int currentX = ($"[ {fileName} ] ").Length; 
        int currentY = 0;
        var items = Toolbox.GetAllItems();
        foreach (var item in items)
        {
            string s = $"{item.Label}({item.Shortcut}) ";
            if (currentX + s.Length > Console.WindowWidth)
            {
                currentY++;
                currentX = 0;
            }
            currentX += s.Length;
        }
        ToolboxHeight = currentY + 1;
    }

    private void RenderToolbox(string? currentPath)
    {
        Console.SetCursorPosition(0, 0);
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;
        
        string fileName = currentPath != null ? Path.GetFileName(currentPath) : "Yeni Dosya";
        string header = $"[ {fileName} ] ";
        
        var items = Toolbox.GetAllItems();
        int currentX = header.Length;
        int currentY = 0;
        
        Console.Write(header);
        
        foreach (var item in items)
        {
            string s = $"{item.Label}({item.Shortcut}) ";
            if (currentX + s.Length > Console.WindowWidth)
            {
                // Satırı boşlukla doldur ki eski içerik kalmasın
                Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - currentX)));
                currentY++;
                Console.SetCursorPosition(0, currentY);
                currentX = 0;
            }
            Console.Write(s);
            currentX += s.Length;
        }
        
        Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - currentX)));
        Console.ResetColor();
    }

    private void RenderStatusBar(EditorBuffer buffer, EditorCursor cursor)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.BackgroundColor = ConsoleColor.DarkCyan;
        Console.ForegroundColor = ConsoleColor.White;
        
        string mode = cursor.IsSelecting ? "SEÇİM" : "DÜZENLEME";
        string filePath = buffer.CurrentFilePath != null ? Path.GetFileName(buffer.CurrentFilePath) : "Yeni Dosya";
        string modifiedIndicator = buffer.IsModified ? "*" : "";
        string posInfo = $"Satır: {cursor.Y + 1}, Sütun: {cursor.X + 1}";
        
        string searchInfo = "";
        if (buffer.Matches.Count > 0 && buffer.CurrentMatchIndex != -1)
        {
            searchInfo = $" | [Eşleşme: {buffer.CurrentMatchIndex + 1}/{buffer.Matches.Count}]";
        }
        
        string statusText = $" {mode} | {filePath}{modifiedIndicator} | {posInfo}{searchInfo}";
        
        // Fill the rest of the line with spaces
        int padding = Console.WindowWidth - statusText.Length;
        if (padding > 0)
        {
            statusText += new string(' ', padding);
        }
        else if (statusText.Length > Console.WindowWidth)
        {
            statusText = statusText.Substring(0, Console.WindowWidth);
        }
        
        Console.Write(statusText);
        Console.ResetColor();
    }

    private void RenderBuffer(EditorBuffer buffer, EditorCursor cursor)
    {
        int viewHeight = Console.WindowHeight - ToolboxHeight - 1; // Durum satırı için -1
        int viewWidth = Console.WindowWidth - (LineNumberWidth + 3);
        
        for (int i = 0; i < viewHeight; i++)
        {
            int lineIdx = i + ScrollY;
            Console.SetCursorPosition(0, ToolboxHeight + i);

            if (lineIdx < buffer.Lines.Count)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{lineIdx + 1,3} | "); // Satır numarası
                Console.ResetColor();
                
                string line = buffer.Lines[lineIdx].ToString();
                var lineMatches = buffer.Matches.Where(m => m.LineIndex == lineIdx).ToList();

                // Satırı karakter karakter değil, stil değiştikçe blok blok yazalım
                StringBuilder lineBuilder = new StringBuilder();
                ConsoleColor currentBg = ConsoleColor.Black;
                ConsoleColor currentFg = ConsoleColor.Gray;

                for (int x = 0; x < viewWidth; x++)
                {
                    int bufferX = x + ScrollX;
                    ConsoleColor bg = ConsoleColor.Black;
                    ConsoleColor fg = ConsoleColor.Gray;

                    bool isMatch = false;
                    bool isCurrentMatch = false;

                    if (bufferX < line.Length)
                    {
                        foreach (var m in lineMatches)
                        {
                            if (bufferX >= m.ColumnIndex && bufferX < m.ColumnIndex + m.Length)
                            {
                                isMatch = true;
                                if (buffer.Matches.IndexOf(m) == buffer.CurrentMatchIndex)
                                    isCurrentMatch = true;
                                break;
                            }
                        }
                    }

                    if (cursor.IsInsideSelection(bufferX, lineIdx))
                    {
                        bg = ConsoleColor.Yellow;
                        fg = ConsoleColor.Black;
                    }
                    else if (isCurrentMatch)
                    {
                        bg = ConsoleColor.DarkBlue;
                        fg = ConsoleColor.White;
                    }
                    else if (isMatch)
                    {
                        bg = ConsoleColor.Blue;
                        fg = ConsoleColor.White;
                    }

                    if (bg != currentBg || fg != currentFg)
                    {
                        if (lineBuilder.Length > 0)
                        {
                            Console.BackgroundColor = currentBg;
                            Console.ForegroundColor = currentFg;
                            Console.Write(lineBuilder.ToString());
                            lineBuilder.Clear();
                        }
                        currentBg = bg;
                        currentFg = fg;
                    }

                    if (bufferX < line.Length)
                        lineBuilder.Append(line[bufferX]);
                    else
                        lineBuilder.Append(' ');
                }

                if (lineBuilder.Length > 0)
                {
                    Console.BackgroundColor = currentBg;
                    Console.ForegroundColor = currentFg;
                    Console.Write(lineBuilder.ToString());
                }
            }
            else
            {
                // Boş satırları temizle
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.ResetColor();
        }
    }

    public void UpdateCursor(EditorCursor cursor)
    {
        int screenX = cursor.X - ScrollX + LineNumberWidth + 2;
        int screenY = cursor.Y - ScrollY + ToolboxHeight;
        
        if (screenX >= 0 && screenX < Console.WindowWidth && screenY >= 0 && screenY < Console.WindowHeight)
        {
            Console.SetCursorPosition(screenX, screenY);
        }
    }
}
