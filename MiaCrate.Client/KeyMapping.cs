using MiaCrate.Client.Platform;
using MiaCrate.Client.Resources;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client;

public class KeyMapping : IComparable<KeyMapping>
{
    public const string CategoryMovement = "key.categories.movement";
    public const string CategoryMisc = "key.categories.misc";
    public const string CategoryMultiplayer = "key.categories.multiplayer";
    public const string CategoryGameplay = "key.categories.gameplay";
    public const string CategoryInventory = "key.categories.inventory";
    public const string CategoryInterface = "key.categories.ui";
    public const string CategoryCreative = "key.categories.creative";
    
    private static readonly Dictionary<string, KeyMapping> _all = new();
    private static readonly Dictionary<InputConstants.Key, KeyMapping> _map = new();
    private static readonly HashSet<string> _categories = new();
    private static readonly Dictionary<string, int> _categorySortOrder = new()
    {
        [CategoryMovement] = 1,
        [CategoryGameplay] = 2,
        [CategoryInventory] = 3,
        [CategoryCreative] = 4,
        [CategoryMultiplayer] = 5,
        [CategoryInterface] = 6,
        [CategoryMisc] = 7
    };

    private int _clickCount;

    public string Name { get; }
    public InputConstants.Key Key { get; }
    public InputConstants.Key DefaultKey { get; }
    public string Category { get; }
    public bool IsDown { get; set; }
    
    public KeyMapping(string name, Keys key, string category) 
        : this(name, InputConstants.KeyType.KeySym, (int) key, category) {}
    
    public KeyMapping(string name, InputConstants.KeyType type, int key, string category)
    {
        Name = name;
        DefaultKey = Key = type.GetOrCreate(key);
        Category = category;
        
        _all[name] = this;
        _map[Key] = this;
        _categories.Add(category);
    }

    public static void Click(InputConstants.Key key)
    {
        var keyMapping = _map.GetValueOrDefault(key);
        if (keyMapping != null) ++keyMapping._clickCount;
    }

    public static void Set(InputConstants.Key key, bool isDown)
    {
        var keyMapping = _map.GetValueOrDefault(key);
        if (keyMapping != null) keyMapping.IsDown = isDown;
    }
    
    public static void SetAll()
    {
        foreach (var keyMapping in _all.Values)
        {
            if (keyMapping.Key.Type == InputConstants.KeyType.KeySym &&
                keyMapping.Key.Value != InputConstants.Unknown.Value)
            {
                unsafe
                {
                    var key = (Keys) keyMapping.Key.Value;
                    keyMapping.IsDown = InputConstants.IsKeyDown(Game.Instance.Window.Handle, key);
                }
            }
        }
    }

    public static void ReleaseAll()
    {
        foreach (var keyMapping in _all.Values)
        {
            keyMapping.Release();
        }
    }

    private void Release()
    {
        _clickCount = 0;
        IsDown = false;
    }

    public int CompareTo(KeyMapping? other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        return Category == other.Category
            ? string.Compare(I18n.Get(Name), I18n.Get(other.Name), StringComparison.Ordinal)
            : _categorySortOrder[Category].CompareTo(_categorySortOrder[other.Category]);
    }
}