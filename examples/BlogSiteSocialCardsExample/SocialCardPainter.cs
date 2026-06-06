using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

/// <summary>
/// A tiny, dependency-free PNG encoder used only to keep this example self-contained — it paints a
/// solid-color card at the requested size. A real renderer would draw the page title/description
/// (with an image library) or screenshot an HTML template (with a headless browser) instead.
/// </summary>
public static class SocialCardPainter
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>Encodes a solid RGB PNG of the given dimensions (defaults to a slate-blue fill).</summary>
    public static byte[] SolidCard(int width, int height, byte r = 0x1E, byte g = 0x29, byte b = 0x3B)
    {
        // Raw image: each scanline is a filter byte (0 = none) followed by RGB triples.
        var raw = new byte[height * (1 + width * 3)];
        var i = 0;
        for (var y = 0; y < height; y++)
        {
            raw[i++] = 0;
            for (var x = 0; x < width; x++)
            {
                raw[i++] = r;
                raw[i++] = g;
                raw[i++] = b;
            }
        }

        byte[] idat;
        using (var ms = new MemoryStream())
        {
            using (var zlib = new ZLibStream(ms, CompressionLevel.Fastest, leaveOpen: true))
            {
                zlib.Write(raw, 0, raw.Length);
            }
            idat = ms.ToArray();
        }

        using var output = new MemoryStream();
        output.Write(Signature);

        var ihdr = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(0), width);
        BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), height);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 2; // color type: truecolor RGB
        WriteChunk(output, "IHDR", ihdr);
        WriteChunk(output, "IDAT", idat);
        WriteChunk(output, "IEND", []);

        return output.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);

        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);

        Span<byte> crc = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crc, Crc32(typeBytes, data));
        stream.Write(crc);
    }

    private static uint Crc32(byte[] type, byte[] data)
    {
        var crc = 0xFFFFFFFFu;
        crc = Accumulate(crc, type);
        crc = Accumulate(crc, data);
        return crc ^ 0xFFFFFFFFu;
    }

    private static uint Accumulate(uint crc, byte[] bytes)
    {
        foreach (var value in bytes)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
            }
        }
        return crc;
    }
}
