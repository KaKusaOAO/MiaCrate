using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IListBuilder
{
    public IDynamicOps Ops { get; }
}

public interface IListBuilder<T> : IListBuilder
{
    public new IDynamicOps<T> Ops { get; }
    IDynamicOps IListBuilder.Ops => Ops;

    public IListBuilder<T> Add(T value);
    public IListBuilder<T> Add(IDataResult<T> value);
    public IDataResult<T> Build(T prefix);

    internal class Instance : IListBuilder<T>
    {
        public IDynamicOps<T> Ops { get; }
        private IDataResult<List<T>> _builder = DataResult.Success(new List<T>(), Lifecycle.Stable);
        
        public Instance(IDynamicOps<T> ops)
        {
            Ops = ops;
        }
        
        public IListBuilder<T> Add(T value)
        {
            _builder = _builder.Select(b =>
            {
                b.Add(value);
                return b;
            });
            return this;
        }

        public IListBuilder<T> Add(IDataResult<T> value)
        {
            _builder = _builder.Apply2Stable<T, List<T>>((list, val) =>
            {
                list.Add(val);
                return list;
            }, value);
            return this;
        }

        public IDataResult<T> Build(T prefix)
        {
            var result = _builder.SelectMany(b => Ops.MergeToList(prefix, b));
            _builder = DataResult.Success(new List<T>(), Lifecycle.Stable);
            return result;
        }
    }
}