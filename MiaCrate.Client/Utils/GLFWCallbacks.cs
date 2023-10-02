﻿namespace MiaCrate.Client.Utils;

// ReSharper disable InconsistentNaming
public static class GLFWCallbacks
{
    public delegate void CursorPosCallback(IntPtr handle, int x, int y);

    public delegate void MouseButtonCallback(IntPtr handle, MouseButton button, InputAction action, KeyModifiers mods);

    public delegate void ScrollCallback(IntPtr handle, int x, int y);

    public unsafe delegate void DropCallback(IntPtr handle, int count, char* paths);

    public delegate void CharModsCallback(IntPtr handle, uint codepoint, KeyModifiers modifiers);

    public delegate void KeyCallback(IntPtr handle, Keys key, int scanCode, InputAction action, KeyModifiers mods);
}

public enum MouseButton
{
    Left = 1,
    Middle,
    Right,
    Button4,
    Button5,
    Button6,
    Button7,
    Button8
}

public enum InputAction
{
    Release,
    Press,
    Repeat
}

public enum CursorModeValue
{
    CursorNormal,
    CursorHidden,
    CursorDisabled
}

[Flags]
public enum KeyModifiers
{
    Shift = 1,
    Control = 2,
    Alt = 4,
    Super = 8,
    CapsLock = 16,
    NumLock = 32
}

public enum Keys
{
    Unknown = 0,
    A = 4,
    B = 5,
    C = 6,
    D = 7,
    E = 8,
    F = 9,
    G = 10, // 0x0000000A
    H = 11, // 0x0000000B
    I = 12, // 0x0000000C
    J = 13, // 0x0000000D
    K = 14, // 0x0000000E
    L = 15, // 0x0000000F
    M = 16, // 0x00000010
    N = 17, // 0x00000011
    O = 18, // 0x00000012
    P = 19, // 0x00000013
    Q = 20, // 0x00000014
    R = 21, // 0x00000015
    S = 22, // 0x00000016
    T = 23, // 0x00000017
    U = 24, // 0x00000018
    V = 25, // 0x00000019
    W = 26, // 0x0000001A
    X = 27, // 0x0000001B
    Y = 28, // 0x0000001C
    Z = 29, // 0x0000001D
    D1 = 30, // 0x0000001E
    D2 = 31, // 0x0000001F
    D3 = 32, // 0x00000020
    D4 = 33, // 0x00000021
    D5 = 34, // 0x00000022
    D6 = 35, // 0x00000023
    D7 = 36, // 0x00000024
    D8 = 37, // 0x00000025
    D9 = 38, // 0x00000026
    D0 = 39, // 0x00000027
    Enter = 40, // 0x00000028
    Escape = 41, // 0x00000029
    Backspace = 42, // 0x0000002A
    Tab = 43, // 0x0000002B
    Space = 44, // 0x0000002C
    Minus = 45, // 0x0000002D
    Equal = 46, // 0x0000002E
    LeftBracket = 47, // 0x0000002F
    RightBracket = 48, // 0x00000030
    Backslash = 49, // 0x00000031
    // SDL_SCANCODE_NONUSHASH = 50, // 0x00000032
    Semicolon = 51, // 0x00000033
    Apostrophe = 52, // 0x00000034
    GraveAccent = 53, // 0x00000035
    Comma = 54, // 0x00000036
    Period = 55, // 0x00000037
    Slash = 56, // 0x00000038
    CapsLock = 57, // 0x00000039
    F1 = 58, // 0x0000003A
    F2 = 59, // 0x0000003B
    F3 = 60, // 0x0000003C
    F4 = 61, // 0x0000003D
    F5 = 62, // 0x0000003E
    F6 = 63, // 0x0000003F
    F7 = 64, // 0x00000040
    F8 = 65, // 0x00000041
    F9 = 66, // 0x00000042
    F10 = 67, // 0x00000043
    F11 = 68, // 0x00000044
    F12 = 69, // 0x00000045
    PrintScreen = 70, // 0x00000046
    ScrollLock = 71, // 0x00000047
    Pause = 72, // 0x00000048
    Insert = 73, // 0x00000049
    Home = 74, // 0x0000004A
    PageUp = 75, // 0x0000004B
    Delete = 76, // 0x0000004C
    End = 77, // 0x0000004D
    PageDown = 78, // 0x0000004E
    Right = 79, // 0x0000004F
    Left = 80, // 0x00000050
    Down = 81, // 0x00000051
    Up = 82, // 0x00000052
    NumLock = 83, // 0x00000053
    KeyPadDivide = 84, // 0x00000054
    KeyPadMultiply = 85, // 0x00000055
    KeyPadSubtract = 86, // 0x00000056
    KeyPadAdd = 87, // 0x00000057
    KeyPadEnter = 88, // 0x00000058
    KeyPad1 = 89, // 0x00000059
    KeyPad2 = 90, // 0x0000005A
    KeyPad3 = 91, // 0x0000005B
    KeyPad4 = 92, // 0x0000005C
    KeyPad5 = 93, // 0x0000005D
    KeyPad6 = 94, // 0x0000005E
    KeyPad7 = 95, // 0x0000005F
    KeyPad8 = 96, // 0x00000060
    KeyPad9 = 97, // 0x00000061
    KeyPad0 = 98, // 0x00000062
    KeyPadDecimal = 99, // 0x00000063
    SDL_SCANCODE_NONUSBACKSLASH = 100, // 0x00000064
    SDL_SCANCODE_APPLICATION = 101, // 0x00000065
    SDL_SCANCODE_POWER = 102, // 0x00000066
    KeyPadEqual = 103, // 0x00000067
    F13 = 104, // 0x00000068
    F14 = 105, // 0x00000069
    F15 = 106, // 0x0000006A
    F16 = 107, // 0x0000006B
    F17 = 108, // 0x0000006C
    F18 = 109, // 0x0000006D
    F19 = 110, // 0x0000006E
    F20 = 111, // 0x0000006F
    F21 = 112, // 0x00000070
    F22 = 113, // 0x00000071
    F23 = 114, // 0x00000072
    F24 = 115, // 0x00000073
    SDL_SCANCODE_EXECUTE = 116, // 0x00000074
    SDL_SCANCODE_HELP = 117, // 0x00000075
    Menu = 118, // 0x00000076
    SDL_SCANCODE_SELECT = 119, // 0x00000077
    SDL_SCANCODE_STOP = 120, // 0x00000078
    SDL_SCANCODE_AGAIN = 121, // 0x00000079
    SDL_SCANCODE_UNDO = 122, // 0x0000007A
    SDL_SCANCODE_CUT = 123, // 0x0000007B
    SDL_SCANCODE_COPY = 124, // 0x0000007C
    SDL_SCANCODE_PASTE = 125, // 0x0000007D
    SDL_SCANCODE_FIND = 126, // 0x0000007E
    SDL_SCANCODE_MUTE = 127, // 0x0000007F
    SDL_SCANCODE_VOLUMEUP = 128, // 0x00000080
    SDL_SCANCODE_VOLUMEDOWN = 129, // 0x00000081
    SDL_SCANCODE_KP_COMMA = 133, // 0x00000085
    SDL_SCANCODE_KP_EQUALSAS400 = 134, // 0x00000086
    SDL_SCANCODE_INTERNATIONAL1 = 135, // 0x00000087
    SDL_SCANCODE_INTERNATIONAL2 = 136, // 0x00000088
    SDL_SCANCODE_INTERNATIONAL3 = 137, // 0x00000089
    SDL_SCANCODE_INTERNATIONAL4 = 138, // 0x0000008A
    SDL_SCANCODE_INTERNATIONAL5 = 139, // 0x0000008B
    SDL_SCANCODE_INTERNATIONAL6 = 140, // 0x0000008C
    SDL_SCANCODE_INTERNATIONAL7 = 141, // 0x0000008D
    SDL_SCANCODE_INTERNATIONAL8 = 142, // 0x0000008E
    SDL_SCANCODE_INTERNATIONAL9 = 143, // 0x0000008F
    SDL_SCANCODE_LANG1 = 144, // 0x00000090
    SDL_SCANCODE_LANG2 = 145, // 0x00000091
    SDL_SCANCODE_LANG3 = 146, // 0x00000092
    SDL_SCANCODE_LANG4 = 147, // 0x00000093
    SDL_SCANCODE_LANG5 = 148, // 0x00000094
    SDL_SCANCODE_LANG6 = 149, // 0x00000095
    SDL_SCANCODE_LANG7 = 150, // 0x00000096
    SDL_SCANCODE_LANG8 = 151, // 0x00000097
    SDL_SCANCODE_LANG9 = 152, // 0x00000098
    SDL_SCANCODE_ALTERASE = 153, // 0x00000099
    SDL_SCANCODE_SYSREQ = 154, // 0x0000009A
    SDL_SCANCODE_CANCEL = 155, // 0x0000009B
    SDL_SCANCODE_CLEAR = 156, // 0x0000009C
    SDL_SCANCODE_PRIOR = 157, // 0x0000009D
    SDL_SCANCODE_RETURN2 = 158, // 0x0000009E
    SDL_SCANCODE_SEPARATOR = 159, // 0x0000009F
    SDL_SCANCODE_OUT = 160, // 0x000000A0
    SDL_SCANCODE_OPER = 161, // 0x000000A1
    SDL_SCANCODE_CLEARAGAIN = 162, // 0x000000A2
    SDL_SCANCODE_CRSEL = 163, // 0x000000A3
    SDL_SCANCODE_EXSEL = 164, // 0x000000A4
    SDL_SCANCODE_KP_00 = 176, // 0x000000B0
    SDL_SCANCODE_KP_000 = 177, // 0x000000B1
    SDL_SCANCODE_THOUSANDSSEPARATOR = 178, // 0x000000B2
    SDL_SCANCODE_DECIMALSEPARATOR = 179, // 0x000000B3
    SDL_SCANCODE_CURRENCYUNIT = 180, // 0x000000B4
    SDL_SCANCODE_CURRENCYSUBUNIT = 181, // 0x000000B5
    SDL_SCANCODE_KP_LEFTPAREN = 182, // 0x000000B6
    SDL_SCANCODE_KP_RIGHTPAREN = 183, // 0x000000B7
    SDL_SCANCODE_KP_LEFTBRACE = 184, // 0x000000B8
    SDL_SCANCODE_KP_RIGHTBRACE = 185, // 0x000000B9
    SDL_SCANCODE_KP_TAB = 186, // 0x000000BA
    SDL_SCANCODE_KP_BACKSPACE = 187, // 0x000000BB
    SDL_SCANCODE_KP_A = 188, // 0x000000BC
    SDL_SCANCODE_KP_B = 189, // 0x000000BD
    SDL_SCANCODE_KP_C = 190, // 0x000000BE
    SDL_SCANCODE_KP_D = 191, // 0x000000BF
    SDL_SCANCODE_KP_E = 192, // 0x000000C0
    SDL_SCANCODE_KP_F = 193, // 0x000000C1
    SDL_SCANCODE_KP_XOR = 194, // 0x000000C2
    SDL_SCANCODE_KP_POWER = 195, // 0x000000C3
    SDL_SCANCODE_KP_PERCENT = 196, // 0x000000C4
    SDL_SCANCODE_KP_LESS = 197, // 0x000000C5
    SDL_SCANCODE_KP_GREATER = 198, // 0x000000C6
    SDL_SCANCODE_KP_AMPERSAND = 199, // 0x000000C7
    SDL_SCANCODE_KP_DBLAMPERSAND = 200, // 0x000000C8
    SDL_SCANCODE_KP_VERTICALBAR = 201, // 0x000000C9
    SDL_SCANCODE_KP_DBLVERTICALBAR = 202, // 0x000000CA
    SDL_SCANCODE_KP_COLON = 203, // 0x000000CB
    SDL_SCANCODE_KP_HASH = 204, // 0x000000CC
    SDL_SCANCODE_KP_SPACE = 205, // 0x000000CD
    SDL_SCANCODE_KP_AT = 206, // 0x000000CE
    SDL_SCANCODE_KP_EXCLAM = 207, // 0x000000CF
    SDL_SCANCODE_KP_MEMSTORE = 208, // 0x000000D0
    SDL_SCANCODE_KP_MEMRECALL = 209, // 0x000000D1
    SDL_SCANCODE_KP_MEMCLEAR = 210, // 0x000000D2
    SDL_SCANCODE_KP_MEMADD = 211, // 0x000000D3
    SDL_SCANCODE_KP_MEMSUBTRACT = 212, // 0x000000D4
    SDL_SCANCODE_KP_MEMMULTIPLY = 213, // 0x000000D5
    SDL_SCANCODE_KP_MEMDIVIDE = 214, // 0x000000D6
    SDL_SCANCODE_KP_PLUSMINUS = 215, // 0x000000D7
    SDL_SCANCODE_KP_CLEAR = 216, // 0x000000D8
    SDL_SCANCODE_KP_CLEARENTRY = 217, // 0x000000D9
    SDL_SCANCODE_KP_BINARY = 218, // 0x000000DA
    SDL_SCANCODE_KP_OCTAL = 219, // 0x000000DB
    SDL_SCANCODE_KP_DECIMAL = 220, // 0x000000DC
    SDL_SCANCODE_KP_HEXADECIMAL = 221, // 0x000000DD
    LeftControl = 224, // 0x000000E0
    LeftShift = 225, // 0x000000E1
    LeftAlt = 226, // 0x000000E2
    LeftSuper = 227, // 0x000000E3
    RightControl = 228, // 0x000000E4
    RightShift = 229, // 0x000000E5
    RightAlt = 230, // 0x000000E6
    RightSuper = 231, // 0x000000E7
    SDL_SCANCODE_MODE = 257, // 0x00000101
    SDL_SCANCODE_AUDIONEXT = 258, // 0x00000102
    SDL_SCANCODE_AUDIOPREV = 259, // 0x00000103
    SDL_SCANCODE_AUDIOSTOP = 260, // 0x00000104
    SDL_SCANCODE_AUDIOPLAY = 261, // 0x00000105
    SDL_SCANCODE_AUDIOMUTE = 262, // 0x00000106
    SDL_SCANCODE_MEDIASELECT = 263, // 0x00000107
    SDL_SCANCODE_WWW = 264, // 0x00000108
    SDL_SCANCODE_MAIL = 265, // 0x00000109
    SDL_SCANCODE_CALCULATOR = 266, // 0x0000010A
    SDL_SCANCODE_COMPUTER = 267, // 0x0000010B
    SDL_SCANCODE_AC_SEARCH = 268, // 0x0000010C
    SDL_SCANCODE_AC_HOME = 269, // 0x0000010D
    SDL_SCANCODE_AC_BACK = 270, // 0x0000010E
    SDL_SCANCODE_AC_FORWARD = 271, // 0x0000010F
    SDL_SCANCODE_AC_STOP = 272, // 0x00000110
    SDL_SCANCODE_AC_REFRESH = 273, // 0x00000111
    SDL_SCANCODE_AC_BOOKMARKS = 274, // 0x00000112
    SDL_SCANCODE_BRIGHTNESSDOWN = 275, // 0x00000113
    SDL_SCANCODE_BRIGHTNESSUP = 276, // 0x00000114
    SDL_SCANCODE_DISPLAYSWITCH = 277, // 0x00000115
    SDL_SCANCODE_KBDILLUMTOGGLE = 278, // 0x00000116
    SDL_SCANCODE_KBDILLUMDOWN = 279, // 0x00000117
    SDL_SCANCODE_KBDILLUMUP = 280, // 0x00000118
    SDL_SCANCODE_EJECT = 281, // 0x00000119
    SDL_SCANCODE_SLEEP = 282, // 0x0000011A
    SDL_SCANCODE_APP1 = 283, // 0x0000011B
    SDL_SCANCODE_APP2 = 284, // 0x0000011C
    SDL_SCANCODE_AUDIOREWIND = 285, // 0x0000011D
    SDL_SCANCODE_AUDIOFASTFORWARD = 286, // 0x0000011E
    SDL_NUM_SCANCODES = 512, // 0x00000200
    
    F25 = 0
}