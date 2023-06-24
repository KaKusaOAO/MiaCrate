namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP7 {}
    public interface IP7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7> : IP7, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>, IPField3<TLeft, TField3>, IPField4<TLeft, TField4>,
        IPField5<TLeft, TField5>, IPField6<TLeft, TField6>, IPField7<TLeft, TField7>
        where TLeft : IK1
    {
    }

    internal class 
        P7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7> : 
        IP7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7> 
        where TLeft : IK1
    {
        public P7(IApp<TLeft, TField1> t1, IApp<TLeft, TField2> t2, IApp<TLeft, TField3> t3, IApp<TLeft, TField4> t4,
            IApp<TLeft, TField5> t5, IApp<TLeft, TField6> t6, IApp<TLeft, TField7> t7)
        {
            T1 = t1;
            T2 = t2;
            T3 = t3;
            T4 = t4;
            T5 = t5;
            T6 = t6;
            T7 = t7;
        }

        public IApp<TLeft, TField1> T1 { get; }
        public IApp<TLeft, TField2> T2 { get; }
        public IApp<TLeft, TField3> T3 { get; }
        public IApp<TLeft, TField4> T4 { get; }
        public IApp<TLeft, TField5> T5 { get; }
        public IApp<TLeft, TField6> T6 { get; }
        public IApp<TLeft, TField7> T7 { get; }
    }
}