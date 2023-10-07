using MiaCrate.Client.Graphics;
using MiaCrate.Client.UI;
using MiaCrate.Client.Utils;
using Mochi.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Sodium.UI.Options.Controls;

public class SliderControl : IControl<int>
{
    private readonly int _min;
    private readonly int _max;
    private readonly int _interval;
    private readonly ControlValueFormatter _mode;
    
    public int MaxWidth => 130;

    public ISodiumOption<int> Option { get; }

    public SliderControl(ISodiumOption<int> option, int min, int max, int interval, ControlValueFormatter mode)
    {
        if (max <= min)
            throw new ArgumentException("The maximum value must be greater than the minimum value");

        if (interval <= 0)
            throw new ArgumentException("The slider interval must be greater than 0");

        if ((max - min) % interval != 0)
            throw new ArgumentException("The maximum value must be dividable by the interval");

        if (mode == null!)
            throw new ArgumentException("The slider mode must not be null");
        
        Option = option;
        _min = min;
        _max = max;
        _interval = interval;
        _mode = mode;
    }

    public ControlElement<int> CreateElement(Dim2I dim) => new Button(Option, dim, _min, _max, _interval, _mode);

    private class Button : ControlElement<int>
    {
        private const int ThumbWidth = 2;
        private const int TrackHeight = 1;
        
        private readonly Rect2I _sliderBounds;
        private readonly ControlValueFormatter _formatter;
        
        private readonly int _min;
        private readonly int _max;
        private readonly int _range;
        private readonly int _interval;

        private double _thumbPosition;
        private bool _sliderHeld;

        public double SnappedThumbPosition => _thumbPosition / (1.0 / _range);

        public int IntValue => _min + _interval * (int) Math.Round(SnappedThumbPosition / _interval);

        public Button(ISodiumOption<int> option, Dim2I dimension, int min, int max, int interval, ControlValueFormatter formatter) 
            : base(option, dimension)
        {
            _min = min;
            _max = max;
            _range = max - min;
            _interval = interval;
            _formatter = formatter;
            
            _sliderBounds = new Rect2I(dimension.LimitX - 96, dimension.CenterY - 5, 90, 10);
            _sliderHeld = false;
        }

        public override void Render(GuiGraphics graphics, int mouseX, int mouseY, float f)
        {
            base.Render(graphics, mouseX, mouseY, f);

            if (Option.IsAvailable && (_hovered || IsFocused))
            {
                RenderSlider(graphics);
            }
            else
            {
                RenderStandaloneValue(graphics);
            }
        }

        private void RenderStandaloneValue(GuiGraphics graphics)
        {
            var sliderX = _sliderBounds.X;
            var sliderY = _sliderBounds.Y;
            var sliderWidth = _sliderBounds.Width;
            var sliderHeight = _sliderBounds.Height;

            var label = _formatter.Format(Option.Value);
            var labelWidth = Font.Width(label);
            
            DrawString(graphics, label, sliderX + sliderWidth - labelWidth, sliderY + sliderHeight / 2 - 4, Argb32.White);
        }

        private void RenderSlider(GuiGraphics graphics)
        {
            var sliderX = _sliderBounds.X;
            var sliderY = _sliderBounds.Y;
            var sliderWidth = _sliderBounds.Width;
            var sliderHeight = _sliderBounds.Height;

            _thumbPosition = GetThumbPositionForValue(Option.Value);

            var thumbOffset = Math.Clamp((double) (IntValue - _min) / _range * sliderWidth, 0, sliderWidth);

            var thumbX = (int) (sliderX + thumbOffset - ThumbWidth);
            var trackY = (int) (sliderY + sliderHeight / 2f - TrackHeight / 2f);
            
            DrawRect(graphics, thumbX, sliderY, thumbX + ThumbWidth * 2, sliderY + sliderHeight, Argb32.White);
            DrawRect(graphics, sliderX, trackY, sliderX + sliderWidth, trackY + TrackHeight, Argb32.White);

            var label = IntValue.ToString();
            var labelWidth = Font.Width(label);
            
            DrawString(graphics, label, sliderX - labelWidth - 6, sliderY + sliderHeight / 2 - 4, Argb32.White);
        }

        public double GetThumbPositionForValue(int value) => (value - _min) * (1.0 / _range);

        public override bool MouseClicked(double x, double y, MouseButton button)
        {
            _sliderHeld = false;
            if (!Option.IsAvailable || button != MouseButton.Left || !Dimension.ContainsCursor(x, y)) return false;
            
            if (_sliderBounds.Contains((int) x, (int) y))
            {
                SetValueFromMouse(x);
                _sliderHeld = true;
            }

            return true;

        }

        private void SetValueFromMouse(double d) => SetValue((d - _sliderBounds.X) / _sliderBounds.Width);

        private void SetValue(double d)
        {
            _thumbPosition = Math.Clamp(d, 0, 1);

            var value = IntValue;
            if (Option.Value != value) Option.Value = value;
        }

        public override bool MouseDragged(double x, double y, MouseButton button, double dx, double dy)
        {
            if (!Option.IsAvailable || button != MouseButton.Left) return false;
            if (_sliderHeld) SetValueFromMouse(x);
            return true;
        }
    }
}