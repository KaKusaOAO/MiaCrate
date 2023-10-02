using MiaCrate.Client.Utils;
using MiaCrate.Extensions;
using MiaCrate.Localizations;
using MiaCrate.Texts;
using Mochi.Texts;
using SDL2;

namespace MiaCrate.Client.Platform;

public static class InputConstants
{
	public const int Press = 1;
	public const int Release = 0;
	public const int Repeat = 2;
	public const int ModControl = 2;
	public static readonly Key Unknown = KeyType.KeySym.GetOrCreate(-1);

	public static bool IsRawMouseInputSupported => true;

	private static GLFWCallbacks.KeyCallback? _keyCallback;
	private static GLFWCallbacks.CharModsCallback? _charModsCallback;
	private static GLFWCallbacks.CursorPosCallback? _cursorPosCallback;
	private static GLFWCallbacks.MouseButtonCallback? _mouseButtonCallback;
	private static GLFWCallbacks.ScrollCallback? _scrollCallback;
	private static GLFWCallbacks.DropCallback? _dropCallback;

	static InputConstants()
	{
		Window.ReceiveSDLEvent += HandleEvent;
	}

	private static void HandleEvent(SDL.SDL_Event ev)
	{
		switch (ev.type)
		{
			case SDL.SDL_EventType.SDL_MOUSEMOTION:
				HandleMouseMotionEvent(ev.motion);
				break;
			case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
			case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
				HandleMouseButtonEvent(ev.button);
				break;
			case SDL.SDL_EventType.SDL_DROPFILE:
				HandleDropEvent(ev.drop);
				break;
			case SDL.SDL_EventType.SDL_KEYUP:
			case SDL.SDL_EventType.SDL_KEYDOWN:
				HandleKeyboardButtonEvent(ev.key);
				break;
		}
	}

	private static void HandleMouseMotionEvent(SDL.SDL_MouseMotionEvent ev)
	{
		var handle = Game.Instance.Window.Handle;
		// TODO: Process with SDL.SDL_GetRelativeMouseMode()
		_cursorPosCallback?.Invoke(handle, ev.x, ev.y);
	}

	private static void HandleDropEvent(SDL.SDL_DropEvent ev)
	{
		unsafe
		{
			var handle = Game.Instance.Window.Handle;
			_dropCallback?.Invoke(handle, 1, (char*) ev.file);
		}
	}

	private static void HandleKeyboardButtonEvent(SDL.SDL_KeyboardEvent ev)
	{
		var key = ev.keysym;
		var scancode = key.scancode;
		var keymod = key.mod;
		var handle = Game.Instance.Window.Handle;
		var mods = (KeyModifiers) 0;

		if ((keymod & SDL.SDL_Keymod.KMOD_SHIFT) == SDL.SDL_Keymod.KMOD_SHIFT)
			mods |= KeyModifiers.Shift;
		
		if ((keymod & SDL.SDL_Keymod.KMOD_CTRL) == SDL.SDL_Keymod.KMOD_CTRL)
			mods |= KeyModifiers.Control;
		
		if ((keymod & SDL.SDL_Keymod.KMOD_ALT) == SDL.SDL_Keymod.KMOD_ALT)
			mods |= KeyModifiers.Alt;

		if ((keymod & SDL.SDL_Keymod.KMOD_GUI) == SDL.SDL_Keymod.KMOD_GUI)
			mods |= KeyModifiers.Super;

		if (IsKeyDown(handle, Keys.CapsLock))
			mods |= KeyModifiers.CapsLock;
		
		if (IsKeyDown(handle, Keys.NumLock))
			mods |= KeyModifiers.NumLock;

		var pressed = ev.state == SDL.SDL_PRESSED;
		var action = ev.repeat == 0 ? (pressed ? InputAction.Press : InputAction.Release) : InputAction.Repeat;
		
		_keyCallback?.Invoke(handle, (Keys) scancode, (int) scancode, action, mods);
		_charModsCallback?.Invoke(handle, key.unicode, mods);
	}

	private static byte _lastState;

	private static void HandleMouseButtonEvent(SDL.SDL_MouseButtonEvent ev)
	{
		var state = ev.state;
		var handle = Game.Instance.Window.Handle;

		if (state == SDL.SDL_PRESSED)
		{
			_mouseButtonCallback?.Invoke(handle, (MouseButton) ev.button, InputAction.Press, 0);
		}
		else
		{
			_mouseButtonCallback?.Invoke(handle, (MouseButton) ev.button, InputAction.Release, 0);
		}

		_lastState = state;
	}

	public static void GrabOrReleaseMouse(IntPtr handle, CursorModeValue value, double xPos, double yPos)
	{
		var showCursor = value == CursorModeValue.CursorNormal
			? SDL.SDL_ENABLE
			: SDL.SDL_DISABLE;

		var relative = value == CursorModeValue.CursorDisabled
			? SDL.SDL_bool.SDL_TRUE
			: SDL.SDL_bool.SDL_FALSE;
		
		SDL.SDL_SetRelativeMouseMode(relative);
		SDL.SDL_ShowCursor(showCursor);
	}

	public static void SetupKeyboardCallbacks(IntPtr handle,
		GLFWCallbacks.KeyCallback keyCallback,
		GLFWCallbacks.CharModsCallback charModsCallback)
	{
		_keyCallback = keyCallback;
		_charModsCallback = charModsCallback;
	}
	
	public static void SetupMouseCallbacks(IntPtr handle,
		GLFWCallbacks.CursorPosCallback cursorPosCallback,
		GLFWCallbacks.MouseButtonCallback mouseButtonCallback,
		GLFWCallbacks.ScrollCallback scrollCallback,
		GLFWCallbacks.DropCallback dropCallback)
	{
		_cursorPosCallback = cursorPosCallback;
		_mouseButtonCallback = mouseButtonCallback;
		_scrollCallback = scrollCallback;
		_dropCallback = dropCallback;
	}

	public static bool IsKeyDown(IntPtr handle, Keys key)
	{
		unsafe
		{
			var ptr = (byte*) SDL.SDL_GetKeyboardState(out var keys);
			if ((int) key >= keys)
			{
				throw new ArgumentException("Key index overflow!");
			}

			return ptr[(int) key] == 1;
		}
	}

	public class KeyType
    {
        public const string KeyKeyboardUnknown = "key.keyboard.unknown";
        
        public static KeyType KeySym { get; } = new("key.keyboard", (i, s) =>
        {
            if (s == KeyKeyboardUnknown) return TranslateText.Of(s);

            var name = SDL.SDL_GetScancodeName((SDL.SDL_Scancode) i);
            return !string.IsNullOrEmpty(name)
                ? Component.Literal(name.ToUpperInvariant())
                : TranslateText.Of(s);
        });

        public static KeyType ScanCode { get; } = new("scancode", (i, s) =>
        {
            var name = SDL.SDL_GetScancodeName((SDL.SDL_Scancode) i);
            return !string.IsNullOrEmpty(name)
                ? Component.Literal(name)
                : TranslateText.Of(s);
        });

        public static KeyType Mouse { get; } = new("key.mouse", (i, s) => Language.Instance.Has(s)
            ? TranslateText.Of(s)
            : TranslateText.Of("key.mouse", Component.Literal($"{i + 1}")));

        private readonly Dictionary<int, Key> _map = new();
        private readonly string _defaultPrefix;
        internal readonly Func<int, string, IComponent> displayText;

        private KeyType(string defaultPrefix, Func<int, string, IComponent> displayText)
        {
            _defaultPrefix = defaultPrefix;
            this.displayText = displayText;
        }

        private static void AddKey(KeyType type, string s, int i)
        {
            var key = new Key(s, type, i);
            type._map[i] = key;
        }

        private static void AddKey(KeyType type, string s, Keys key) => AddKey(type, s, (int) key);
        private static void AddKey(KeyType type, string s, MouseButton button) => AddKey(type, s, (int) button);

        public Key GetOrCreate(int value)
        {
            return _map.ComputeIfAbsent(value, v =>
            {
                var j = v;
                if (this == Mouse) j++;

                var s = $"{_defaultPrefix}.{j}";
                return new Key(s, this, v);
            });
        }

        static KeyType()
        {
            AddKey(KeySym, KeyKeyboardUnknown, -1);
            AddKey(Mouse, "key.mouse.left", MouseButton.Left);
            AddKey(Mouse, "key.mouse.right", MouseButton.Right);
            AddKey(Mouse, "key.mouse.middle", MouseButton.Middle);
            AddKey(Mouse, "key.mouse.4", MouseButton.Button4);
            AddKey(Mouse, "key.mouse.5", MouseButton.Button5);
            AddKey(Mouse, "key.mouse.6", MouseButton.Button6);
            AddKey(Mouse, "key.mouse.7", MouseButton.Button7);
            AddKey(Mouse, "key.mouse.8", MouseButton.Button8);
            AddKey(KeySym, "key.keyboard.0", Keys.D0);
			AddKey(KeySym, "key.keyboard.1", Keys.D1);
			AddKey(KeySym, "key.keyboard.2", Keys.D2);
			AddKey(KeySym, "key.keyboard.3", Keys.D3);
			AddKey(KeySym, "key.keyboard.4", Keys.D4);
			AddKey(KeySym, "key.keyboard.5", Keys.D5);
			AddKey(KeySym, "key.keyboard.6", Keys.D6);
			AddKey(KeySym, "key.keyboard.7", Keys.D7);
			AddKey(KeySym, "key.keyboard.8", Keys.D8);
			AddKey(KeySym, "key.keyboard.9", Keys.D9);
			AddKey(KeySym, "key.keyboard.a", Keys.A);
			AddKey(KeySym, "key.keyboard.b", Keys.B);
			AddKey(KeySym, "key.keyboard.c", Keys.C);
			AddKey(KeySym, "key.keyboard.d", Keys.D);
			AddKey(KeySym, "key.keyboard.e", Keys.E);
			AddKey(KeySym, "key.keyboard.f", Keys.F);
			AddKey(KeySym, "key.keyboard.g", Keys.G);
			AddKey(KeySym, "key.keyboard.h", Keys.H);
			AddKey(KeySym, "key.keyboard.i", Keys.I);
			AddKey(KeySym, "key.keyboard.j", Keys.J);
			AddKey(KeySym, "key.keyboard.k", Keys.K);
			AddKey(KeySym, "key.keyboard.l", Keys.L);
			AddKey(KeySym, "key.keyboard.m", Keys.M);
			AddKey(KeySym, "key.keyboard.n", Keys.N);
			AddKey(KeySym, "key.keyboard.o", Keys.O);
			AddKey(KeySym, "key.keyboard.p", Keys.P);
			AddKey(KeySym, "key.keyboard.q", Keys.Q);                    
			AddKey(KeySym, "key.keyboard.r", Keys.R);
			AddKey(KeySym, "key.keyboard.s", Keys.S);
			AddKey(KeySym, "key.keyboard.t", Keys.T);
			AddKey(KeySym, "key.keyboard.u", Keys.U);
			AddKey(KeySym, "key.keyboard.v", Keys.V);
			AddKey(KeySym, "key.keyboard.w", Keys.W);
			AddKey(KeySym, "key.keyboard.x", Keys.X);
			AddKey(KeySym, "key.keyboard.y", Keys.Y);
			AddKey(KeySym, "key.keyboard.z", Keys.Z);
			AddKey(KeySym, "key.keyboard.f1", Keys.F1);
			AddKey(KeySym, "key.keyboard.f2", Keys.F2);
			AddKey(KeySym, "key.keyboard.f3", Keys.F3);
			AddKey(KeySym, "key.keyboard.f4", Keys.F4);
			AddKey(KeySym, "key.keyboard.f5", Keys.F5);
			AddKey(KeySym, "key.keyboard.f6", Keys.F6);
			AddKey(KeySym, "key.keyboard.f7", Keys.F7);
			AddKey(KeySym, "key.keyboard.f8", Keys.F8);
			AddKey(KeySym, "key.keyboard.f9", Keys.F9);
			AddKey(KeySym, "key.keyboard.f10", Keys.F10);
			AddKey(KeySym, "key.keyboard.f11", Keys.F11);
			AddKey(KeySym, "key.keyboard.f12", Keys.F12);
			AddKey(KeySym, "key.keyboard.f13", Keys.F13);
			AddKey(KeySym, "key.keyboard.f14", Keys.F14);
			AddKey(KeySym, "key.keyboard.f15", Keys.F15);
			AddKey(KeySym, "key.keyboard.f16", Keys.F16);
			AddKey(KeySym, "key.keyboard.f17", Keys.F17);
			AddKey(KeySym, "key.keyboard.f18", Keys.F18);
			AddKey(KeySym, "key.keyboard.f19", Keys.F19);
			AddKey(KeySym, "key.keyboard.f20", Keys.F20);
			AddKey(KeySym, "key.keyboard.f21", Keys.F21);
			AddKey(KeySym, "key.keyboard.f22", Keys.F22);
			AddKey(KeySym, "key.keyboard.f23", Keys.F23);
			AddKey(KeySym, "key.keyboard.f24", Keys.F24);
			AddKey(KeySym, "key.keyboard.f25", Keys.F25);
			AddKey(KeySym, "key.keyboard.num.lock", Keys.NumLock);
			AddKey(KeySym, "key.keyboard.keypad.0", Keys.KeyPad0);
			AddKey(KeySym, "key.keyboard.keypad.1", Keys.KeyPad1);
			AddKey(KeySym, "key.keyboard.keypad.2", Keys.KeyPad2);
			AddKey(KeySym, "key.keyboard.keypad.3", Keys.KeyPad3);
			AddKey(KeySym, "key.keyboard.keypad.4", Keys.KeyPad4);
			AddKey(KeySym, "key.keyboard.keypad.5", Keys.KeyPad5);
			AddKey(KeySym, "key.keyboard.keypad.6", Keys.KeyPad6);
			AddKey(KeySym, "key.keyboard.keypad.7", Keys.KeyPad7);
			AddKey(KeySym, "key.keyboard.keypad.8", Keys.KeyPad8);
			AddKey(KeySym, "key.keyboard.keypad.9", Keys.KeyPad9);
			AddKey(KeySym, "key.keyboard.keypad.add", Keys.KeyPadAdd);
			AddKey(KeySym, "key.keyboard.keypad.decimal", Keys.KeyPadDecimal);
			AddKey(KeySym, "key.keyboard.keypad.enter", Keys.KeyPadEnter);
			AddKey(KeySym, "key.keyboard.keypad.equal", Keys.KeyPadEqual);
			AddKey(KeySym, "key.keyboard.keypad.multiply", Keys.KeyPadMultiply);
			AddKey(KeySym, "key.keyboard.keypad.divide", Keys.KeyPadDivide);
			AddKey(KeySym, "key.keyboard.keypad.subtract", Keys.KeyPadSubtract);
			AddKey(KeySym, "key.keyboard.down", Keys.Down);
			AddKey(KeySym, "key.keyboard.left", Keys.Left);
			AddKey(KeySym, "key.keyboard.right", Keys.Right);
			AddKey(KeySym, "key.keyboard.up", Keys.Up);
			AddKey(KeySym, "key.keyboard.apostrophe", Keys.Apostrophe);
			AddKey(KeySym, "key.keyboard.backslash", Keys.Backslash);
			AddKey(KeySym, "key.keyboard.comma", Keys.Comma);
			AddKey(KeySym, "key.keyboard.equal", Keys.Equal);
			AddKey(KeySym, "key.keyboard.grave.accent", Keys.GraveAccent);
			AddKey(KeySym, "key.keyboard.left.bracket", Keys.LeftBracket);
			AddKey(KeySym, "key.keyboard.minus", Keys.Minus);
			AddKey(KeySym, "key.keyboard.period", Keys.Period);
			AddKey(KeySym, "key.keyboard.right.bracket", Keys.RightBracket);
			AddKey(KeySym, "key.keyboard.semicolon", Keys.Semicolon);
			AddKey(KeySym, "key.keyboard.slash", Keys.Slash);
			AddKey(KeySym, "key.keyboard.space", Keys.Space);
			AddKey(KeySym, "key.keyboard.tab", Keys.Tab);
			AddKey(KeySym, "key.keyboard.left.alt", Keys.LeftAlt);
			AddKey(KeySym, "key.keyboard.left.control", Keys.LeftControl);
			AddKey(KeySym, "key.keyboard.left.shift", Keys.LeftShift);
			AddKey(KeySym, "key.keyboard.left.win", Keys.LeftSuper);
			AddKey(KeySym, "key.keyboard.right.alt", Keys.RightAlt);
			AddKey(KeySym, "key.keyboard.right.control", Keys.RightControl);
			AddKey(KeySym, "key.keyboard.right.shift", Keys.RightShift);
			AddKey(KeySym, "key.keyboard.right.win", Keys.RightSuper);
			AddKey(KeySym, "key.keyboard.enter", Keys.Enter);
			AddKey(KeySym, "key.keyboard.escape", Keys.Escape);
			AddKey(KeySym, "key.keyboard.backspace", Keys.Backspace);
			AddKey(KeySym, "key.keyboard.delete", Keys.Delete);
			AddKey(KeySym, "key.keyboard.end", Keys.End);
			AddKey(KeySym, "key.keyboard.home", Keys.Home);
			AddKey(KeySym, "key.keyboard.insert", Keys.Insert);
			AddKey(KeySym, "key.keyboard.page.down", Keys.PageDown);
			AddKey(KeySym, "key.keyboard.page.up", Keys.PageUp);
			AddKey(KeySym, "key.keyboard.caps.lock", Keys.CapsLock);
			AddKey(KeySym, "key.keyboard.pause", Keys.Pause);
			AddKey(KeySym, "key.keyboard.scroll.lock", Keys.ScrollLock);
			AddKey(KeySym, "key.keyboard.menu", Keys.Menu);
			AddKey(KeySym, "key.keyboard.print.screen", Keys.PrintScreen);
			AddKey(KeySym, "key.keyboard.world.1", 161);
			AddKey(KeySym, "key.keyboard.world.2", 162);
        }
    }

    public class Key
    {
        internal static readonly Dictionary<string, Key> NameMap = new();
            
        private readonly string _name;
        public KeyType Type { get; }
        public int Value { get; }
        private readonly Lazy<IComponent> _displayName;

        internal Key(string name, KeyType type, int value)
        {
            _name = name;
            Type = type;
            Value = value;
            _displayName = new Lazy<IComponent>(() => type.displayText(value, name));
            NameMap[name] = this;
        }
    }
}