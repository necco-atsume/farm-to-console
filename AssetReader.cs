using System.Buffers.Binary;
using System.CommandLine.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public enum Endianness
{
    Little = 0,
    Big = 1
}

public class AssetReader
{
    private readonly ReadOnlyMemory<byte> bytes;

    public AssetReader(byte[] bytes)
    {
        this.bytes = new ReadOnlyMemory<byte>(bytes);
    }

    public AssetReader(ReadOnlyMemory<byte> bytes)
    {
        this.bytes = bytes;
    }

    public static AssetReader FromFile(string filePath)
    {
        return new AssetReader(File.ReadAllBytes(filePath));
    }

    public AssetReader WithOffset(int offset) 
    {
        return new AssetReader(bytes.Slice(offset));
    }

    public Cursor StartReading(int offset = 0) => new Cursor { Offset = offset };

    public byte[] ReadBytes(Cursor cursor, int length)
    {
        byte[] result = ReadBytes(cursor.Offset, length);
        cursor.MoveForward(length);
        return result;
    } 

    public byte[] ReadBytes(int offset, int length) => bytes.Slice(offset, length).ToArray();

    public byte ReadByte(int offset) => bytes.Span[offset];
    public sbyte ReadSByte(int offset) => (sbyte) bytes.Span[offset];

    public ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes.Span[offset..]);
    public ushort ReadUInt16(Cursor cursor)
    { 
        var val = ReadUInt16(cursor.Offset);
        cursor.MoveForward(sizeof(ushort));
        return val;
    }

    public uint ReadUInt32(int offset) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.Span[offset..]);
    public uint ReadUInt32(Cursor cursor)
    { 
        var val = ReadUInt32(cursor.Offset);
        cursor.MoveForward(sizeof(uint));
        return val;
    }
    public ulong ReadUInt64(int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.Span[offset..]);
    public ulong ReadUInt64(Cursor cursor)
    { 
        var val = ReadUInt64(cursor.Offset);
        cursor.MoveForward(sizeof(ulong));
        return val;
    }

    public short ReadInt16(int offset) => BinaryPrimitives.ReadInt16LittleEndian(bytes.Span[offset..]);
    public short ReadInt16(Cursor cursor)
    { 
        var val = ReadInt16(cursor.Offset);
        cursor.MoveForward(sizeof(short));
        return val;
    }

    public int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes.Span[offset..]);
    public int ReadInt32(Cursor cursor)
    { 
        var val = ReadInt32(cursor.Offset);
        cursor.MoveForward(sizeof(int));
        return val;
    }

    public long ReadInt64(int offset) => BinaryPrimitives.ReadInt64LittleEndian(bytes.Span[offset..]);
    public long ReadInt64(Cursor cursor)
    { 
        var val = ReadInt64(cursor.Offset);
        cursor.MoveForward(sizeof(long));
        return val;
    }

    public float ReadFloat(int offset) => BinaryPrimitives.ReadSingleLittleEndian(bytes.Span[offset..]);
    public float ReadFloat(Cursor cursor)
    { 
        var val = ReadFloat(cursor.Offset);
        cursor.MoveForward(sizeof(float));
        return val;
    }

    public double ReadDouble(int offset) => BinaryPrimitives.ReadDoubleLittleEndian(bytes.Span[offset..]);
    public double ReadDouble(Cursor cursor)
    { 
        var val = ReadDouble(cursor.Offset);
        cursor.MoveForward(sizeof(double));
        return val;
    }

    public string ReadNullTerminatedString(Cursor cursor) 
    {
        var result = ReadNullTerminatedString(cursor.Offset);
        cursor.MoveForward(result.Length);
        return result;
    }

    private int StrlenAt(int offset) 
    {
        int length = 0;
        while (offset + length < bytes.Length && bytes.Span[offset + length] != 0)
        {
            length++;
        }
        return length;
    }

    public string ReadNullTerminatedString(int offset, bool encodingIsAscii = true)
    {
        int length = StrlenAt(offset);
        if (encodingIsAscii) return ReadAsciiString(offset, length);
        else return ReadShiftJisString(offset, length);
    }

    public string ReadHarvestMoonDsString(int offset)
    {
        // TODO: Don't like that this logic lives here... :/
        // Modular-ize this.
        byte[] textRaw = ReadNullTerminatedBytes(offset);

        List<byte> byteBuf = new();
        for (int i = 0; i < textRaw.Length; i++) {
            // NB: 0xFF is a special space char in shiftjis.
            // \u8190 is a star character that's used all over the place.
            if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0xFF)
            {
                byteBuf.AddRange("[:81ff]".Select(x => (byte)x).ToArray());
                i++;
            } 
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0x99)
            {
                byteBuf.AddRange("[:star]".Select(x => (byte)x).ToArray());
                i++;
            } 
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0x63)
            {
                byteBuf.AddRange("[:ellipsis]".Select(x => (byte)x).ToArray());
                i++;
            } 
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0x40)
            {
                byteBuf.AddRange("[:idsp]".Select(x => (byte)x).ToArray());
                i++;
            } 
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0xF4)
            {
                byteBuf.AddRange("[:note]".Select(x => (byte)x).ToArray());
                i++;
            } 
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81 && textRaw[i+1] == 0x48)
            {
                byteBuf.AddRange("[:fwqm]".Select(x => (byte)x).ToArray());
                i++;
            }
            else if (i + 1 < textRaw.Length && textRaw[i] == 0xFF && textRaw[i+1] == 0x24)
            {
                byteBuf.AddRange("[%player]".Select(x => (byte)x).ToArray());
                i++;
            }
            else if (i + 1 < textRaw.Length && textRaw[i] == 0x81)
            {
                byteBuf.AddRange(("[:" + textRaw[i].ToString("x") + textRaw[i+1].ToString("x") + "]").Select(x => (byte)x).ToArray());
            }
            else if (textRaw[i] == 0xff) 
            {
                byteBuf.AddRange("[%replace]".Select(x => (byte)x).ToArray());
            }
            else 
            {
                byteBuf.Add(textRaw[i]);
            }
        }

        byte[] text = byteBuf.ToArray();

        if (text.Any(b => (b & 0x80) == 0x80 && b != 0xFF)) 
        {
            // Likely Japanese.
            try {
                var shiftJis = Encoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                return "jp:" + shiftJis.GetString(text);
            } 
            catch {}
        }

        byte[] textReplaced = text.SelectMany(a => 
            a > 0x7f 
                ? new[] { (byte)'[', (byte)':', (byte)a.ToString("x")[0], (byte)a.ToString("x")[1], (byte)']' } 
                : new[] { a }).ToArray();
        Encoding.GetEncoding("ascii"); 
        return "en:" + Encoding.ASCII.GetString(textReplaced);
    }

    public string ReadShiftJisString(int offset, int length)
    {
        var bytes = ReadBytes(offset, length);
        return Encoding.GetEncoding(932).GetString(bytes);
    }

    public string ReadAsciiString(int offset, int length) 
    {
        var bytes = ReadBytes(offset, length);
        if (bytes.Any(b => (b & 0x80) == 0x80)) 
        {
            return "hex:" + Convert.ToHexString(bytes);
        }
        else 
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }

    public byte[] ReadNullTerminatedBytes(int offset) 
    {
        int length = 0;
        while (offset + length < bytes.Length && bytes.Span[offset + length] != 0)
        {
            length++;
        }

        return bytes.Slice(offset, length).ToArray();
    }

    public class Cursor 
    {
        public int Offset { get; set; } = 0;

        public void MoveForward(int bytes) 
        {
            Offset += bytes;
        }

        public void AlignTo(int multipleOfSize) 
        {
            Offset += multipleOfSize - (Offset % multipleOfSize);
        }
    }
}