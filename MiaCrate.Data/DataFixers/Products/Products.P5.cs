namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP5 {}
    public interface IP5<TLeft, TField1, TField2, TField3, TField4, TField5> : IP5, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>, IPField3<TLeft, TField3>, IPField4<TLeft, TField4>,
        IPField5<TLeft, TField5>
        where TLeft : IK1
    {
    }

    internal class 
        P5<TLeft, TField1, TField2, TField3, TField4, TField5> : 
        IP5<TLeft, TField1, TField2, TField3, TField4, TField5> 
        where TLeft : IK1
    {
        public P5(IApp<TLeft, TField1> t1, IApp<TLeft, TField2> t2, IApp<TLeft, TField3> t3, IApp<TLeft, TField4> t4, IApp<TLeft, TField5> t5)
        {
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            T5 = t5;
        }

        public IApp<TLeft, TField1> T1 { get; }
        public IApp<TLeft, TField2> T2 { get; }
        public IApp<TLeft, TField3> T3 { get; }
        public IApp<TLeft, TField4> T4 { get; }
        public IApp<TLeft, TField5> T5 { get; }
    }
}