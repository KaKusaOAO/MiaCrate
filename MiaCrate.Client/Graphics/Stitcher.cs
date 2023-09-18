namespace MiaCrate.Client.Graphics;

public class Stitcher<T> where T : IStitcherEntry
{
    private readonly List<Holder> _texturesToBeStitched = new();
    private readonly List<Region> _storage = new();
    private int _storageX;
    private int _storageY;
    private readonly int _maxWidth;
    private readonly int _maxHeight;
    private readonly int _mipLevel;

    public int Width => _storageX;
    public int Height => _storageY;
    
    public Stitcher(int maxWidth, int maxHeight, int mipLevel)
    {
        _maxWidth = maxWidth;
        _maxHeight = maxHeight;
        _mipLevel = mipLevel;
    }

    private static int SmallestFittingMinTexel(int i, int j) => 
        (i >> j) + ((i & (1 << j) - 1) == 0 ? 0 : 1) << j;

    public void RegisterSprite(T entry)
    {
        var holder = new Holder(entry, _mipLevel);
        _texturesToBeStitched.Add(holder);
    }

    public void Stitch()
    {
        var list = new List<Holder>(_texturesToBeStitched);
        list.Sort((a, b) =>
        {
            var h = a.Height.CompareTo(b.Height);
            if (h != 0) return -h;

            var w = a.Width.CompareTo(b.Width);
            if (w != 0) return -w;

            return -a.Entry.Name.CompareTo(b.Entry.Name);
        });
        
        foreach (var holder in list.Where(holder => !AddToStorage(holder)))
            throw new StitcherException(holder.Entry, list.Select(h => (IStitcherEntry) h.Entry));
    }

    public void GatherSprites(ISpriteLoader loader)
    {
        foreach (var region in _storage)
        {
            region.Walk(loader);
        }
    }

    private bool AddToStorage(Holder holder)
    {
        foreach (var region in _storage)
        {
            if (region.Add(holder)) return true;
        }

        return Expand(holder);
    }

    private bool Expand(Holder holder)
    {
        var i = Util.SmallestEncompassingPowerOfTwo(_storageX);
        var j = Util.SmallestEncompassingPowerOfTwo(_storageY);
        var k = Util.SmallestEncompassingPowerOfTwo(_storageX + holder.Width);
        var l = Util.SmallestEncompassingPowerOfTwo(_storageY + holder.Height);

        var bl = k <= _maxWidth;
        var bl2 = l <= _maxHeight;
        if (!bl && !bl2) return false;

        var bl3 = bl && i != k;
        var bl4 = bl2 && j != l;
        var bl5 = bl3 ^ bl4 ? bl3 : bl && i <= j;

        Region region;
        if (bl5)
        {
            if (_storageY == 0)
            {
                _storageY = l;
            }

            region = new Region(_storageX, 0, k - _storageX, _storageY);
            _storageX = k;
        }
        else
        {
            region = new Region(0, _storageY, _storageX, l - _storageY);
            _storageY = l;
        }

        region.Add(holder);
        _storage.Add(region);
        return true;
    }
    
    public record Holder(T Entry, int Width, int Height)
    {
        public Holder(T entry, int i) 
            : this(entry, 
                SmallestFittingMinTexel(entry.Width, i), 
                SmallestFittingMinTexel(entry.Height, i)) {}
    }

    public class Region
    {
        private readonly int _originX;
        private readonly int _originY;
        private readonly int _width;
        private readonly int _height;
        private List<Region>? _subSlots;
        private Holder? _holder;

        public int X => _originX;
        public int Y => _originY;

        public Region(int originX, int originY, int width, int height)
        {
            _originX = originX;
            _originY = originY;
            _width = width;
            _height = height;
        }

        public bool Add(Holder holder)
        {
            if (_holder != null) return false;

            var i = holder.Width;
            var j = holder.Height;
            
            // Check if the region can contain this holder.
            if (i <= _width && j <= _height)
            {
                if (i == _width && j == _height)
                {
                    // The holder perfectly fits the region. 
                    _holder = holder;
                    return true;
                }

                // Create sub-slots if we didn't 
                if (_subSlots == null)
                {
                    _subSlots = new List<Region>();
                    _subSlots.Add(new Region(_originX, _originY, i, j));

                    var k = _width - i;
                    var l = _height - j;
                    if (l > 0 && k > 0)
                    {
                        var m = Math.Max(_height, k);
                        var n = Math.Max(_width, l);
                        if (m >= n)
                        {
                            _subSlots.Add(new Region(_originX, _originY + j, i, l));
                            _subSlots.Add(new Region(_originX + i, _originY, k, _height));
                        }
                        else
                        {
                            _subSlots.Add(new Region(_originX + i, _originY, k, j));
                            _subSlots.Add(new Region(_originX, _originY + j, _width, l));
                        }
                    } else if (k == 0)
                    {
                        _subSlots.Add(new Region(_originX, _originY + j, i, l));
                    } else if (l == 0)
                    {
                        _subSlots.Add(new Region(_originX + i, _originY, k, j));
                    }
                }

                // Try to add the holder in the first sub-slot available
                return _subSlots.Any(subSlot => subSlot.Add(holder));
            }

            // Holder too large for this region, cannot add.
            return false;
        }

        public void Walk(ISpriteLoader loader)
        {
            if (_holder != null)
            {
                loader.Load(_holder.Entry, X, Y);
            } else if (_subSlots != null)
            {
                foreach (var subSlot in _subSlots)
                {
                    subSlot.Walk(loader);
                }
            }
        }
    }
    
    public interface ISpriteLoader
    {
        void Load(T entry, int i, int j);

        public static ISpriteLoader Create(Action<T, int, int> action) => new Instance(action);

        private class Instance : ISpriteLoader
        {
            private readonly Action<T, int, int> _action;

            public Instance(Action<T, int, int> action)
            {
                _action = action;
            }

            public void Load(T entry, int i, int j) => _action(entry, i, j);
        }
    }
}