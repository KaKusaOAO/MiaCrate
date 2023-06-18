namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP3 {}
    public interface IP3<TLeft, TField1, TField2, TField3> : IP3, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>, IPField3<TLeft, TField3>
        where TLeft : IK1
    {
    }

    internal class P3<TLeft, TField1, TField2, TField3> : IP3<TLeft, TField1, TField2, TField3> where TLeft : IK1
    {
        public P3(IApp<TLeft, TField1> t1, IApp<TLeft, TField2> t2, IApp<TLeft, TField3> t3)
        {
            T1 = t1;
            T2 = t2;
            T3 = t3;
        }

        public IApp<TLeft, TField1> T1 { get; }
        public IApp<TLeft, TField2> T2 { get; }
        public IApp<TLeft, TField3> T3 { get; }
    }
}