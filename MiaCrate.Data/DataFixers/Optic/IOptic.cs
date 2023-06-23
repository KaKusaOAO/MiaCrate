namespace MiaCrate.Data.Optic;

public interface IOptic { }
public interface IOpticProof<T> : IOptic where T : IK1 {}
public interface IOpticFieldS<T> : IOptic {}
public interface IOpticFieldT<T> : IOptic {}
public interface IOpticFieldA<T> : IOptic {}
public interface IOpticFieldB<T> : IOptic {}

public interface IOpticIn<TLeft, TRight> : IOpticFieldS<TLeft>, IOpticFieldT<TRight> {}
public interface IOpticOut<TLeft, TRight> : IOpticFieldA<TLeft>, IOpticFieldB<TRight> {}

public interface IOptic<TProof, TS, TT, TA, TB> : IOpticProof<TProof>, IOpticIn<TS, TT>, IOpticOut<TA, TB> 
    where TProof : IK1
{
    Func<IApp2<TP, TA, TB>, IApp2<TP, TS, TT>> Eval<TP>(IApp<TProof, TP> proof) where TP : IK2;
}