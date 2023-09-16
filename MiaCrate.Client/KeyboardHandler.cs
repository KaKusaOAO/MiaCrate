using MiaCrate.Client.Platform;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client;

public class KeyboardHandler
{
    private readonly Game _game;

    public KeyboardHandler(Game game)
    {
        _game = game;
    }

    public unsafe void Setup(Window* handle)
    {
        InputConstants.SetupKeyboardCallbacks(handle,
            (window, key, code, action, mods) => _game.Execute(() => KeyPress(window, key, code, action, mods)),
            (window, codepoint, modifiers) => _game.Execute(() => CharTyped(window, codepoint, modifiers)));
    }

    private unsafe void KeyPress(Window* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
    {
        
    }

    private unsafe void CharTyped(Window* window, uint codepoint, KeyModifiers modifiers)
    {
        foreach (var c in char.ConvertFromUtf32((int) codepoint))
        {
            
        }
    }
}