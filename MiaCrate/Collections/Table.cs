using System.Collections;

namespace MiaCrate.Collections;

public class Table<TRow, TColumn, TValue> : ITable<TRow, TColumn, TValue>
{
    private readonly Dictionary<(TRow Row, TColumn Column), TValue> _table = new();
    public IEnumerator<KeyValuePair<(TRow Row, TColumn Column), TValue>> GetEnumerator() => _table.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _table.Clear();
    public void CopyTo(KeyValuePair<(TRow Row, TColumn Column), TValue>[] array, int arrayIndex) => 
        ((IDictionary<(TRow, TColumn), TValue>) _table).CopyTo(array, arrayIndex);

    public int Count => _table.Count;

    public bool IsReadOnly => false;

    public bool IsEmpty => !_table.Any();
    public ICollection<TRow> Rows => _table.Keys.Select(k => k.Row).ToList();

    public ICollection<TColumn> Columns => _table.Keys.Select(k => k.Column).ToList();

    public ICollection<TValue> Values => _table.Values;

    public bool Contains(TRow rowKey, TColumn columnKey) => _table.ContainsKey((rowKey, columnKey));

    public bool TryGetValue(TRow row, TColumn column, out TValue value) => _table.TryGetValue((row, column), out value);

    public Dictionary<TColumn, TValue> Row(TRow row) => 
        _table.ToDictionary(k => k.Key.Column, v => v.Value);

    public Dictionary<TRow, TValue> Column(TColumn column) =>
        _table.ToDictionary(k => k.Key.Row, v => v.Value);

    public ICollection<(TRow Row, TColumn Column, TValue Value)> Cells => 
        _table.Select(k => (k.Key.Row, k.Key.Column, k.Value)).ToList();

    public void Put(TRow rowKey, TColumn columnKey, TValue? value)
    {
        if (value == null)
        {
            Remove(rowKey, columnKey);
            return;
        }
        
        _table[(rowKey, columnKey)] = value;
    }

    public bool Remove(TRow rowKey, TColumn columnKey) => _table.Remove((rowKey, columnKey));
}