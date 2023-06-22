using MiaCrate.Data.Optic;

namespace MiaCrate.Data;

public interface IOpticParts {}
public interface IOpticPartsLeft<T> : IOpticParts {}
public interface IOpticPartsRight<T> : IOpticParts {}

public interface IOpticParts<TLeft, TRight> : IOpticPartsLeft<TLeft>, IOpticPartsRight<TRight>
{
    IOpticOut<TLeft, TRight> Optic { get; }
}
