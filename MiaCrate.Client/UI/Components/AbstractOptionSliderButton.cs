using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public abstract class AbstractOptionSliderButton : AbstractSliderButton
{
    protected Options Options { get; }
    
    protected AbstractOptionSliderButton(Options options, int x, int y, int width, int height,double value) 
        : base(x, y, width, height, CommonComponents.Empty, value)
    {
        Options = options;
    }
}