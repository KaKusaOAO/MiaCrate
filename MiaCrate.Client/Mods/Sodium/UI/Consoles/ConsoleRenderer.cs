using MiaCrate.Client.Platform;
using MiaCrate.Client.UI;
using MiaCrate.Client.Utils;
using MiaCrate.Extensions;
using MiaCrate.Localizations;
using Mochi.Texts;
using Mochi.Utils;
using SDL2;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Client.Sodium.UI;

public class ConsoleRenderer
{
    public static ConsoleRenderer Instance { get; } = new();

    private static Dictionary<MessageLevel, ColorPalette> Colors { get; } = new Dictionary<MessageLevel, ColorPalette>
    {
        [MessageLevel.Info] = new(
            new Argb32(255, 255, 255),
            new Argb32(15, 15, 15),
            new Argb32(15, 15, 15)),

        [MessageLevel.Warn] = new(
            new Argb32(244, 187, 0),
            new Argb32(25, 21, 0),
            new Argb32(180, 150, 0)),
        
        [MessageLevel.Severe] = new(
            new Argb32(220, 0, 0),
            new Argb32(25, 0, 0),
            new Argb32(160, 0, 0)),
    };

    private readonly List<ActiveMessage> _activeMessages = new();
    
    private ConsoleRenderer() {}

    public void Update(Console console, double currentTime)
    {
        PurgeMessages(currentTime);
        PollMessages(console, currentTime);
    }

    private void PurgeMessages(double currentTime)
    {
        _activeMessages.RemoveAll(m => currentTime > m.Timestamp + m.Duration);
    }

    private void PollMessages(Console console, double currentTime)
    {
        var log = console.Messages;

        while (log.TryDequeue(out var message))
        {
            _activeMessages.Add(ActiveMessage.Create(message, currentTime));
        }
    }

    public void Render(GuiGraphics graphics)
    {
        var currentTime = SDL.SDL_GetTicks() / 1000.0;
        // var currentTime = GLFW.GetTime();

        var game = Game.Instance;
        var pose = graphics.Pose;
        
        pose.PushPose();
        pose.Translate(0, 0, 1000);

        var paddingWidth = 3;
        var paddingHeight = 1;

        var renders = new List<MessageRender>();

        {
            var x = 4;
            var y = 4;
            
            foreach (var message in _activeMessages)
            {
                var opacity = GetMessageOpacity(message, currentTime);
                if (opacity < 0.025) continue;

                var lines = new List<FormattedCharSequence>();
                var width = 270;

                var splitter = game.Font.Splitter;
                splitter.SplitLines(message.Text.AsFormattedText(), width - 20, Style.Empty, (text, b) =>
                {
                    lines.Add(Language.Instance.GetVisualOrder(text));
                });

                var height = Font.LineHeight * lines.Count + paddingHeight * 2;
                renders.Add(new MessageRender(x, y, width, height, message.Level, lines, opacity));
                y += height;
            }
        }

        var mouseX = game.MouseHandler.XPos / game.Window.GuiScale;
        var mouseY = game.MouseHandler.YPos / game.Window.GuiScale;
        var hovered = renders.Any(render => mouseX >= render.X && mouseX < render.X + render.Width && mouseY >= render.Y && mouseY < render.Y + render.Height);
        
        foreach (var (x, sourceY, width, height, level, lines, sourceOpacity) in renders)
        {
            var y = sourceY;
            var opacity = sourceOpacity;
            var colors = Colors[level];

            if (hovered)
            {
                opacity *= 0.4;
            }
            
            // message background
            graphics.Fill(x, y, x + width, y + height, colors.Background.WithAlpha(opacity));
            
            // message colored stripe
            graphics.Fill(x, y, x + 1, y + height, colors.Foreground.WithAlpha(opacity));
            
            foreach (var line in lines)
            {
                // message text
                graphics.DrawString(game.Font, line, x + paddingWidth + 3, y + paddingHeight,
                    colors.Text.WithAlpha(opacity), false);

                y += Font.LineHeight;
            }
        }
        
        pose.PopPose();
    }

    private static double GetMessageOpacity(ActiveMessage message, double time)
    {
        var midpoint = message.Timestamp + message.Duration / 2;

        if (time > midpoint)
            return GetFadeOutOpacity(message, time);

        if (time < midpoint)
            return GetFadeInOpacity(message, time);

        return 1;
    }

    private static double GetFadeInOpacity(ActiveMessage message, double time)
    {
        var animDuration = 0.25;
        var animStart = message.Timestamp;
        var animEnd = message.Timestamp + animDuration;

        return GetAnimationProgress(time, animStart, animEnd);
    }

    private static double GetFadeOutOpacity(ActiveMessage message, double time)
    {
        var animDuration = Math.Min(0.5, message.Duration * 0.2);
        var animStart = message.Timestamp + message.Duration - animDuration;
        var animEnd = message.Timestamp + message.Duration;
        
        return 1 - GetAnimationProgress(time, animStart, animEnd);
    }

    private static double GetAnimationProgress(double time, double animStart, double animEnd) => 
        Math.Clamp(Util.InverseLerp(time, animStart, animEnd), 0, 1);

    private record ActiveMessage(MessageLevel Level, IComponent Text, double Duration, double Timestamp)
    {
        public static ActiveMessage Create(ConsoleMessage message, double timestamp)
        {
            var text = message.Text.Clone().WithFont(Game.UniformFont);
            return new ActiveMessage(message.Level, text, message.Duration, timestamp);
;        }
    }

    private record ColorPalette(Argb32 Text, Argb32 Background, Argb32 Foreground);
    
    private record MessageRender(int X, int Y, int Width, int Height, MessageLevel Level,
        List<FormattedCharSequence> Lines, double Opacity);
}