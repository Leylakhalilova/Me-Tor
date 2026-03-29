namespace MiniNotepad.UI;

public record ToolboxItem(string Label, string Shortcut);

public static class Toolbox
{
    public static readonly List<ToolboxItem> FileItems = new()
    {
        new ToolboxItem("Aç", "CTRL+O"),
        new ToolboxItem("Kaydet", "CTRL+S"),
        new ToolboxItem("Farklı Kaydet", "CTRL+A"),
    };

    public static readonly List<ToolboxItem> EditItems = new()
    {
        new ToolboxItem("Bul", "CTRL+F"),
        new ToolboxItem("Eşleşme Bulucu", "CTRL+G"),
        new ToolboxItem("Değiştir", "CTRL+H"),
        new ToolboxItem("Geri Al", "CTRL+Z"),
        new ToolboxItem("Kopyala", "CTRL+C"),
        new ToolboxItem("Kes", "CTRL+X"),
        new ToolboxItem("Yapıştır", "CTRL+V"),
    };
    
    public static List<ToolboxItem> GetAllItems()
    {
        var all = new List<ToolboxItem>();
        all.AddRange(FileItems);
        all.AddRange(EditItems);
        return all;
    }
}
