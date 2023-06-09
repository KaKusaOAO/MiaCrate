namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP2 {}
    public interface IP2<TLeft, TField1, TField2> : IP2, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>
        where TLeft : IK1
    {
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, Func<TField1, TField2, T> func);
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, IApp<TLeft, Func<TField1, TField2, T>> func);
    }

    internal class P2<TLeft, TField1, TField2> : IP2<TLeft, TField1, TField2> where TLeft : IK1
    {
        public P2(IApp<TLeft, TField1> t1, IApp<TLeft, TField2> t2)
        {
            T1 = t1;
            T2 = t2;
        }

        public IApp<TLeft, TField1> T1 { get; }

        public IApp<TLeft, TField2> T2 { get; }

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, Func<TField1, TField2, T> func) =>
            Apply(instance, instance.Point(func));

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, IApp<TLeft, Func<TField1, TField2, T>> func) => 
            instance.Ap2(func, T1, T2);
    }
}