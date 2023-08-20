public class TextTableReader
{
    public static TextTable ReadFrom(string file)
    {
        AssetReader reader = AssetReader.FromFile(file);
        AssetReader.Cursor cursor = reader.StartReading();

        uint groupCount = reader.Read<uint>(cursor);

        List<uint> groupOffsets = GetGroups(reader, cursor, groupCount);

        List<TextGroup> groups = new((int) groupCount);
        foreach (var groupOffset in groupOffsets)
        {
            TextGroup group = ReadGroup(reader, (int) groupOffset);
            groups.Add(group);
        }

        return new TextTable
        {
            Groups = groups
        };
    }

    private static List<uint> GetGroups(AssetReader reader, AssetReader.Cursor cursor, uint groupCount)
    {
        List<uint> groupOffsets = new((int)groupCount);

        for (int i = 0; i < groupCount; i++)
        {
            groupOffsets.Add(reader.Read<uint>(cursor));
        }

        return groupOffsets;
    }

    private static TextGroup ReadGroup(AssetReader reader, int offset)
    {
        var gr = reader.WithOffset((int) offset);
        var cursor = gr.StartReading();

        uint entryCount = gr.Read<uint>(cursor);

        List<ushort> entryOffsets = new((int) entryCount);
        List<TextEntry> entries = new((int) entryCount);

        for (int i = 0; i < entryCount; i++) {
            entryOffsets.Add(gr.Read<ushort>(cursor));
        }

        foreach (var textOffset in entryOffsets) {
            ushort unknown = gr.Read<ushort>(cursor);
            string text = gr.ReadNullTerminatedString(textOffset);
            entries.Add(new TextEntry 
                { 
                    Unknown = (int) unknown,
                    Text = text,
                    MetaOriginalText = text, 
                    MetaOriginalAddress = textOffset,
                }
            );
        };

        return new TextGroup
        {
            Entries = entries
        };
    }
}
