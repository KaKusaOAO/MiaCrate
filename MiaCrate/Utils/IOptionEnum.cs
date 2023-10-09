using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate;

public interface IOptionEnum
{
    public int Id { get; }
    public string Key { get; }
    public IComponent Caption => MiaComponent.Translatable(Key);
}