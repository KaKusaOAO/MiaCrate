using MiaCrate.Data.Optic;

namespace MiaCrate.Data;

public interface IFamilyOptic { }
public interface IFamilyOpticLeft<T> : IFamilyOptic {}
public interface IFamilyOpticRight<T> : IFamilyOptic {}

public interface IFamilyOptic<TLeft, TRight> : IFamilyOpticLeft<TLeft>, IFamilyOpticRight<TRight>
{
    IOpticParts<TLeft, TRight> Apply(int index);
}