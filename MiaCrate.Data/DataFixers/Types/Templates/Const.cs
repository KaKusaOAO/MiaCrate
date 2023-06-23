namespace MiaCrate.Data;

public sealed class Const : ITypeTemplate
{
    private readonly IType _type;
    
    public int Size => 0;

    public Const(IType type)
    {
        _type = type;
    }

    public ITypeFamily Apply(ITypeFamily family) => new ConstTypeFamily(_type);

    private class ConstTypeFamily : ITypeFamily
    {
        private readonly IType _type;

        public ConstTypeFamily(IType type)
        {
            _type = type;
        }

        public IType Apply(int index) => _type;
    }
}