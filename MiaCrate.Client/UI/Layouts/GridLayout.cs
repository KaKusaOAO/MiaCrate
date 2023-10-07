namespace MiaCrate.Client.UI;

public class GridLayout : AbstractLayout
{
    private readonly List<ILayoutElement> _children = new();
    private readonly List<CellInhabitant> _cellInhabitants = new();
    private int _rowSpacing;
    private int _columnSpacing;
    
    public ILayoutSettings DefaultCellSetting { get; } = ILayoutSettings.Defaults;
    
    public GridLayout() : this(0, 0) {}

    public GridLayout(int i, int j)
        : base(i, j, 0, 0) { }

    public override void ArrangeElements()
    {
        base.ArrangeElements();

        var i = _cellInhabitants.Select(cellInhabitant => cellInhabitant.LastOccupiedRow).Prepend(0).Max();
        var j = _cellInhabitants.Select(cellInhabitant => cellInhabitant.LastOccupiedColumn).Prepend(0).Max();

        var iArr = new int[j + 1];
        var jArr = new int[i + 1];

        foreach (var cell in _cellInhabitants)
        {
            var k = cell.Height - (cell.OccupiedRows - 1) * _rowSpacing;
            var divisor = new Divisor(k, cell.OccupiedRows);

            for (var l = cell.Row; l <= cell.LastOccupiedRow; l++)
            {
                divisor.MoveNext();
                jArr[l] = Math.Max(jArr[l], divisor.Current);
            }

            var ll = cell.Width - (cell.OccupiedColumns - 1) * _columnSpacing;
            var divisor2 = new Divisor(ll, cell.OccupiedColumns);

            for (var m = cell.Column; m <= cell.LastOccupiedColumn; m++)
            {
                divisor2.MoveNext();
                iArr[m] = Math.Max(iArr[m], divisor2.Current);
            }
        }

        var colXArr = new int[j + 1];
        var rowYArr = new int[i + 1];
        
        colXArr[0] = 0;
        for (var k = 1; k <= j; k++)
        {
            colXArr[k] = colXArr[k - 1] + iArr[k - 1] + _columnSpacing;
        }
        
        rowYArr[0] = 0;
        for (var k = 1; k <= i; k++)
        {
            rowYArr[k] = rowYArr[k - 1] + jArr[k - 1] + _rowSpacing;
        }
        
        foreach (var cell in _cellInhabitants)
        {
            var l = 0;

            for (var ii = cell.Column; ii <= cell.LastOccupiedColumn; ii++)
            {
                l += iArr[ii];
            }

            l += _columnSpacing * (cell.OccupiedColumns - 1);
            cell.SetX(X + colXArr[cell.Column], l);

            var n = 0;
            for (var ii = cell.Row; ii <= cell.LastOccupiedRow; ii++)
            {
                n += jArr[ii];
            }

            n += _rowSpacing * (cell.OccupiedRows - 1);
            cell.SetY(Y + rowYArr[cell.Row], n);
        }

        Width = colXArr[j] + iArr[j];
        Height = rowYArr[i] + jArr[i];
    }

    public ILayoutSettings NewCellSettings() => DefaultCellSetting.Copy();

    public T AddChild<T>(T element, int row, int column) where T : ILayoutElement =>
        AddChild(element, row, column, NewCellSettings());
    
    public T AddChild<T>(T element, int row, int column, ILayoutSettings settings) where T : ILayoutElement => 
        AddChild(element, row, column, 1, 1, settings);

    public T AddChild<T>(T element, int row, int column, Action<ILayoutSettings> setupSettings) where T : ILayoutElement =>
        AddChild(element, row, column, 1, 1, Util.Make(NewCellSettings(), setupSettings));
    
    public T AddChild<T>(T element, int row, int column, int occupiedRows, int occupiedColumns) 
        where T : ILayoutElement =>
        AddChild(element, row, column, occupiedRows, occupiedColumns, NewCellSettings());

    public T AddChild<T>(T element, int row, int column, int occupiedRows, int occupiedColumns, Action<ILayoutSettings> setupSettings) 
        where T : ILayoutElement =>
        AddChild(element, row, column, occupiedRows, occupiedColumns, Util.Make(NewCellSettings(), setupSettings));
    
    public T AddChild<T>(T element, int row, int column, int occupiedRows, int occupiedColumns, ILayoutSettings settings) 
        where T : ILayoutElement
    {
        if (occupiedRows < 1)
            throw new ArgumentException("Occupied rows must be at least 1");

        if (occupiedColumns < 1)
            throw new ArgumentException("Occupied columns must be at least 1");
        
        _cellInhabitants.Add(new CellInhabitant(element, row, column, occupiedRows, occupiedColumns, settings));
        _children.Add(element);
        return element;
    }

    public GridLayout SetColumnSpacing(int i)
    {
        _columnSpacing = i;
        return this;
    }

    public GridLayout SetRowSpacing(int i)
    {
        _rowSpacing = i;
        return this;
    }

    public GridLayout SetSpacing(int i) => SetColumnSpacing(i).SetRowSpacing(i);

    public override void VisitChildren(Action<ILayoutElement> consumer)
    {
        foreach (var child in _children)
        {
            consumer(child);
        }
    }

    public RowHelper CreateRowHelper(int columns) => new(this, columns);
    
    public class CellInhabitant : AbstractChildWrapper
    {
        public int Row { get; }
        public int Column { get; }
        public int OccupiedRows { get; }
        public int OccupiedColumns { get; }

        public int LastOccupiedRow => Row + OccupiedRows - 1;
        public int LastOccupiedColumn => Column + OccupiedColumns - 1;
        
        public CellInhabitant(ILayoutElement child, int row, int column, int occupiedRows, int occupiedColumns, ILayoutSettings settings) 
            : base(child, settings)
        {
            Row = row;
            Column = column;
            OccupiedRows = occupiedRows;
            OccupiedColumns = occupiedColumns;
        }
    }

    public sealed class RowHelper
    {
        private readonly GridLayout _instance;
        private readonly int _columns;
        private int _index;

        public ILayoutSettings DefaultCellSetting => _instance.DefaultCellSetting;

        public RowHelper(GridLayout instance, int columns)
        {
            _instance = instance;
            _columns = columns;
        }

        public ILayoutSettings NewCellSettings() => DefaultCellSetting.Copy();

        public T AddChild<T>(T element, int occupiedColumns = 1) where T : ILayoutElement =>
            AddChild(element, occupiedColumns, DefaultCellSetting);
        
        public T AddChild<T>(T element, ILayoutSettings settings) where T : ILayoutElement => 
            AddChild(element, 1, settings);

        public T AddChild<T>(T element, int occupiedColumns, ILayoutSettings settings) where T : ILayoutElement
        {
            var j = _index / _columns;
            var k = _index % _columns;

            if (k + occupiedColumns > _columns)
            {
                ++j;
                k = 0;
                _index = Util.RoundToward(_index, _columns);
            }

            _index += occupiedColumns;
            return _instance.AddChild(element, j, k, 1, occupiedColumns, settings);
        }
    }
}