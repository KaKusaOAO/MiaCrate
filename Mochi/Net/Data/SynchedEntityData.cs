using Mochi.Extensions;
using Mochi.World.Entities;

namespace Mochi.Net.Data;

public class SynchedEntityData
{
    private static readonly Dictionary<Type, int> _entityIdMap = new();
    private readonly Dictionary<int, IDataItem> _itemsById = new();
    public bool IsDirty { get; private set; }

    public static IEntityDataAccessor<T> DefineId<T>(Type type, IEntityDataSerializer<T> serializer)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        
        var i = 0;
        if (_entityIdMap.ContainsKey(type))
        {
            i = _entityIdMap[type];
        } else
        {
            var j = 0;
            var t = type;

            while (t != typeof(Entity))
            {
                t = t.BaseType!;
                if (_entityIdMap.ContainsKey(t))
                {
                    j = _entityIdMap[t] + 1;
                    break;
                }
            }

            i = j;
        }

        if (i > 254)
        {
            throw new ArgumentException("Data value ID is too large");
        }

        _entityIdMap[type] = i;
        return serializer.CreateAccessor(i);
    }

    public static IEntityDataAccessor<T> DefineId<TEntity, T>(IEntityDataSerializer<T> serializer)
        where TEntity : Entity => DefineId(typeof(TEntity), serializer);

    public void Define<T>(IEntityDataAccessor<T> accessor, T val)
    {
        var i = accessor.Id;
        if (i > 254)
        {
            throw new ArgumentException("Data value ID is too large");
        }

        if (_itemsById.ContainsKey(i))
        {
            throw new ArgumentException("Data value ID is already defined");
        }

        if (EntityDataSerializers.GetId(accessor.Serializer) < 0)
        {
            throw new ArgumentException("Data value serializer is not registered");
        }
        
        CreateDataItem(accessor, val);
    }

    private void CreateDataItem<T>(IEntityDataAccessor<T> accessor, T val)
    {
        var item = new DataItem<T>(accessor, val);
        _itemsById[accessor.Id] = item;
    }

    private DataItem<T> GetItem<T>(IEntityDataAccessor<T> accessor) => (DataItem<T>)_itemsById[accessor.Id];
    
    public T Get<T>(IEntityDataAccessor<T> accessor) => GetItem(accessor).Value;
    
    public void Set<T>(IEntityDataAccessor<T> accessor, T val)
    {
        var item = GetItem(accessor);
        if (Equals(item.Value, val)) return;
        
        item.Value = val;
        item.IsDirty = true;
        IsDirty = true;
    }

    public static void Pack(List<IDataItem>? list, Stream stream)
    {
        if (list != null)
        {
            foreach (var item in list)
            {
                WriteDataItem(stream, item);
            }
        }
        
        stream.WriteByte(255);
    }

    public List<IDataItem>? PackDirty()
    {
        if (!IsDirty) return null;
        
        var result = new List<IDataItem>();
        
        foreach (var item in _itemsById.Values.Where(item => item.IsDirty))
        {
            result.Add(item.Copy());
            item.IsDirty = false;
        }

        IsDirty = false;
        return result;
    }

    public List<IDataItem>? GetAll() => !_itemsById.Any() 
        ? null 
        : _itemsById.Values.Select(i => i.Copy()).ToList();

    private static void WriteDataItem(Stream stream, IDataItem item)
    {
        var accessor = item.Accessor;
        var i = EntityDataSerializers.GetId(accessor.Serializer);
        if (i < 0)
        {
            throw new Exception("Data value serializer is not registered");
        }
        
        stream.WriteByte((byte) accessor.Id);
        stream.WriteVarInt(i);
        accessor.Serializer.Write(stream, item.Value);
    }

    public interface IDataItem
    {
        public IEntityDataAccessor Accessor { get; }
        public object Value { get; set; }
        public bool IsDirty { get; set; }
        
        public IDataItem Copy();
    }
    
    public interface IDataItem<T> : IDataItem
    {
        public new IEntityDataAccessor<T> Accessor { get; }
        IEntityDataAccessor IDataItem.Accessor => Accessor;
        
        public new T Value { get; set; }
        object IDataItem.Value
        {
            get => Value;
            set => Value = (T) value;
        }

        public new IDataItem<T> Copy();
        IDataItem IDataItem.Copy() => Copy();
    }
    
    public class DataItem<T> : IDataItem<T>
    {
        public IEntityDataAccessor<T> Accessor { get; }
        public T Value { get; set; }
        public bool IsDirty { get; set; } = true;
        
        public DataItem(IEntityDataAccessor<T> accessor, T val)
        {
            Accessor = accessor;
            Value = val;
        }

        public IDataItem<T> Copy() => new DataItem<T>(Accessor, Accessor.Serializer.Copy(Value));
    }
}