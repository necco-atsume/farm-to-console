using System.IO;

public class TextTable
{
    public int Count => Groups.Count;
    public List<TextGroup> Groups { get; set; }

    public TextTable() 
    {
        Groups = new List<TextGroup>();
    }

    public TextTable(List<TextGroup> groups) 
    {
        Groups = groups;
    }
}

public class TextGroup
{
    public int Count => Entries.Count;
    public List<TextEntry> Entries { get; set; }

    public TextGroup() 
    {
        Entries = new List<TextEntry>();
    }

    public TextGroup(List<TextEntry> entries)
    {
        Entries = entries;
    }
}

public class TextEntry
{
    // These are typically just ascending ushorts.
    // NO idea what they're for. (Are they storing the index or something? lol)
    public int Unknown { get; set; }

    public string Text { get; set; } = string.Empty;
    public string MetaOriginalText { get; set; } = string.Empty;

    public int MetaOriginalAddress { get; set; }
    public string MetaOriginalHex { get; set; } = string.Empty;
}
