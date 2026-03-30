using System.Text;
using System.Runtime.InteropServices;
using MiniNotepad.Core;
using MiniNotepad.UI;
using MiniNotepad.IO;

namespace MiniNotepad;

class Program
{
    static string internalClipboard = "";
    // Windows terminal ayarlarını değiştirmek için Win32 API
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    const int STD_INPUT_HANDLE = -10;
    const uint ENABLE_PROCESSED_INPUT = 0x0001;

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.CursorVisible = true;
        Console.TreatControlCAsInput = true;

        // Windows'ta CTRL+S gibi tuşların terminal tarafından yutulmasını engelle
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr hIn = GetStdHandle(STD_INPUT_HANDLE);
            if (GetConsoleMode(hIn, out uint mode))
            {
                // ENABLE_PROCESSED_INPUT (0x0001): CTRL+C, CTRL+S gibi tuşların sistem tarafından işlenmesini engeller
                // ENABLE_QUICK_EDIT_MODE (0x0040): Fare ile seçim yapmayı engeller (inputu durdurabilir)
                // ENABLE_EXTENDED_FLAGS (0x0080): QuickEdit gibi flagleri değiştirmek için gereklidir
                mode &= ~ENABLE_PROCESSED_INPUT;
                mode &= ~0x0040u; // QuickEdit OFF (uint literal eklendi)
                mode |= 0x0080u;  // Extended Flags ON (uint literal eklendi)
                SetConsoleMode(hIn, mode);
            }
        }

        var buffer = new EditorBuffer();
        buffer.CurrentFilePath = "yeni_dosya.txt"; // Başlangıçta varsayılan dosya adı
        var cursor = new EditorCursor();
        var renderer = new Renderer();

        bool running = true;
        renderer.Render(buffer, cursor);

        while (running)
        {
            var keyInfo = Console.ReadKey(true);
            bool isCtrl = (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
            bool isShift = (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;

            // Kısayol kontrolleri
            bool isCtrlS = (keyInfo.Key == ConsoleKey.S && isCtrl) || (keyInfo.KeyChar == (char)19);
            bool isCtrlO = (keyInfo.Key == ConsoleKey.O && isCtrl) || (keyInfo.KeyChar == (char)15);
            bool isCtrlA = (keyInfo.Key == ConsoleKey.A && isCtrl) || (keyInfo.KeyChar == (char)1);
            bool isCtrlT = (keyInfo.Key == ConsoleKey.T && isCtrl) || (keyInfo.KeyChar == (char)20);
            bool isCtrlZ = (keyInfo.Key == ConsoleKey.Z && isCtrl) || (keyInfo.KeyChar == (char)26);
            bool isCtrlG = (keyInfo.Key == ConsoleKey.G && isCtrl) || (keyInfo.KeyChar == (char)7);
            bool isCtrlF = (keyInfo.Key == ConsoleKey.F && isCtrl) || (keyInfo.KeyChar == (char)6);
            bool isCtrlR = (keyInfo.Key == ConsoleKey.R && isCtrl) || (keyInfo.KeyChar == (char)18);
            bool isCtrlC = (keyInfo.Key == ConsoleKey.C && isCtrl) || (keyInfo.KeyChar == (char)3);
            bool isCtrlX = (keyInfo.Key == ConsoleKey.X && isCtrl) || (keyInfo.KeyChar == (char)24);
            bool isCtrlV = (keyInfo.Key == ConsoleKey.V && isCtrl) || (keyInfo.KeyChar == (char)22);
            
            // Ctrl+Backspace: ASCII 127 (Linux) veya ASCII 8 (Windows) + Control Modifier
            // H tuşu ile Backspace karışmaması için (ASCII 8) ConsoleKey.H kontrolü eklendi
            bool isCtrlBackspace = (isCtrl && keyInfo.Key != ConsoleKey.H && (keyInfo.Key == ConsoleKey.Backspace || keyInfo.KeyChar == (char)8 || keyInfo.KeyChar == (char)127));

            if (isCtrlS || isCtrlT || isCtrlO || isCtrlA || isCtrlZ || isCtrlG || isCtrlF || isCtrlR || isCtrlC || isCtrlX || isCtrlV || isCtrlBackspace)
            {
                try
                {
                    if (isCtrlS || isCtrlT)
                    {
                        cursor.ClearSelection();
                        if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                        {
                            var saveAsPath = FileExplorer.Explore("Farklı Kaydet", null, null);
                            if (!string.IsNullOrWhiteSpace(saveAsPath))
                            {
                                FileExplorer.Save(saveAsPath, buffer.GetLines());
                                buffer.CurrentFilePath = saveAsPath;
                                buffer.IsModified = false;
                                ShowStatus("Kaydedildi.");
                            }
                        }
                        else
                        {
                            FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
                            buffer.IsModified = false;
                            ShowStatus("Değişiklikler kaydedildi.");
                        }
                    }
                    else if (isCtrlA)
                    {
                        cursor.ClearSelection();
                        var saveAsPath = FileExplorer.Explore("Farklı Kaydet", null, buffer.CurrentFilePath);
                        if (!string.IsNullOrWhiteSpace(saveAsPath))
                        {
                            FileExplorer.Save(saveAsPath, buffer.GetLines());
                            buffer.CurrentFilePath = saveAsPath;
                            buffer.IsModified = false;
                            ShowStatus("Farklı kaydedildi.");
                        }
                    }
                    else if (isCtrlO)
                    {
                        if (buffer.IsModified)
                        {
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write("Değişiklikleri kaydetmek istiyor musunuz? (e/h): ");
                            var choice = Console.ReadKey(true).Key;
                            if (choice == ConsoleKey.E)
                            {
                                if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                                {
                                    var saveAsPath = FileExplorer.Explore("Kaydet", null, null);
                                    if (!string.IsNullOrWhiteSpace(saveAsPath))
                                        FileExplorer.Save(saveAsPath, buffer.GetLines());
                                }
                                else
                                {
                                    FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
                                }
                            }
                        }

                        var openPath = FileExplorer.Explore("Dosya Aç", null, null);
                        if (!string.IsNullOrWhiteSpace(openPath) && File.Exists(openPath))
                        {
                            var content = FileExplorer.Open(openPath);
                            buffer.Load(content, openPath);
                            cursor.X = 0;
                            cursor.Y = 0;
                            cursor.ClearSelection();
                            ShowStatus("Dosya açıldı.");
                        }
                    }
                    else if (isCtrlZ)
                    {
                        buffer.Undo(cursor);
                        cursor.ClearSelection();
                    }
                    else if (isCtrlC)
                    {
                        if (cursor.IsSelecting)
                        {
                            internalClipboard = buffer.GetSelectedText(cursor);
                            ShowStatus("Seçim kopyalandı.");
                        }
                    }
                    else if (isCtrlX)
                    {
                        if (cursor.IsSelecting)
                        {
                            internalClipboard = buffer.GetSelectedText(cursor);
                            buffer.DeleteSelection(cursor);
                            ShowStatus("Seçim kesildi.");
                        }
                    }
                    else if (isCtrlV)
                    {
                        if (cursor.IsSelecting)
                        {
                            buffer.DeleteSelection(cursor);
                        }
                        buffer.InsertText(cursor, internalClipboard);
                    }
                    else if (isCtrlF)
                    {
                        cursor.ClearSelection();
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Aranacak metin (Kısmi Eşleşme): ");
                        string? searchTerm = ReadLineWithEsc();
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            buffer.UpdateSearch(searchTerm, false); // false for partial match
                            if (buffer.Matches.Count > 0)
                            {
                                var firstMatch = buffer.Matches[0];
                                cursor.Y = firstMatch.LineIndex;
                                cursor.X = firstMatch.ColumnIndex;
                                buffer.CurrentMatchIndex = 0;
                            }
                            else
                            {
                                ShowStatus("Eşleşme bulunamadı!");
                            }
                        }
                    }
                    else if (isCtrlG)
                    {
                        cursor.ClearSelection();
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Aranacak kelime (Tam Eşleşme): ");
                        string? searchTerm = ReadLineWithEsc();
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            buffer.UpdateSearch(searchTerm, true); // true for whole word match
                            if (buffer.Matches.Count > 0)
                            {
                                var firstMatch = buffer.Matches[0];
                                cursor.Y = firstMatch.LineIndex;
                                cursor.X = firstMatch.ColumnIndex;
                                buffer.CurrentMatchIndex = 0;
                            }
                            else
                            {
                                ShowStatus("Eşleşme bulunamadı!");
                            }
                        }
                    }
                    else if (isCtrlR)
                    {
                        cursor.ClearSelection();
                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                        Console.Write("Değiştirilecek kelime: ");
                        string? oldText = ReadLineWithEsc();
                        if (oldText != null)
                        {
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write(new string(' ', Console.WindowWidth - 1));
                            Console.SetCursorPosition(0, Console.WindowHeight - 1);
                            Console.Write("Yeni kelime: ");
                            string? newText = ReadLineWithEsc();
                            if (newText != null)
                            {
                                buffer.ReplaceAll(oldText, newText);
                                ShowStatus("Tüm eşleşmeler değiştirildi.");
                            }
                        }
                    }
                    else if (isCtrlBackspace)
                    {
                        if (cursor.IsSelecting)
                        {
                            buffer.DeleteSelection(cursor);
                        }
                        else
                        {
                            buffer.DeleteWord(cursor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"HATA: {ex.Message}");
                }
            }
            else
            {
                // Temel navigasyon ve düzenleme
                bool isNavKey = keyInfo.Key == ConsoleKey.LeftArrow || keyInfo.Key == ConsoleKey.RightArrow ||
                                 keyInfo.Key == ConsoleKey.UpArrow || keyInfo.Key == ConsoleKey.DownArrow ||
                                 keyInfo.Key == ConsoleKey.Home || keyInfo.Key == ConsoleKey.End;

                // Navigasyon tuşuna basıldığında Ctrl basılı değilse seçimi temizle
                if (!isCtrl && isNavKey)
                {
                    cursor.ClearSelection();
                }

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (buffer.Matches.Count > 0 && buffer.CurrentMatchIndex != -1)
                        {
                            buffer.CurrentMatchIndex = (buffer.CurrentMatchIndex - 1 + buffer.Matches.Count) % buffer.Matches.Count;
                            var m = buffer.Matches[buffer.CurrentMatchIndex];
                            cursor.Y = m.LineIndex;
                            cursor.X = m.ColumnIndex;
                        }
                        else
                        {
                            if (isCtrl) cursor.StartSelection();
                            cursor.Move(0, -1, buffer.Lines);
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (buffer.Matches.Count > 0 && buffer.CurrentMatchIndex != -1)
                        {
                            buffer.CurrentMatchIndex = (buffer.CurrentMatchIndex + 1) % buffer.Matches.Count;
                            var m = buffer.Matches[buffer.CurrentMatchIndex];
                            cursor.Y = m.LineIndex;
                            cursor.X = m.ColumnIndex;
                        }
                        else
                        {
                            if (isCtrl) cursor.StartSelection();
                            cursor.Move(0, 1, buffer.Lines);
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        buffer.ClearSearch();
                        if (isCtrl)
                        {
                            cursor.StartSelection();
                            if (isShift) cursor.MoveToPreviousWord(buffer.Lines);
                            else cursor.Move(-1, 0, buffer.Lines);
                        }
                        else cursor.Move(-1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.RightArrow:
                        buffer.ClearSearch();
                        if (isCtrl)
                        {
                            cursor.StartSelection();
                            if (isShift) cursor.MoveToNextWord(buffer.Lines);
                            else cursor.Move(1, 0, buffer.Lines);
                        }
                        else cursor.Move(1, 0, buffer.Lines);
                        break;
                    case ConsoleKey.Home:
                        buffer.ClearSearch();
                        if (isCtrl) cursor.StartSelection();
                        cursor.X = 0;
                        break;
                    case ConsoleKey.End:
                        buffer.ClearSearch();
                        if (isCtrl) cursor.StartSelection();
                        cursor.X = buffer.Lines[cursor.Y].Length;
                        break;
                    case ConsoleKey.Enter:
                        buffer.ClearSearch();
                        if (cursor.IsSelecting) buffer.DeleteSelection(cursor);
                        buffer.NewLine(cursor.X, cursor.Y);
                        cursor.X = 0;
                        cursor.Y++;
                        break;
                    case ConsoleKey.Backspace:
                        buffer.ClearSearch();
                        if (cursor.IsSelecting)
                        {
                            buffer.DeleteSelection(cursor);
                        }
                        else if (cursor.X > 0 || cursor.Y > 0)
                        {
                            int oldX = cursor.X;
                            int oldY = cursor.Y;
                            if (cursor.X == 0)
                            {
                                cursor.Y--;
                                cursor.X = buffer.Lines[cursor.Y].Length;
                            }
                            else
                            {
                                cursor.X--;
                            }
                            buffer.DeleteChar(oldX, oldY);
                        }
                        break;
                    case ConsoleKey.Delete:
                        buffer.ClearSearch();
                        if (cursor.IsSelecting) buffer.DeleteSelection(cursor);
                        else buffer.DeleteForward(cursor.X, cursor.Y);
                        break;
                    case ConsoleKey.Escape:
                        if (buffer.IsModified)
                        {
                            bool validExit = false;
                            while (!validExit)
                            {
                                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                Console.Write(new string(' ', Console.WindowWidth - 1));
                                Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                Console.Write("Değişiklikleri kaydetmek istiyor musunuz? (e/h): ");
                                var choiceExit = Console.ReadKey(true).Key;
                                if (choiceExit == ConsoleKey.E)
                                {
                                    validExit = true;
                                    try
                                    {
                                        if (string.IsNullOrEmpty(buffer.CurrentFilePath))
                                        {
                                            var path = FileExplorer.Explore("Kapanış Öncesi Kaydet");
                                            if (!string.IsNullOrWhiteSpace(path))
                                                FileExplorer.Save(path, buffer.GetLines());
                                        }
                                        else
                                        {
                                            FileExplorer.Save(buffer.CurrentFilePath, buffer.GetLines());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.SetCursorPosition(0, Console.WindowHeight - 1);
                                        Console.BackgroundColor = ConsoleColor.Red;
                                        Console.Write($"KAYDETME HATASI: {ex.Message}");
                                        Console.ResetColor();
                                        Console.ReadKey(true);
                                    }
                                }
                                else if (choiceExit == ConsoleKey.H)
                                {
                                    validExit = true;
                                }
                            }
                        }
                        running = false;
                        break;
                    default:
                        if (!char.IsControl(keyInfo.KeyChar))
                        {
                            buffer.ClearSearch();
                            if (cursor.IsSelecting) buffer.DeleteSelection(cursor);

                            int viewWidth = Console.WindowWidth - 7; // LineNumberWidth(4) + " | "(3)
                            if (cursor.X >= viewWidth)
                            {
                                buffer.NewLine(cursor.X, cursor.Y);
                                cursor.X = 0;
                                cursor.Y++;
                            }
                            buffer.InsertChar(cursor.X, cursor.Y, keyInfo.KeyChar);
                            cursor.X++;
                        }
                        break;
                }
            }

            renderer.Render(buffer, cursor);
        }
    }

    static string? ReadLineWithEsc()
    {
        StringBuilder sb = new StringBuilder();
        int startX = Console.CursorLeft;
        int startY = Console.CursorTop;

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return sb.ToString();
            }
            if (key.Key == ConsoleKey.Escape)
            {
                return null;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write(key.KeyChar);
            }
        }
    }

    static void ShowStatus(string message)
    {
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.SetCursorPosition(0, Console.WindowHeight - 1);
        Console.Write(message + " (Bir tuşa basın)");
        Console.ReadKey(true);
    }
}
