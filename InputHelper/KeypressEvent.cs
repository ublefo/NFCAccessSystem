namespace InputHelper;

public class KeyPressEvent : EventArgs
{
    public KeyPressEvent(EventCode code, KeyState state)
    {
        Code = code;
        State = state;
    }

    public EventCode Code { get; }

    public KeyState State { get; }
}