namespace Mochi.World.Entities.AI;

public class RangedAttribute : Attribute
{
    private readonly double _minValue;
    private readonly double _maxValue;

    public RangedAttribute(string descriptionId, double defaultValue, double minValue, double maxValue) : base(descriptionId, defaultValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
        if (minValue > maxValue)
        {
            throw new ArgumentException("Minimum value cannot be bigger than maximum value!");
        }
        else if (defaultValue < minValue)
        {
            throw new ArgumentException("Default value cannot be lower than minimum value!");
        }
        else if (defaultValue > maxValue)
        {
            throw new ArgumentException("Default value cannot be bigger than maximum value!");
        }
    }

    public double GetMinValue()
    {
        return _minValue;
    }

    public double GetMaxValue()
    {
        return _maxValue;
    }

    public override double SanitizeValue(double value)
    {
        return double.IsNaN(value) ? _minValue : Math.Clamp(value, _minValue, _maxValue);
    }
}