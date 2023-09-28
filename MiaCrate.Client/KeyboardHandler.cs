using MiaCrate.Client.Platform;
using MiaCrate.Client.Utils;

namespace MiaCrate.Client;

public class KeyboardHandler
{
    private readonly Game _game;

    public KeyboardHandler(Game game)
    {
        _game = game;
    }

    public unsafe void Setup(IntPtr handle)
    {
        InputConstants.SetupKeyboardCallbacks(handle,
            (window, key, code, action, mods) => _game.Execute(() => KeyPress(window, key, code, action, mods)),
            (window, codepoint, modifiers) => _game.Execute(() => CharTyped(window, codepoint, modifiers)));
    }

    private unsafe void KeyPress(IntPtr window, Keys key, int scancode, InputAction action, KeyModifiers mods)
    {
        
    }

    private unsafe void CharTyped(IntPtr window, uint codepoint, KeyModifiers modifiers)
    {
        foreach (var c in char.ConvertFromUtf32((int) codepoint))
        {
            
        }
    }
}