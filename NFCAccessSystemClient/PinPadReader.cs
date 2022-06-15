using InputHelper;

namespace NFCAccessSystemClient;

public class PinPadReader
{
    private InputReader Reader { get; set; }

    public PinPadReader(InputReader reader)
    {
        Reader = reader;
    }

    public string ReadPin()
    {
        var result = "";

        while (true)
        {
            var keyPress = Reader.GetKeyPress();
            // only respond to keyup events
            if (keyPress.State == KeyState.KeyUp)
            {
                switch (keyPress.Code)
                {
                    case EventCode.Kp0:
                        result += "0";
                        break;
                    case EventCode.Kp1:
                        result += "1";
                        break;
                    case EventCode.Kp2:
                        result += "2";
                        break;
                    case EventCode.Kp3:
                        result += "3";
                        break;
                    case EventCode.Kp4:
                        result += "4";
                        break;
                    case EventCode.Kp5:
                        result += "5";
                        break;
                    case EventCode.Kp6:
                        result += "6";
                        break;
                    case EventCode.Kp7:
                        result += "7";
                        break;
                    case EventCode.Kp8:
                        result += "8";
                        break;
                    case EventCode.Kp9:
                        result += "9";
                        break;
                    case EventCode.Backspace:
                        if (result.Length > 0)
                        {
                            result = result.Remove(result.Length - 1, 1);
                        }

                        break;
                    case EventCode.KpEnter:
                        Console.WriteLine($"Enter pressed, returning code {result}");
                        return result;
                }

                Console.WriteLine($"Key pressed, current buffer: {result}");
            }
        }
    }
}