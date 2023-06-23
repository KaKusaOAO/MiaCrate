namespace MiaCrate.Data;

public interface IApp2 {}
public interface IApp2Left<T> : IApp2 where T : IK2 {}
public interface IApp2FieldA<T> : IApp2 {}
public interface IApp2FieldB<T> : IApp2 {}

public interface IApp2<TLeft, TA, TB> : IApp2, IApp2Left<TLeft>, IApp2FieldA<TA>, IApp2FieldB<TB> where TLeft : IK2
{
    
}
