using System.Buffers.Binary;
using System.ComponentModel;
using System.Runtime.InteropServices;
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

    public T Read<T>(Cursor cursor, Endianness endianness = Endianness.Little) where T : unmanaged
    {
        T value = Read<T>(cursor.Offset, endianness);
        cursor.Offset += Marshal.SizeOf<T>();

        return value;
    }

    public T Read<T>(int offset, Endianness endianness = Endianness.Little) where T : unmanaged
    {
        object result = (Type.GetTypeCode(typeof(T)), endianness) switch {
            (TypeCode.Byte, _) => bytes.Span[offset],
            (TypeCode.SByte, _) => (sbyte) bytes.Span[offset],

            (TypeCode.UInt16, Endianness.Little) => BinaryPrimitives.ReadUInt16LittleEndian(bytes.Span.Slice(offset, sizeof(short))),
            (TypeCode.UInt32, Endianness.Little) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.Span.Slice(offset, sizeof(int))),
            (TypeCode.UInt64, Endianness.Little) => BinaryPrimitives.ReadUInt64LittleEndian(bytes.Span.Slice(offset, sizeof(long))),
            (TypeCode.Int16, Endianness.Little) => BinaryPrimitives.ReadInt16LittleEndian(bytes.Span.Slice(offset, sizeof(short))),
            (TypeCode.Int32, Endianness.Little) => BinaryPrimitives.ReadInt32LittleEndian(bytes.Span.Slice(offset, sizeof(int))),
            (TypeCode.Int64, Endianness.Little) => BinaryPrimitives.ReadInt64LittleEndian(bytes.Span.Slice(offset, sizeof(long))),
            (TypeCode.Single, Endianness.Little) => BinaryPrimitives.ReadSingleLittleEndian(bytes.Span.Slice(offset, sizeof(float))),
            (TypeCode.Double, Endianness.Little) => BinaryPrimitives.ReadDoubleLittleEndian(bytes.Span.Slice(offset, sizeof(double))),

            (TypeCode.UInt16, Endianness.Big) => BinaryPrimitives.ReadUInt16BigEndian(bytes.Span.Slice(offset, sizeof(short))),
            (TypeCode.UInt32, Endianness.Big) => BinaryPrimitives.ReadUInt32BigEndian(bytes.Span.Slice(offset, sizeof(int))),
            (TypeCode.UInt64, Endianness.Big) => BinaryPrimitives.ReadUInt64BigEndian(bytes.Span.Slice(offset, sizeof(long))),
            (TypeCode.Int16, Endianness.Big) => BinaryPrimitives.ReadInt16BigEndian(bytes.Span.Slice(offset, sizeof(short))),
            (TypeCode.Int32, Endianness.Big) => BinaryPrimitives.ReadInt32BigEndian(bytes.Span.Slice(offset, sizeof(int))),
            (TypeCode.Int64, Endianness.Big) => BinaryPrimitives.ReadInt64BigEndian(bytes.Span.Slice(offset, sizeof(long))),
            (TypeCode.Single, Endianness.Big) => BinaryPrimitives.ReadSingleBigEndian(bytes.Span.Slice(offset, sizeof(float))),
            (TypeCode.Double, Endianness.Big) => BinaryPrimitives.ReadDoubleBigEndian(bytes.Span.Slice(offset, sizeof(double))),
            _ => throw new Exception()
        };

        return (T) result;
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

    public string ReadNullTerminatedString(int offset)
    {
        int length = StrlenAt(offset);
        return ReadAsciiString(offset, length);
    }

    public string ReadAsciiString(int offset, int length) 
    {
        var bytes = ReadBytes(offset, length);
        return Encoding.ASCII.GetString(bytes);
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