namespace FF2SaveEditor.Core.GameData;

/// <summary>
/// Converts between FF2 Famicom text encoding and Unicode.
/// Uses same basic encoding as FF3: 0x80-0x89=0-9, 0x8A-0xA3=A-Z, 0xA4-0xBD=a-z.
/// </summary>
public static class TextEncoding
{
    public static string Decode(ReadOnlySpan<byte> data)
    {
        var chars = new char[data.Length];
        int len = 0;
        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            if (b == 0xFF) break;
            chars[len++] = b switch
            {
                >= 0x80 and <= 0x89 => (char)('0' + (b - 0x80)),
                >= 0x8A and <= 0xA3 => (char)('A' + (b - 0x8A)),
                >= 0xA4 and <= 0xBD => (char)('a' + (b - 0xA4)),
                0xBF => '\'',
                0xC2 => '-',
                0xC4 => '!',
                0xC5 => '?',
                _ => '?',
            };
        }
        return new string(chars, 0, len);
    }

    public static byte[] Encode(string text, int maxLength)
    {
        var result = new byte[maxLength];
        Array.Fill(result, (byte)0xFF);
        int len = Math.Min(text.Length, maxLength);
        for (int i = 0; i < len; i++)
        {
            result[i] = text[i] switch
            {
                >= '0' and <= '9' => (byte)(0x80 + (text[i] - '0')),
                >= 'A' and <= 'Z' => (byte)(0x8A + (text[i] - 'A')),
                >= 'a' and <= 'z' => (byte)(0xA4 + (text[i] - 'a')),
                '\'' => 0xBF,
                '-' => 0xC2,
                '!' => 0xC4,
                '?' => 0xC5,
                _ => 0xFF,
            };
        }
        return result;
    }
}
