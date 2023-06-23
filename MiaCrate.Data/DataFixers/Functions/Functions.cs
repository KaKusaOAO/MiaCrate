using System.Runtime.CompilerServices;

namespace MiaCrate.Data;

public static class Functions
{
    public static IPointFree<IFunction<TA, TC>> Comp<TA, TB, TC>(IPointFree<IFunction<TB, TC>> f1,
        IPointFree<IFunction<TA, TB>> f2)
    {
        if (IsId(f1)) return (IPointFree<IFunction<TA, TC>>) f2;
        if (IsId(f2)) return (IPointFree<IFunction<TA, TC>>) f1;

        if (f1 is IComp<TB, TC> comp1 && f2 is IComp<TA, TB> comp2)
        {
            var functions = new IPointFree<IFunction>[comp1.Functions.Length + comp2.Functions.Length];
            Array.Copy(comp1.Functions, 0, functions, 0, comp1.Functions.Length);
            Array.Copy(comp2.Functions, 0, functions, comp1.Functions.Length, comp2.Functions.Length);
            return new Comp<TA, TC>(functions);
        }
        
        if (f1 is IComp<TB, TC> comp1A)
        {
            var functions = new IPointFree<IFunction>[comp1A.Functions.Length + 1];
            Array.Copy(comp1A.Functions, 0, functions, 0, comp1A.Functions.Length);
            functions[^1] = Unsafe.As<IPointFree<IFunction>>(f2);
            return new Comp<TA, TC>(functions);
        }
        
        if (f2 is IComp<TA, TB> comp2A)
        {
            var functions = new IPointFree<IFunction>[comp2A.Functions.Length + 1];
            functions[0] = Unsafe.As<IPointFree<IFunction>>(f1);
            Array.Copy(comp2A.Functions, 0, functions, 1, comp2A.Functions.Length);
            return new Comp<TA, TC>(functions);
        }

        return new Comp<TA, TC>(
            Unsafe.As<IPointFree<IFunction>>(f1), 
            Unsafe.As<IPointFree<IFunction>>(f2));
    }

    public static IBang<T> Bang<T>(IType<T> type) => new Bang<T>(type);
    public static IPointFree<IFunction<T, T>> Id<T>(IType<T> type) => new Id<T>(Dsl.Func(type, type));

    public static bool IsId(IPointFree func) => func is IId;
}