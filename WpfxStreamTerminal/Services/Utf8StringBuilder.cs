using System.Text;

namespace WpfxStreamTerminal.Services;

public sealed class Utf8StringBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly Decoder _decoder = Encoding.UTF8.GetDecoder();
    private readonly char[] _chars;

    public Utf8StringBuilder(int maxBytesPerAppend)
    {
        _chars = new char[Encoding.UTF8.GetMaxCharCount(maxBytesPerAppend)];
    }

    public void Append(byte[] bytes, int count)
    {
        int charsRead = _decoder.GetChars(bytes, 0, count, _chars, 0);

        _sb.Append(_chars, 0, charsRead);
    }

    public int FindLineTerminator(char terminatorChar)
    {
        for (int i = 0; i < _sb.Length; i++)
        {
            if (_sb[i] == terminatorChar)
                return i;
        }

        return -1;
    }

    public string ToString(int startIndex, int length) => _sb.ToString(startIndex, length);

    public void Remove(int startIndex, int length) => _sb.Remove(startIndex, length);
}
