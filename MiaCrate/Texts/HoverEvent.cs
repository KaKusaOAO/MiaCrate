using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Core;
using MiaCrate.Nbt;
using MiaCrate.World.Entities;
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

    public static HoverEvent? Deserialize(JsonObject obj)
    {
        if (!obj.TryGetPropertyValue("action", out var actionNode))
            return null;
        
        var actionName = actionNode!.GetValue<string>();
        var action = ActionType.GetByName(actionName);
        if (action == null) return null;

        var node = obj["contents"];
        if (node != null)
            return action.Deserialize(node);

        var component = MiaComponent.FromJson(obj["value"]);
        return component != null ? action.DeserializeFromLegacy(component) : null;
    }

    public JsonObject Serialize()
    {
        return new JsonObject
        {
            ["action"] = Action.Name,
            ["contents"] = Action.Serialize(_value)
        };
    }

    public static class ActionType
    {
        private static readonly Dictionary<string, IActionType> _lookup = new();

        public static ActionType<IComponent> ShowText { get; } = Register(
            new ActionType<IComponent>("show_text", true,
            MiaComponent.FromJson, c => c.ToJson(), c => c));

        public static ActionType<ItemStackInfo> ShowItem { get; } =
            Register(new ActionType<ItemStackInfo>("show_item", true,
            ItemStackInfo.Create, i => i.Serialize(), ItemStackInfo.Create));

        public static ActionType<EntityTooltipInfo> ShowEntity { get; } =
            Register(new ActionType<EntityTooltipInfo>("show_entity", true,
            EntityTooltipInfo.Create, e => e.Serialize(), EntityTooltipInfo.Create));
        
        private static ActionType<T> Register<T>(ActionType<T> type)
        {
            _lookup.Add(type.Name, type);
            return type;
        }

        public static IActionType? GetByName(string name) => _lookup.GetValueOrDefault(name);
    }
    
    public interface IActionType
    {
        public bool IsAllowedFromServer { get; }
        public string Name { get; }

        public HoverEvent? Deserialize(JsonNode node);
        public HoverEvent? DeserializeFromLegacy(IComponent component);
        public JsonNode Serialize(object content);
    }
    
    private interface IActionType<in T> : IActionType
    {
        public JsonNode Serialize(T content);
        JsonNode IActionType.Serialize(object content) => Serialize((T) content);
    }
    
    public sealed class ActionType<T> : IActionType<T>
    {
        public bool IsAllowedFromServer { get; }
        private readonly Func<JsonNode, T?> _deserialize;
        private readonly Func<T, JsonNode> _serialize;
        private readonly Func<IComponent, T?> _legacyArgDeserialize;

        public string Name { get; }
        
        public ActionType(string name, bool allowFromServer, Func<JsonNode, T?> deserialize, Func<T, JsonNode> serialize, Func<IComponent, T?> legacyArgDeserialize)
        {
            Name = name;
            IsAllowedFromServer = allowFromServer;
            _deserialize = deserialize;
            _serialize = serialize;
            _legacyArgDeserialize = legacyArgDeserialize;
        }

        public HoverEvent? Deserialize(JsonNode node)
        {
            var o = _deserialize(node);
            return o == null ? null : Create(this, o);
        }
        
        public HoverEvent? DeserializeFromLegacy(IComponent component)
        {
            var o = _legacyArgDeserialize(component);
            return o == null ? null : Create(this, o);
        }

        public JsonNode Serialize(T content) => _serialize(content);
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

        public JsonNode Serialize()
        {
            var obj = new JsonObject
            {
                ["id"] = BuiltinRegistries.Item.GetKey(_item)!.ToString()
            };

            if (_count != 1) obj["count"] = _count;
            if (_tag != null) obj["tag"] = _tag.ToString();

            return obj;
        }
    }

    public class EntityTooltipInfo
    {
        private readonly IEntityType _type;
        private readonly Uuid _uuid;
        private readonly IComponent? _name;

        public EntityTooltipInfo(IEntityType type, Uuid uuid, IComponent? name)
        {
            _type = type;
            _uuid = uuid;
            _name = name;
        }

        public static EntityTooltipInfo? Create(JsonNode node)
        {
            if (node is not JsonObject obj) return null;

            var type = BuiltinRegistries.EntityType.Get(new ResourceLocation(obj["type"]!.GetValue<string>()))!;
            var uuid = Uuid.Parse(obj["id"]!.GetValue<string>());
            var name = MiaComponent.FromJson(obj["name"]);
            return new EntityTooltipInfo(type, uuid, name);
        }

        public static EntityTooltipInfo? Create(IComponent component)
        {
            try
            {
                var tag = NbtParser.ParseTag(component.ToPlainText());
                var name = MiaComponent.FromJson(tag["name"]!.GetValue<string>());
                var type = BuiltinRegistries.EntityType.Get(new ResourceLocation(tag["type"]!.GetValue<string>()))!;
                var uuid = Uuid.Parse(tag["id"]!.GetValue<string>());
                return new EntityTooltipInfo(type, uuid, name);
            }
            catch
            {
                return null;
            }
        }

        public JsonNode Serialize()
        {
            var obj = new JsonObject
            {
                ["type"] = BuiltinRegistries.EntityType.GetKey(_type)!.ToString(),
                ["id"] = _uuid.ToString()
            };

            if (_name != null)
            {
                obj["name"] = _name.ToJson();
            }

            return obj;
        }
    }
}