namespace MiaCrate;

public interface IProfilerFiller
{
    public void StartTick();
    public void EndTick();
    public void Push(string str);
    public void Push(Func<string> func);
    public void Pop();
    public void PopPush(string str);
    public void PopPush(Func<string> func);
    public void IncrementCounter(Func<string> func, int count = 1);
    public void IncrementCounter(string str, int count = 1);

    public static IProfilerFiller Tee(IProfilerFiller a, IProfilerFiller b) => new Composed(a, b);
    
    private class Composed : IProfilerFiller
    {
        private readonly IProfilerFiller _a;
        private readonly IProfilerFiller _b;

        public Composed(IProfilerFiller a, IProfilerFiller b)
        {
            _a = a;
            _b = b;
        }
        
        public void StartTick()
        {
            _a.StartTick();
            _b.StartTick();
        }

        public void EndTick()
        {
            _a.EndTick();
            _b.EndTick();
        }

        public void Push(string str)
        {
            _a.Push(str);
            _b.Push(str);
        }

        public void Push(Func<string> func)
        {
            _a.Push(func);
            _b.Push(func);
        }

        public void Pop()
        {
            _a.Pop();
            _b.Pop();
        }

        public void PopPush(string str)
        {
            _a.PopPush(str);
            _b.PopPush(str);
        }

        public void PopPush(Func<string> func)
        {
            _a.PopPush(func);
            _b.PopPush(func);
        }

        public void IncrementCounter(Func<string> func, int count = 1)
        {
            _a.IncrementCounter(func, count);
            _b.IncrementCounter(func, count);
        }

        public void IncrementCounter(string str, int count = 1)
        {
            _a.IncrementCounter(str, count);
            _b.IncrementCounter(str, count);
        }
    }
}