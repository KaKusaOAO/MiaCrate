namespace MiaCrate.Data;

public static partial class Products
{
    public interface IPLeft<T> where T : IK1 {}

    public interface IPHasField1<T>
    {
        public IAppRight<T> T1 { get; }
    }
    
    public interface IPHasField2<T> 
    {
        public IAppRight<T> T2 { get; }
    }

    public interface IPHasField3<T>
    {
        public IAppRight<T> T3 { get; }
    }

    public interface IPHasField4<T>
    {
        public IAppRight<T> T4 { get; }
    }

    public interface IPHasField5<T>
    {
        public IAppRight<T> T5 { get; }
    }

    public interface IPHasField6<T>
    {
        public IAppRight<T> T6 { get; }
    }

    public interface IPHasField7<T>
    {
        public IAppRight<T> T7 { get; }
    }

    public interface IPHasField8<T>
    {
        public IAppRight<T> T8 { get; }
    }

    public interface IPHasField9<T>
    {
        public IAppRight<T> T9 { get; }
    }

    public interface IPHasField10<T>
    {
        public IAppRight<T> T10 { get; }
    }

    public interface IPHasField11<T>
    {
        public IAppRight<T> T11 { get; }
    }

    public interface IPHasField12<T>
    {
        public IAppRight<T> T12 { get; }
    }

    public interface IPHasField13<T>
    {
        public IAppRight<T> T13 { get; }
    }

    public interface IPHasField14<T>
    {
        public IAppRight<T> T14 { get; }
    }

    public interface IPHasField15<T>
    {
        public IAppRight<T> T15 { get; }
    }

    public interface IPHasField16<T>
    {
        public IAppRight<T> T16 { get; }
    }
    
    public interface IPField1<TLeft, T> : IPLeft<TLeft>, IPHasField1<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T1 { get; }
        IAppRight<T> IPHasField1<T>.T1 => T1;
    }
    
    public interface IPField2<TLeft, T> : IPLeft<TLeft>, IPHasField2<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T2 { get; }
        IAppRight<T> IPHasField2<T>.T2 => T2;
    }
    
    public interface IPField3<TLeft, T> : IPLeft<TLeft>, IPHasField3<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T3 { get; }
        IAppRight<T> IPHasField3<T>.T3 => T3;
    }
    
    public interface IPField4<TLeft, T> : IPLeft<TLeft>, IPHasField4<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T4 { get; }
        IAppRight<T> IPHasField4<T>.T4 => T4;
    }
    
    public interface IPField5<TLeft, T> : IPLeft<TLeft>, IPHasField5<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T5 { get; }
        IAppRight<T> IPHasField5<T>.T5 => T5;
    }
    
    
    public interface IPField6<TLeft, T> : IPLeft<TLeft>, IPHasField6<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T6 { get; }
        IAppRight<T> IPHasField6<T>.T6 => T6;
    }
    
    
    public interface IPField7<TLeft, T> : IPLeft<TLeft>, IPHasField7<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T7 { get; }
        IAppRight<T> IPHasField7<T>.T7 => T7;
    }
    
    
    public interface IPField8<TLeft, T> : IPLeft<TLeft>, IPHasField8<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T8 { get; }
        IAppRight<T> IPHasField8<T>.T8 => T8;
    }
    
    
    public interface IPField9<TLeft, T> : IPLeft<TLeft>, IPHasField9<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T9 { get; }
        IAppRight<T> IPHasField9<T>.T9 => T9;
    }
    
    
    public interface IPField10<TLeft, T> : IPLeft<TLeft>, IPHasField10<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T10 { get; }
        IAppRight<T> IPHasField10<T>.T10 => T10;
    }
    
    
    public interface IPField11<TLeft, T> : IPLeft<TLeft>, IPHasField11<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T11 { get; }
        IAppRight<T> IPHasField11<T>.T11 => T11;
    }
    
    
    public interface IPField12<TLeft, T> : IPLeft<TLeft>, IPHasField12<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T12 { get; }
        IAppRight<T> IPHasField12<T>.T12 => T12;
    }
    
    
    public interface IPField13<TLeft, T> : IPLeft<TLeft>, IPHasField13<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T13 { get; }
        IAppRight<T> IPHasField13<T>.T13 => T13;
    }
    
    
    public interface IPField14<TLeft, T> : IPLeft<TLeft>, IPHasField14<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T14 { get; }
        IAppRight<T> IPHasField14<T>.T14 => T14;
    }
    
    
    public interface IPField15<TLeft, T> : IPLeft<TLeft>, IPHasField15<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T15 { get; }
        IAppRight<T> IPHasField15<T>.T15 => T15;
    }
    
    
    public interface IPField16<TLeft, T> : IPLeft<TLeft>, IPHasField16<T> where TLeft : IK1
    {
        public new IApp<TLeft, T> T16 { get; }
        IAppRight<T> IPHasField16<T>.T16 => T16;
    }
}