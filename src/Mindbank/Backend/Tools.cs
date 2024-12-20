using System;
using System.IO;
using System.Linq;
using System.Text;
using Mindbank.Backend.Exceptions;

namespace Mindbank.Backend;

public static class Tools
{
    public static void WriteByteArrWithVarInt(Stream stream, byte[] arr)
    {
        WriteVarInt(stream, arr.Length);
        stream.Write(arr, 0, arr.Length);
    }

    public static byte[] DecodeByteArrWithVarInt(Stream stream)
    {
        var value = DecodeVarInt(stream);
        var valueBytes = new byte[value];
        var valueRead = stream.Read(valueBytes, 0, value);
        if (valueRead != value)
            throw new MindbankEndOfFileException(1);
        return valueBytes;
    }

    public static void WriteVarInt(Stream stream, int value)
    {
        do
        {
            var b = (byte)(value & 0x7F);
            value >>= 7;
            b |= (byte)(value > 0 ? 0x80 : 0);
            stream.WriteByte(b);
        } while (value > 0);
    }

    public static void WriteVarUInt(Stream stream, uint value)
    {
        do
        {
            var b = (byte)(value & 0x7F);
            value >>= 7;
            b |= (byte)(value > 0 ? 0x80 : 0);
            stream.WriteByte(b);
        } while (value > 0);
    }

    public static void WriteVarLong(Stream stream, long value)
    {
        do
        {
            var b = (byte)(value & 0x7F);
            value >>= 7;
            b |= (byte)(value > 0 ? 0x80 : 0);
            stream.WriteByte(b);
        } while (value > 0);
    }

    public static int DecodeVarInt(Stream stream)
    {
        var value = 0;
        var shift = 0;
        byte b;
        do
        {
            b = (byte)stream.ReadByte();
            value |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    public static uint DecodeVarUInt(Stream stream)
    {
        uint value = 0;
        var shift = 0;
        while (true)
        {
            var b = (byte)stream.ReadByte();
            value |= (uint)(b & 0x7F) << shift;
            shift += 7;

            if ((b & 0x80) == 0) break;
        }

        return value;
    }

    public static long DecodeVarLong(Stream stream)
    {
        var value = 0;
        var shift = 0;
        byte b;
        do
        {
            b = (byte)stream.ReadByte();
            value |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);

        return value;
    }

    public static bool IsBitSet(int value, int bitPosition)
    {
        if (bitPosition < 0 || bitPosition > (sizeof(int) - 1) * 8)
            throw new ArgumentOutOfRangeException(
                nameof(bitPosition),
                string.Format(Lang.Lang.Tools_IsBitSet_Error, (sizeof(int) - 1) * 8)
            );
        var bitmask = 1 << bitPosition;
        return (value & bitmask) != 0;
    }

    public static string GenerateRandomText(int length = 17)
    {
        length = length switch
        {
            0 => throw new ArgumentOutOfRangeException(nameof(length), Lang.Lang.Tools_GenerateRandomTextError),
            < 0 => length * -1,
            _ => length
        };
        if (length >= int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(length),
                Lang.Lang.Tools_GenerateRandomTextError2);
        var builder = new StringBuilder();
        Enumerable
            .Range(65, 26)
            .Select(e => ((char)e).ToString())
            .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
            .Concat(Enumerable.Range(0, length - 1).Select(e => e.ToString()))
            .OrderBy(_ => Guid.NewGuid())
            .Take(length)
            .ToList().ForEach(e => builder.Append(e));
        return builder.ToString();
    }
}