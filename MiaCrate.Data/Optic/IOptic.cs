namespace MiaCrate.Data.Optic;

public interface IOptic { }
public interface IOpticProof<T> : IOptic where T : IK1 {}
public interface IOpticFieldS<T> : IOptic {}
public interface IOpticFieldT<T> : IOptic {}
public interface IOpticFieldA<T> : IOptic {}
public interface IOpticFieldB<T> : IOptic {}

public interface IOptic<TProof, TS, TT, TA, TB> : IOpticProof<TProof>, 
    IOpticFieldS<TS>, IOpticFieldT<TT>,
    IOpticFieldA<TA>, IOpticFieldB<TB> where TProof : IK1
{
    Func<IApp2<TP, TA, TB>, IApp2<TP, TS, TT>> Eval<TP>(IApp<TProof, TP> proof) where TP : IK2;
}