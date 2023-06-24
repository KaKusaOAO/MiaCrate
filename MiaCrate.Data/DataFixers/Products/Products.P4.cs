namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP4 {}
    public interface IP4<TLeft, TField1, TField2, TField3, TField4> : IP4, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>, IPField3<TLeft, TField3>, IPField4<TLeft, TField4>
        where TLeft : IK1
    {
    }

    internal class P4<TLeft, TField1, TField2, TField3, TField4> : IP4<TLeft, TField1, TField2, TField3, TField4> where TLeft : IK1
    {
        public P4(IApp<TLeft, TField1> t1, IApp<TLeft, TField2> t2, IApp<TLeft, TField3> t3, IApp<TLeft, TField4> t4)
        {
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
        }

        public IApp<TLeft, TField1> T1 { get; }
        public IApp<TLeft, TField2> T2 { get; }
        public IApp<TLeft, TField3> T3 { get; }
        public IApp<TLeft, TField4> T4 { get; }
    }
}