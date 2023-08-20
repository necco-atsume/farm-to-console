public class TextTableReader
{
    public static TextTable ReadFrom(string file)
    {
        AssetReader reader = AssetReader.FromFile(file);
        AssetReader.Cursor cursor = reader.StartReading();

        uint groupCount = reader.ReadUInt32(cursor);

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
            groupOffsets.Add(reader.ReadUInt32(cursor));
        }

        return groupOffsets;
    }

    private static TextGroup ReadGroup(AssetReader reader, int offset)
    {
        var gr = reader.WithOffset(offset);
        var cursor = gr.StartReading();

        uint entryCount = gr.ReadUInt32(cursor);

        List<ushort> entryOffsets = new((int) entryCount) { 0 };
        List<TextEntry> entries = new((int) entryCount);

        for (int i = 0; i < entryCount; i++) {
            entryOffsets.Add(gr.ReadUInt16(cursor));
        }

        var textReader = gr.WithOffset(4 + (4 * (int) entryCount));

        foreach (var textOffset in entryOffsets.Take((int) entryCount)) {
            ushort unknown = gr.ReadUInt16(cursor);
            string text = textReader.ReadHarvestMoonDsString(textOffset);
            entries.Add(new TextEntry 
                { 
                    Unknown = unknown,
                    Text = MessageConverter.ToHumanReadable(text),
                    MetaOriginalText = MessageConverter.ToHumanReadable(text), 
                    MetaOriginalAddress = textOffset,
                    MetaOriginalHex = Convert.ToHexString(textReader.ReadNullTerminatedBytes(textOffset)),
                }
            );
        };

        return new TextGroup
        {
            Entries = entries
        };
    }
}
