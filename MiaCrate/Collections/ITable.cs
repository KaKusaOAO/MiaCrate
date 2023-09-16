namespace MiaCrate.Collections;

public interface ITable
{
    bool IsEmpty { get; }
}

public interface ITable<TRow, TColumn, TValue> : ITable, IDictionary<(TRow Row, TColumn Column), TValue>
{
    TValue? this[TRow rowKey, TColumn columnKey]
    {
        get => TryGetValue(rowKey, columnKey, out var result) ? result : default;
        set => Put(rowKey, columnKey, value);
    }

    TValue IDictionary<(TRow Row, TColumn Column), TValue>.this[(TRow Row, TColumn Column) key]
    {
        get => this[key];
        set => this[key] = value;
    }
    
    bool Contains(TRow rowKey, TColumn columnKey);
    bool ContainsRow(TRow rowKey) => Rows.Contains(rowKey);
    bool ContainsColumn(TColumn columnKey) => Columns.Contains(columnKey);
    bool ContainsValue(TValue value) => Values.Contains(value);
    bool TryGetValue(TRow row, TColumn column, out TValue value);

    Dictionary<TColumn, TValue> Row(TRow row);
    Dictionary<TRow, TValue> Column(TColumn column);
    ICollection<(TRow Row, TColumn Column, TValue Value)> Cells { get; }
    ICollection<TRow> Rows { get; }
    ICollection<TColumn> Columns { get; }

    ICollection<(TRow Row, TColumn Column)> IDictionary<(TRow Row, TColumn Column), TValue>.Keys => 
        Rows.SelectMany(_ => Columns, (row, column) => (row, column)).ToList();

    bool IDictionary<(TRow Row, TColumn Column), TValue>.ContainsKey((TRow, TColumn) key) => 
        Contains(key.Item1, key.Item2);

    bool ICollection<KeyValuePair<(TRow Row, TColumn Column), TValue>>.Contains(
        KeyValuePair<(TRow Row, TColumn Column), TValue> item) =>
        item.Value != null && Cells.Contains((item.Key.Row, item.Key.Column, item.Value));

    bool IDictionary<(TRow Row, TColumn Column), TValue>.TryGetValue((TRow Row, TColumn Column) key, out TValue value) => 
        TryGetValue(key.Row, key.Column, out value);

    void Put(TRow rowKey, TColumn columnKey, TValue? value);

    void IDictionary<(TRow Row, TColumn Column), TValue>.Add((TRow Row, TColumn Column) key, TValue? value) =>
        Put(key.Row, key.Column, value);

    void ICollection<KeyValuePair<(TRow Row, TColumn Column), TValue>>.Add(
        KeyValuePair<(TRow Row, TColumn Column), TValue> item) =>
        Put(item.Key.Row, item.Key.Column, item.Value);

    bool Remove(TRow rowKey, TColumn columnKey);

    bool IDictionary<(TRow Row, TColumn Column), TValue>.Remove((TRow Row, TColumn Column) key) =>
        Remove(key.Row, key.Column);

    bool ICollection<KeyValuePair<(TRow Row, TColumn Column), TValue>>.Remove(
        KeyValuePair<(TRow Row, TColumn Column), TValue> item) => Remove(item.Key.Row, item.Key.Column);
}

