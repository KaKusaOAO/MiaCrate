using System.Collections;

namespace MiaCrate.Client.UI;

public class CodepointMap<T> : IEnumerable<CodepointMap<T>.Entry> where T : class
{
    private const int BlockBits = 8;
    private const int BlockSize = 256;
    private const int InBlockMask = 0xff;
    private const int MaxBlock = BlockCount - 1;
    private const int BlockCount = 4352;

    private readonly T?[] _empty;
    private readonly T?[][] _blockMap;
    private readonly Func<int, T[]> _blockConstructor;

    public CodepointMap(Func<int, T[]> blockConstructor, Func<int, T[][]> func2)
    {
        _empty = blockConstructor(BlockSize);
        _blockMap = func2(BlockCount);
        Array.Fill(_blockMap, _empty);
        _blockConstructor = blockConstructor;
    }

    public void Clear() => Array.Fill(_blockMap, _empty);

    public T? Get(int i)
    {
        var j = i >> BlockBits;
        var k = i & InBlockMask;
        return _blockMap[j][k];
    }

    public T? Put(int i, T obj)
    {
        var j = i >> BlockBits;
        var k = i & InBlockMask;
        var arr = _blockMap[j];

        if (arr == _empty)
        {
            _blockMap[j] = arr = _blockConstructor(BlockSize);
            arr[k] = obj;
            return null;
        }

        var o = arr[k];
        arr[k] = obj;
        return o;
    }

    public T ComputeIfAbsent(int i, Func<int, T> func)
    {
        var j = i >> BlockBits;
        var k = i & InBlockMask;
        var arr = _blockMap[j];
        var obj = arr[k];
        if (obj != null) return obj;

        if (arr == _empty)
        {
            _blockMap[j] = arr = _blockConstructor(BlockSize);
        }

        var o = func(i);
        arr[k] = o;
        return o;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns>The removed entry, or <c>null</c> if nothing was removed.</returns>
    public T? Remove(int i)
    {
        var j = i >> BlockBits;
        var k = i & InBlockMask;
        var arr = _blockMap[j];
        if (arr == _empty) return null;

        var obj = arr[k];
        arr[k] = null;
        return obj;
    }

    public IEnumerator<Entry> GetEnumerator()
    {
        for (var i = 0; i < _blockMap.Length; i++)
        {
            var arr = _blockMap[i];
            if (arr == _empty) continue;

            for (var j = 0; j < arr.Length; j++)
            {
                var obj = arr[j];
                if (obj == null) continue;

                var k = i << BlockBits | j;
                yield return new Entry(k, obj);
            }
        }
    }

    public IEnumerable<int> Keys => this.Select(e => e.Index);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public record Entry(int Index, T Object);
}