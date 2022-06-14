namespace InputHelper;

/// <summary>
/// Mapping for this can be found here: https://github.com/torvalds/linux/blob/master/include/uapi/linux/input-event-codes.h
/// </summary>
public enum EventCode
{
    Reserved = 0,
    Esc = 1,
    Num1 = 2,
    Num2 = 3,
    Num3 = 4,
    Num4 = 5,
    Num5 = 6,
    Num6 = 7,
    Num7 = 8,
    Num8 = 9,
    Num9 = 10,
    Num0 = 11,
    Minus = 12,
    Equal = 13,
    Backspace = 14,
    Tab = 15,
    Q = 16,
    W = 17,
    E = 18,
    R = 19,
    T = 20,
    Y = 21,
    U = 22,
    I = 23,
    O = 24,
    P = 25,
    LeftBrace = 26,
    RightBrace = 27,
    Enter = 28,
    LeftCtrl = 29,
    A = 30,
    S = 31,
    D = 32,
    F = 33,
    G = 34,
    H = 35,
    J = 36,
    K = 37,
    L = 38,
    Semicolon = 39,
    Apostrophe = 40,
    Grave = 41,
    LeftShift = 42,
    Backslash = 43,
    Z = 44,
    X = 45,
    C = 46,
    V = 47,
    B = 48,
    N = 49,
    M = 50,
    Comma = 51,
    Dot = 52,
    Slash = 53,
    RightShift = 54,
    KpAsterisk = 55,
    LeftAlt = 56,
    Space = 57,
    Capslock = 58,
    F1 = 59,
    Pf2 = 60,
    F3 = 61,
    F4 = 62,
    F5 = 63,
    F6 = 64,
    F7 = 65,
    F8 = 66,
    Pf9 = 67,
    F10 = 68,
    Numlock = 69,
    ScrollLock = 70,
    Kp7 = 71,
    Kp8 = 72,
    Kp9 = 73,
    PkpMinus = 74,
    Kp4 = 75,
    Kp5 = 76,
    Kp6 = 77,
    KpPlus = 78,
    Kp1 = 79,
    Kp2 = 80,
    Kp3 = 81,
    Kp0 = 82,
    KpDot = 83,

    Zenkakuhankaku = 85,

    //102ND = 86,
    F11 = 87,
    F12 = 88,
    Ro = 89,
    Katakana = 90,
    Hiragana = 91,
    Henkan = 92,
    Katakanahiragana = 93,
    Muhenkan = 94,
    KpJpComma = 95,
    KpEnter = 96,
    RightCtrl = 97,
    KpSlash = 98,
    SysRq = 99,
    RightAlt = 100,
    LineFeed = 101,
    Home = 102,
    Up = 103,
    Pageup = 104,
    Left = 105,
    Right = 106,
    End = 107,
    Down = 108,
    Pagedown = 109,
    Insert = 110,
    Delete = 111,
    Macro = 112,
    Mute = 113,
    VolumeDown = 114,
    VolumeUp = 115,
    Power = 116, // SC System Power Down
    KpEqual = 117,
    KpPlusMinus = 118,
    Pause = 119,
    Scale = 120, // AL Compiz Scale (Expose)

    KpComma = 121,
    Hangeul = 122,
    Hanja = 123,
    Yen = 124,
    LeftMeta = 125,
    RightMeta = 126,
    Compose = 127,

    Stop = 128, // AC Stop
    Again = 129,
    Props = 130, // AC Properties
    Undo = 131, // AC Undo
    Front = 132,
    Copy = 133, // AC Copy
    Open = 134, // AC Open
    Paste = 135, // AC Paste
    Find = 136, // AC Search
    Cut = 137, // AC Cut
    Help = 138, // AL Integrated Help Center
    Menu = 139, // Menu (show menu)
    Calc = 140, // AL Calculator
    Setup = 141,
    Sleep = 142, // SC System Sleep
    Wakeup = 143, // System Wake Up
    File = 144, // AL Local Machine Browser
    Sendfile = 145,
    DeleteFile = 146,
    Xfer = 147,
    Prog1 = 148,
    Prog2 = 149,
    Www = 150, // AL Internet Browser
    MsDos = 151,
    Coffee = 152, // AL Terminal Lock/Screensaver
    RotateDisplay = 153, // Display orientation for e.g. tablets
    CycleWindows = 154,
    Mail = 155,
    Bookmarks = 156, // AC Bookmarks
    Computer = 157,
    Back = 158, // AC Back
    Forward = 159, // AC Forward
    CloseCd = 160,
    EjectCd = 161,
    EjectCloseCd = 162,
    NextSong = 163,
    PlayPause = 164,
    PreviousSong = 165,
    StopCd = 166,
    Record = 167,
    Rewind = 168,
    Phone = 169, // Media Select Telephone
    Iso = 170,
    Config = 171, // AL Consumer Control Configuration
    Homepage = 172, // AC Home
    Refresh = 173, // AC Refresh
    Exit = 174, // AC Exit
    Move = 175,
    Edit = 176,
    ScrollUp = 177,
    ScrollDown = 178,
    KpLeftParen = 179,
    KpRightParen = 180,
    New = 181, // AC New
    Redo = 182, // AC Redo/Repeat

    F13 = 183,
    F14 = 184,
    F15 = 185,
    F16 = 186,
    F17 = 187,
    F18 = 188,
    F19 = 189,
    F20 = 190,
    F21 = 191,
    F22 = 192,
    F23 = 193,
    F24 = 194,

    PlayCd = 200,
    PauseCd = 201,
    Prog3 = 202,
    Prog4 = 203,
    Dashboard = 204, // AL Dashboard
    Suspend = 205,
    Close = 206, // AC Close
    Play = 207,
    FastForward = 208,
    BassBoost = 209,
    Print = 210, // AC Print
    Hp = 211,
    Camera = 212,
    Sound = 213,
    Question = 214,
    Email = 215,
    Chat = 216,
    Search = 217,
    Connect = 218,
    Finance = 219, // AL Checkbook/Finance
    Sport = 220,
    Shop = 221,
    AltErase = 222,
    Cancel = 223, // AC Cancel
    BrightnessDown = 224,
    BrightnessUp = 225,
    Media = 226,

    SwitchVideoMode = 227, // Cycle between available video outputs (Monitor/LCD/TV-out/etc)
    KbdIllumToggle = 228,
    KbdIllumDown = 229,
    KbdIllumUp = 230,

    Send = 231, // AC Send
    Reply = 232, // AC Reply
    ForwardMail = 233, // AC Forward Msg
    Save = 234, // AC Save
    Documents = 235,

    Battery = 236,

    Bluetooth = 237,
    Wlan = 238,
    Uwb = 239,

    Unknown = 240,

    VideoNext = 241, // drive next video source
    VideoPrev = 242, // drive previous video source
    BrightnessCycle = 243, // brightness up, after max is min
    BrightnessAuto = 244, // Set Auto Brightness: manual brightness control is off, rely on ambient
    DisplayOff = 245, // display device to off state

    Wwan = 246, // Wireless WAN (LTE, UMTS, GSM, etc.)
    RfKill = 247, // Key that controls all radios

    MicMute = 248, // Mute / unmute the microphone
    LeftMouse = 272,
    RightMouse = 273,
    MiddleMouse = 274,
    MouseBack = 275,
    MouseForward = 276,

    ToolFinger = 325,
    ToolQuintTap = 328,
    Touch = 330,
    ToolDoubleTap = 333,
    ToolTripleTap = 334,
    ToolQuadTap = 335,
    Mic = 582
}