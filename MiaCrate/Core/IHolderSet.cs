using System.Collections;
using MiaCrate.Data;
using MiaCrate.Tags;
using Mochi.Utils;

namespace MiaCrate.Core;

public interface IHolderSet : IEnumerable
{
    public int Count { get; }
}

public interface IHolderSet<T> : IHolderSet, IEnumerable<IHolder<T>> where T : class
{
    public IHolder<T> this[int index] { get; }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(IHolder<T> holder);

    public bool CanSerializeIn(IHolderOwner<T> owner);

    public IEither<ITagKey<T>, List<IHolder<T>>> Unwrap();

    public IOptional<ITagKey<T>> UnwrapKey();

    public IOptional<IHolder<T>> GetRandomElement(IRandomSource randomSource);
}

public interface INamedHolderSet<T> : IHolderSet<T> where T : class
{
    
}

public interface IDirectHolderSet<T> : IHolderSet<T> where T : class
{
    
}

public static class HolderSet
{
    public static INamedHolderSet<T> CreateNamed<T>(IHolderOwner<T> owner, ITagKey<T> tagKey) where T : class =>
        new NamedSet<T>(owner, tagKey);

    public static IDirectHolderSet<T> Direct<T>(params IHolder<T>[] holders) where T : class =>
        new DirectSet<T>(holders.ToList());

    public static IDirectHolderSet<T> Direct<T>(List<IHolder<T>> holders) where T : class => 
        new DirectSet<T>(holders);

    private abstract class ListBacked<T> : IHolderSet<T> where T : class
    {
        protected abstract List<IHolder<T>> Contents { get; }

        public int Count => Contents.Count;

        public IHolder<T> this[int index] => Contents[index];

        public IEnumerator<IHolder<T>> GetEnumerator() => Contents.GetEnumerator();

        public abstract bool Contains(IHolder<T> holder);

        public virtual bool CanSerializeIn(IHolderOwner<T> owner) => true;

        public abstract IEither<ITagKey<T>, List<IHolder<T>>> Unwrap();

        public abstract IOptional<ITagKey<T>> UnwrapKey();

        public IOptional<IHolder<T>> GetRandomElement(IRandomSource randomSource) => 
            Util.GetRandomSafe(Contents, randomSource);
    }

    private class DirectSet<T> : ListBacked<T>, IDirectHolderSet<T> where T : class
    {
        protected override List<IHolder<T>> Contents { get; }
        private HashSet<IHolder<T>>? _contentsSet;

        public DirectSet(List<IHolder<T>> list)
        {
            Contents = list;
        }

        public override bool Contains(IHolder<T> holder)
        {
            _contentsSet ??= Contents.ToHashSet();
            return _contentsSet.Contains(holder);
        }

        public override IEither<ITagKey<T>, List<IHolder<T>>> Unwrap() => 
            Either.CreateRight(Contents).Left<ITagKey<T>>();

        public override IOptional<ITagKey<T>> UnwrapKey() => Optional.Empty<ITagKey<T>>();
    }

    private class NamedSet<T> : ListBacked<T>, INamedHolderSet<T> where T : class
    {
        private readonly IHolderOwner<T> _owner;
        private List<IHolder<T>> _contents = new();
            
        public ITagKey<T> Key { get; }

        protected override List<IHolder<T>> Contents => _contents;

        public NamedSet(IHolderOwner<T> owner, ITagKey<T> key)
        {
            _owner = owner;
            Key = key;
        }

        public void Bind(List<IHolder<T>> list)
        {
            _contents = new List<IHolder<T>>(list);
        }

        public override bool Contains(IHolder<T> holder) => holder.Is(Key);

        public override IEither<ITagKey<T>, List<IHolder<T>>> Unwrap()
        {
            throw new NotImplementedException();
        }

        public override IOptional<ITagKey<T>> UnwrapKey()
        {
            throw new NotImplementedException();
        }
    }
}