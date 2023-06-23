namespace MiaCrate.Data.Optic;

public interface ICompositionOptic : IOptic
{
    IOptic Outer { get; }
    IOptic Inner { get; }
}

public interface ICompositionOpticFieldA1<T> : ICompositionOptic {}
public interface ICompositionOpticFieldB1<T> : ICompositionOptic {}

public interface ICompositionOptic<TProof, TS, TT, TA, TB, TA1, TB1> : IOptic<TProof, TS, TT, TA1, TB1>,
    ICompositionOpticFieldA1<TA1>, ICompositionOpticFieldB1<TB1> where TProof : IK1
{
    new IOptic<TProof, TS, TT, TA, TB> Outer { get; }
    IOptic ICompositionOptic.Outer => Outer;
    new IOptic<TProof, TA, TB, TA1, TB1> Inner { get; }
    IOptic ICompositionOptic.Inner => Inner;
}

public class CompositionOptic<TProof, TS, TT, TA, TB, TA1, TB1> : ICompositionOptic<TProof, TS, TT, TA, TB, TA1, TB1> 
    where TProof : IK1
{
    public IOptic<TProof, TS, TT, TA, TB> Outer { get; }
    public IOptic<TProof, TA, TB, TA1, TB1> Inner { get; }

    public CompositionOptic(IOptic<TProof, TS, TT, TA, TB> outer, IOptic<TProof, TA, TB, TA1, TB1> inner)
    {
        Outer = outer;
        Inner = inner;
    }

    public Func<IApp2<TP, TA1, TB1>, IApp2<TP, TS, TT>> Eval<TP>(IApp<TProof, TP> proof) where TP : IK2
    {
        // compose(before):
        // return (V v) -> apply(before.apply(v));
        return v => Outer.Eval(proof)(Inner.Eval(proof)(v));
    }
}
