using System.Reflection;
using System.Reflection.Emit;

namespace MiaCrate.Data;

public static partial class Products
{
    public interface IP4 {}
    public interface IP4<TLeft, TField1, TField2, TField3, TField4> : IP4, 
        IPField1<TLeft, TField1>, IPField2<TLeft, TField2>, IPField3<TLeft, TField3>, IPField4<TLeft, TField4>
        where TLeft : IK1
    {
#if !NATIVEAOT
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance)
        {
            var method = typeof(T).GetMethod(".ctor", 
                BindingFlags.CreateInstance | BindingFlags.Public | 
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
                {
                    typeof(TField1),
                    typeof(TField2),
                    typeof(TField3),
                    typeof(TField4)
                }, null);

            if (method == null)
                throw new InvalidOperationException("No matching constructor found");

            var dyn = new DynamicMethod("CallCtor", typeof(T),
                new[]
                {
                    typeof(TField1),
                    typeof(TField2),
                    typeof(TField3),
                    typeof(TField4)
                });
            
            var il = dyn.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldarg_S, (short) 4);
            il.EmitCall(OpCodes.Newobj, method, null);
            il.Emit(OpCodes.Ret);

            var d = dyn.CreateDelegate(typeof(Func<TField1, TField2, TField3, TField4, T>));
            return Apply(instance, (Func<TField1, TField2, TField3, TField4, T>) d);
        }
#endif
        
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, Func<TField1, TField2, TField3, TField4, T> func);
        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance, IApp<TLeft, Func<TField1, TField2, TField3, TField4, T>> func);
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

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance,
            Func<TField1, TField2, TField3, TField4, T> func)
        {
            return Apply(instance, instance.Point(func));
        }

        public IApp<TLeft, T> Apply<T>(IApplicativeLeft<TLeft> instance,
            IApp<TLeft, Func<TField1, TField2, TField3, TField4, T>> func)
        {
            return instance.Ap4(func, T1, T2, T3, T4);
        }
    }
}