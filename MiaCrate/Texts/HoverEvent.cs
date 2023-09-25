using System.Text.Json.Nodes;
using MiaCrate.Core;
using MiaCrate.Nbt;
using MiaCrate.World.Items;
using Mochi.Nbt;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Texts;

public class HoverEvent
{
    private readonly object _value;
    
    public IActionType Action { get; }

    private HoverEvent(IActionType action, object value)
    {
        Action = action;
        _value = value;
    }

    public static HoverEvent Create<T>(ActionType<T> action, T value) => new(action, value!);
    
    public static HoverEvent CreateUnsafe(IActionType action, object value) => new(action, value);

    public T GetValue<T>() => (T) _value;
    
    public T? GetValue<T>(ActionType<T> type) => Action == type ? (T) _value : default;

    public static class ActionType
    {
        private static readonly Dictionary<string, IActionType> _lookup = new();

        public static ActionType<IComponent> ShowText { get; } = Register(
            new ActionType<IComponent>("show_text", true,
            Component.FromJson, c => c.ToJson(), c => c));

        /*
        public static ActionType<IComponent> ShowItem { get; } =
            Register(new ActionType<IComponent>("show_item", true));

        public static ActionType<IComponent> ShowEntity { get; } =
            Register(new ActionType<IComponent>("show_entity", true));
        */
        
        private static ActionType<T> Register<T>(ActionType<T> type)
        {
            _lookup.Add(type.Name, type);
            return type;
        }
    }
    
    public interface IActionType
    {
        public string Name { get; }
    }
    
    private interface IActionType<T> : IActionType
    {
        
    }
    
    public sealed class ActionType<T> : IActionType<T>
    {
        private readonly bool _allowFromServer;
        private readonly Func<JsonNode, T> _deserialize;
        private readonly Func<T, JsonNode> _serialize;
        private readonly Func<IComponent, T> _legacyArgDeserialize;

        public string Name { get; }
        
        public ActionType(string name, bool allowFromServer, Func<JsonNode, T> deserialize, Func<T, JsonNode> serialize, Func<IComponent, T> legacyArgDeserialize)
        {
            Name = name;
            _allowFromServer = allowFromServer;
            _deserialize = deserialize;
            _serialize = serialize;
            _legacyArgDeserialize = legacyArgDeserialize;
        }
    }

    public class ItemStackInfo
    {
        private readonly Item _item;
        private readonly int _count;
        private readonly NbtCompound? _tag;
        private readonly Lazy<ItemStack> _itemStack;

        public ItemStack ItemStack => _itemStack.Value;

        private ItemStackInfo(Item item, int count, NbtCompound? tag)
        {
            _item = item;
            _count = count;
            _tag = tag;
            _itemStack = new Lazy<ItemStack>(CreateItemStack);
        }

        public ItemStackInfo(ItemStack stack)
            : this(stack.Item, stack.Count, stack.Tag?.Copy()) { }
        
        private ItemStack CreateItemStack()
        {
            var stack = new ItemStack(_item, _count);
            if (_tag != null)
            {
                stack.Tag = _tag;
            }
            
            return stack;
        }

        public static ItemStackInfo Create(JsonNode node)
        {
            if (node is JsonValue val)
            {
                var key = val.GetValue<string>();
                var item = BuiltinRegistries.Item.Get(new ResourceLocation(key));
                if (item == null)
                    throw new Exception($"Unknown item: {key}");
                
                return new ItemStackInfo(item, 1, null);
            }

            {
                var obj = node.AsObject();
                var key = obj["id"]!.GetValue<string>();
                var item = BuiltinRegistries.Item.Get(new ResourceLocation(key));
                if (item == null)
                    throw new Exception($"Unknown item: {key}");

                var count = obj["count"]?.GetValue<int>() ?? 1;

                if (obj.TryGetPropertyValue("tag", out var tagNode))
                {
                    var nbt = tagNode!.GetValue<string>();

                    try
                    {
                        var tag = NbtParser.ParseTag(nbt);
                        return new ItemStackInfo(item, count, tag);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Failed to parse tag: {nbt}");
                        Logger.Warn(ex);
                    }
                }

                return new ItemStackInfo(item, count, null);
            }
        }
        
        public static ItemStackInfo? Create(IComponent component)
        {
            var nbt = component.ToPlainText();

            try
            {
                var tag = NbtParser.ParseTag(nbt);
                return new ItemStackInfo(ItemStack.Of(tag));
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to parse item tag: {nbt}");
                Logger.Warn(ex);
                return null;
            }
        }
    }
}