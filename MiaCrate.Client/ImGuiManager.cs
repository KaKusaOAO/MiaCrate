using System.Numerics;
using ImGuiNET;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Utils;
using Veldrid;
using MouseButton = MiaCrate.Client.Utils.MouseButton;

namespace MiaCrate.Client;

public class ImGuiManager
{
    private readonly Game _game;
    private readonly GraphicsDevice _gd;
    private readonly CommandList _cl;
    private readonly ImGuiRenderer _renderer;
    private readonly InputSnapshotImpl _snapshot;
    private DateTimeOffset _lastUpdateNow = DateTimeOffset.Now;

    public ImGuiManager(Game game)
    {
        _game = game;
        _snapshot = new InputSnapshotImpl(game);
        _gd = GlStateManager.Device;
        _cl = _gd.ResourceFactory.CreateCommandList();
        
        var fb = _gd.SwapchainFramebuffer;
        var outputDesc = fb.OutputDescription;
        _renderer = new ImGuiRenderer(_gd, outputDesc, (int) fb.Width, (int) fb.Height);
    }

    public void Render()
    {
        var now = DateTimeOffset.Now;
        var delta = (float) (now - _lastUpdateNow).TotalSeconds;
        _lastUpdateNow = now;

        var fb = _gd.SwapchainFramebuffer;

        _renderer.WindowResized((int) fb.Width, (int) fb.Height);
        _renderer.Update(delta, _snapshot);
        RenderContent();
        
        _cl.Begin();
        _cl.SetFramebuffer(fb);
        _renderer.Render(_gd, _cl);
        _cl.End();
        
        _gd.SubmitCommands(_cl);
        _gd.WaitForIdle();
    }

    private void RenderContent()
    {
        if (ImGui.BeginMainMenuBar())
        {
            ImGui.EndMainMenuBar();
        }
        
        if (ImGui.Begin($"{MiaCore.ProductName} - Debug Detail"))
        {
            var style = ImGui.GetStyle();
            var indent = style.IndentSpacing;

            if (ImGui.CollapsingHeader("Backend Info", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                ImGui.Text($"Backend type: {_gd.BackendType}");
                ImGui.Text($"Device Name: {_gd.DeviceName}");
                ImGui.Text($"Vendor Name: {_gd.VendorName}");

                if (ImGui.CollapsingHeader("Backend-specific Info"))
                {
                    ImGui.Indent();
                    if (_gd.BackendType == GraphicsBackend.Vulkan)
                    {
                        var info = _gd.GetVulkanInfo();
                        ImGui.Text($"Driver Name: {info.DriverName}");
                        ImGui.Text($"Driver Info: {info.DriverInfo}");
                    }
                    else if (_gd.BackendType is GraphicsBackend.OpenGL or GraphicsBackend.OpenGLES)
                    {
                        var info = _gd.GetOpenGLInfo();
                        ImGui.Text($"GL Version: {info.Version}");
                        ImGui.Text($"GLSL Version: {info.ShadingLanguageVersion}");
                    }
                    else
                    {
                        ImGui.Text($"No info available :(");
                    }
                    
                    ImGui.Unindent();
                }

                ImGui.Unindent();
            }
            
            if (ImGui.CollapsingHeader("Framebuffer Debug", ImGuiTreeNodeFlags.DefaultOpen))
            {
                var texture = _game.MainRenderTarget.ColorTexture;
                var image = _renderer.GetOrCreateImGuiBinding(_gd.ResourceFactory, texture!.TextureView);

                var width = ImGui.GetWindowWidth() - style.WindowPadding.X * 2;
                var height = width / texture.Texture.Width * texture.Texture.Height;
                ImGui.Image(image, new Vector2(width, height));
            }
            
            ImGui.End();
        }
    }

    private class InputSnapshotImpl : InputSnapshot
    {
        private readonly Game _game;
        private readonly Dictionary<MouseButton, bool> _buttonStates = new();
        private float _wheelDelta;

        public InputSnapshotImpl(Game game)
        {
            _game = game;

            InputConstants.MouseButtonChanged += (_, button, action, mods) =>
            {
                _buttonStates[button] = action == InputAction.Press;
            };

            InputConstants.Scrolled += (_, x, y) =>
            {
                _wheelDelta += y * 0.75f;
            };
        }

        private MouseButton FromVeldridButton(Veldrid.MouseButton button)
        {
            return button switch
            {
                Veldrid.MouseButton.Left => MouseButton.Left,
                Veldrid.MouseButton.Middle => MouseButton.Middle,
                Veldrid.MouseButton.Right => MouseButton.Right,
                Veldrid.MouseButton.Button4 => MouseButton.Button4,
                Veldrid.MouseButton.Button5 => MouseButton.Button5,
                _ => (MouseButton) (-1)
            };
        }

        public bool IsMouseDown(Veldrid.MouseButton button) => 
            _buttonStates.GetValueOrDefault(FromVeldridButton(button), false);

        public IReadOnlyList<KeyEvent> KeyEvents => new List<KeyEvent>();

        public IReadOnlyList<MouseEvent> MouseEvents => new List<MouseEvent>();

        public IReadOnlyList<char> KeyCharPresses => new List<char>();

        public Vector2 MousePosition => new(
            (float) (_game.MouseHandler.XPos), 
            (float) (_game.MouseHandler.YPos));

        public float WheelDelta
        {
            get
            {
                var val = _wheelDelta;
                _wheelDelta = 0;
                return val;
            }
        }
    }
}