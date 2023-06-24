namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP1 {}

    public interface IP1<TLeft, TField1> : IP1, IPField1<TLeft, TField1>
        where TLeft : IK1
    {
        public IP2<TLeft, TField1, TField2> And<TField2>(IApp<TLeft, TField2> t2);
        public IP3<TLeft, TField1, TField2, TField3> And<TField2, TField3>(IP2<TLeft, TField2, TField3> p);
        public IP4<TLeft, TField1, TField2, TField3, TField4> And<TField2, TField3, TField4>(
            IP3<TLeft, TField2, TField3, TField4> p);
        public IP5<TLeft, TField1, TField2, TField3, TField4, TField5> And<TField2, TField3, TField4, TField5>(
            IP4<TLeft, TField2, TField3, TField4, TField5> p);
        public IP6<TLeft, TField1, TField2, TField3, TField4, TField5, TField6> 
            And<TField2, TField3, TField4, TField5, TField6>(
                IP5<TLeft, TField2, TField3, TField4, TField5, TField6> p);
        public IP7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7> 
            And<TField2, TField3, TField4, TField5, TField6, TField7>(
                IP6<TLeft, TField2, TField3, TField4, TField5, TField6, TField7> p);
        public IP8<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7, TField8> 
            And<TField2, TField3, TField4, TField5, TField6, TField7, TField8>(
                IP7<TLeft, TField2, TField3, TField4, TField5, TField6, TField7, TField8> p);

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, Func<TField1, T> func);
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, IApp<TLeft, Func<TField1, T>> func);
    }

    internal class P1<TLeft, TField1> : IP1<TLeft, TField1> where TLeft : IK1
    {
        public P1(IApp<TLeft, TField1> t1)
        {
            T1 = t1;
        }

        public IApp<TLeft, TField1> T1 { get; }
        
        public IP2<TLeft, TField1, TField2> And<TField2>(IApp<TLeft, TField2> t2) => 
            new P2<TLeft, TField1, TField2>(T1, t2);

        public IP3<TLeft, TField1, TField2, TField3> And<TField2, TField3>(IP2<TLeft, TField2, TField3> p) => 
            new P3<TLeft, TField1, TField2, TField3>(T1, p.T1, p.T2);

        public IP4<TLeft, TField1, TField2, TField3, TField4> And<TField2, TField3, TField4>(
            IP3<TLeft, TField2, TField3, TField4> p) =>
            new P4<TLeft, TField1, TField2, TField3, TField4>(T1, p.T1, p.T2, p.T3);

        public IP5<TLeft, TField1, TField2, TField3, TField4, TField5> And<TField2, TField3, TField4, TField5>(
            IP4<TLeft, TField2, TField3, TField4, TField5> p) =>
            new P5<TLeft, TField1, TField2, TField3, TField4, TField5>(T1, p.T1, p.T2, p.T3, p.T4);

        public IP6<TLeft, TField1, TField2, TField3, TField4, TField5, TField6> And<TField2, TField3, TField4, TField5,
            TField6>(IP5<TLeft, TField2, TField3, TField4, TField5, TField6> p) =>
            new P6<TLeft, TField1, TField2, TField3, TField4, TField5, TField6>(T1, p.T1, p.T2, p.T3, p.T4, p.T5);

        public IP7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7> And<TField2, TField3, TField4,
            TField5, TField6, TField7>(IP6<TLeft, TField2, TField3, TField4, TField5, TField6, TField7> p) =>
            new P7<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7>(T1, p.T1, p.T2, p.T3, p.T4,
                p.T5, p.T6);

        public IP8<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7, TField8> And<TField2, TField3,
            TField4, TField5, TField6, TField7, TField8>(
            IP7<TLeft, TField2, TField3, TField4, TField5, TField6, TField7, TField8> p) =>
            new P8<TLeft, TField1, TField2, TField3, TField4, TField5, TField6, TField7, TField8>(T1, p.T1, p.T2, p.T3, 
                p.T4, p.T5, p.T6, p.T7);

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, Func<TField1, T> func) =>
            Apply(instance, instance.Point(func));

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, IApp<TLeft, Func<TField1, T>> func) =>
            instance.Ap(func, T1);
    }
}