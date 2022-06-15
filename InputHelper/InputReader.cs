using InputHelper;

public class InputReader : IDisposable
{
    private const int BufferLength = 24;
    private readonly byte[] _buffer = new byte[BufferLength];
    private FileStream _stream;

    public InputReader(string path)
    {
        _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    }

    public KeyPressEvent GetKeyPress()
    {
        while (true)
        {
            _stream.Read(_buffer, 0, BufferLength);

            var type = BitConverter.ToInt16(new[] {_buffer[16], _buffer[17]}, 0);
            var code = BitConverter.ToInt16(new[] {_buffer[18], _buffer[19]}, 0);
            var value = BitConverter.ToInt32(new[] {_buffer[20], _buffer[21], _buffer[22], _buffer[23]}, 0);

            var eventType = (EventType) type;

            if (eventType == EventType.EV_KEY)
            {
                var c = (EventCode) code;
                var s = (KeyState) value;
                return new KeyPressEvent(c, s);
            }
        }
    }

    public void Dispose()
    {
        _stream.Dispose();
        _stream = null;
    }
}