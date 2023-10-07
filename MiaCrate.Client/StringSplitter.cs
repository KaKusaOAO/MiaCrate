using Mochi.Core;
using Mochi.Texts;
using Mochi.Utils;
using Vortice.Direct3D11;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Client;

public class StringSplitter
{
    private readonly WidthProvider _widthProvider;

    public StringSplitter(WidthProvider widthProvider)
    {
        _widthProvider = widthProvider;
    }

    public float StringWidth(string? str)
    {
        if (str == null) return 0;

        var f = 0f;
        StringDecomposer.IterateFormatted(str, Style.Empty, IFormattedCharSink.Create((_, style, j) =>
        {
            f += _widthProvider(j, style);
            return true;
        }));

        return f;
    }
    
    public float StringWidth(IFormattedText text)
    {
        var f = 0f;
        StringDecomposer.IterateFormatted(text, Style.Empty, IFormattedCharSink.Create((_, style, j) =>
        {
            f += _widthProvider(j, style);
            return true;
        }));

        return f;
    }
    
    public float StringWidth(FormattedCharSequence seq)
    {
        var f = 0f;
        seq(IFormattedCharSink.Create((_, style, j) =>
        {
            f += _widthProvider(j, style);
            return true;
        }));

        return f;
    }

    public float StringWidth(IComponent component) => StringWidth(IFormattedText.FromComponent(component));

    public List<IFormattedText> SplitLines(string text, int i, Style style)
    {
        var list = new List<IFormattedText>();
        SplitLines(text, i, style, false, (sx, ix, j) =>
        {
            list.Add(IFormattedText.Of(text[ix..j], sx));
        });

        return list;
    }
    
    public void SplitLines(string text, int i, Style style, bool bl, LinePosConsumer consumer)
    {
        var j = 0;
        var k = text.Length;

        var s2 = style;
        
        while (j < k)
        {
            var lineBreakFinder = new LineBreakFinder(this, i);
            var bl2 = StringDecomposer.IterateFormatted(text, j, s2, style, lineBreakFinder);
            if (bl2)
            {
                consumer(s2, j, k);
                break;
            }

            var l = lineBreakFinder.SplitPosition;
            var c = text[l];
            var m = c != '\n' && c != ' ' ? l : l + 1;

            consumer(s2, j, bl ? m : l);
            j = m;

            s2 = lineBreakFinder.SplitStyle;
        }
    }
    
    public List<IFormattedText> SplitLines(IFormattedText text, int i, Style style)
    {
        var list = new List<IFormattedText>();
        SplitLines(text, i, style, (t, _) =>
        {
            list.Add(t);
        });

        return list;
    }
    
    public List<IFormattedText> SplitLines(IFormattedText text, int i, Style style, IFormattedText text2)
    {
        var list = new List<IFormattedText>();
        SplitLines(text, i, style, (t, bl) =>
        {
            list.Add(bl ? IFormattedText.Composite(text2, t) : t);
        });

        return list;
    }

    public void SplitLines(IFormattedText text, int i, Style style, Action<IFormattedText, bool> consumer)
    {
        var list = new List<LineComponent>();
        text.Visit((sx, str) =>
        {
            if (!string.IsNullOrEmpty(str))
            {
                list.Add(new LineComponent(str, sx));
            }

            return Optional.Empty<Unit>();
        }, style);

        var comps = new FlatComponents(list);
        var bl = true;
        var bl2 = false;
        var bl3 = false;

        while (true)
        {
            while (bl)
            {
                bl = false;

                var lineBreakFinder = new LineBreakFinder(this, i);
                foreach (var comp in comps.Parts)
                {
                    var bl4 = StringDecomposer.IterateFormatted(comp.Contents, 0, comp.Style, style, lineBreakFinder);
                    if (!bl4)
                    {
                        var j = lineBreakFinder.SplitPosition;
                        var style2 = lineBreakFinder.SplitStyle;
                        var c = comps.CharAt(j);

                        var bl5 = c == '\n';
                        var bl6 = bl5 || c == ' ';
                        bl2 = bl5;

                        var ft2 = comps.SplitAt(j, bl6 ? 1 : 0, style2);
                        consumer(ft2, bl3);

                        bl3 = !bl5;
                        bl = true;
                        break;
                    }
                    
                    lineBreakFinder.AddToOffset(comp.Contents.Length);
                }
            }

            var ft3 = comps.Remainder;
            if (ft3 != null)
            {
                consumer(ft3, bl3);
            }
            else if (bl2)
            {
                consumer(IFormattedText.Empty, false);
            }

            return;
        }
    }

    public delegate void LinePosConsumer(Style style, int i, int j);
    
    public delegate float WidthProvider(int i, Style style);

    private class FlatComponents
    {
        public List<LineComponent> Parts { get; }
        private string _flatParts;

        public IFormattedText? Remainder
        {
            get
            {
                var collector = new ComponentCollector();
                
                foreach (var comp in Parts)
                {
                    collector.Append(comp);
                }
                
                Parts.Clear();
                return collector.Result;
            }
        }

        public FlatComponents(List<LineComponent> list)
        {
            Parts = list;
            _flatParts = string.Join("", list.SelectMany(c => c.Contents));
        }

        public char CharAt(int i) => _flatParts[i];

        public IFormattedText SplitAt(int i, int j, Style style)
        {
            var collector = new ComponentCollector();
            var k = i;
            var bl = false;
            
            var index = 0;
            var updateQueue = new Dictionary<int, LineComponent>();
            var removed = new List<LineComponent>();
            
            foreach (var comp in Parts)
            {
                try
                {
                    var str = comp.Contents;
                    var l = str.Length;

                    string str2;
                    if (!bl)
                    {
                        if (k > l)
                        {
                            collector.Append(comp);
                            removed.Add(comp);
                            k -= l;
                        }
                        else
                        {
                            str2 = str[..k];
                            if (!string.IsNullOrEmpty(str2))
                            {
                                collector.Append(IFormattedText.Of(str2, comp.Style));
                            }

                            k += j;
                            bl = true;
                        }
                    }

                    if (bl)
                    {
                        if (k <= l)
                        {
                            str2 = str[k..];
                            if (string.IsNullOrEmpty(str2))
                            {
                                removed.Add(comp);
                            }
                            else
                            {
                                updateQueue[index] = new LineComponent(str2, style);
                            }

                            break;
                        }

                        removed.Add(comp);
                        k -= l;
                    }
                }
                finally
                {
                    index++;
                }
            }
            
            foreach (var (idx, val) in updateQueue)
            {
                Parts[idx] = val;
            }

            Parts.RemoveAll(c => removed.Contains(c));
            _flatParts = _flatParts[(i + j)..];
            return collector.ResultOrEmpty;
        }
    }

    private class LineBreakFinder : IFormattedCharSink
    {
        private readonly StringSplitter _instance;
        private readonly float _maxWidth;
        
        private int _lineBreak = -1;
        private Style _lineBreakStyle = Style.Empty;
        private bool _hadNonZeroWidthChar;
        private float _width;
        private int _lastSpace = -1;
        private Style _lastSpaceStyle = Style.Empty;
        private int _nextChar;
        private int _offset;

        public bool LineBreakFound => _lineBreak != -1;
        public int SplitPosition => LineBreakFound ? _lineBreak : _nextChar;
        public Style SplitStyle => _lineBreakStyle;
        
        public LineBreakFinder(StringSplitter instance, float f)
        {
            _instance = instance;
            _maxWidth = Math.Max(f, 1);
        }

        public bool Accept(int i, Style style, int codepoint)
        {
            var k = i + _offset;
            switch (codepoint)
            {
                case 10:
                    return FinishIteration(k, style);
                
                case 32:
                    _lastSpace = k;
                    _lastSpaceStyle = style;
                    break;
            }

            var f = _instance._widthProvider(codepoint, style);
            _width += f;

            if (_hadNonZeroWidthChar && _width > _maxWidth)
            {
                return _lastSpace != -1
                    ? FinishIteration(_lastSpace, _lastSpaceStyle)
                    : FinishIteration(k, style);
            }

            _hadNonZeroWidthChar |= f != 0;
            _nextChar = k + char.ConvertFromUtf32(codepoint).ToArray().Length;
            return true;
        }

        private bool FinishIteration(int i, Style style)
        {
            _lineBreak = i;
            _lineBreakStyle = style;
            return false;
        }

        public void AddToOffset(int i)
        {
            _offset += i;
        }
    }

    private class LineComponent : IFormattedText
    {
        public string Contents { get; }
        public Style Style { get; }

        public LineComponent(string contents, Style style)
        {
            Contents = contents;
            Style = style;
        }

        public IOptional<T> Visit<T>(IFormattedText.ContentConsumer<T> consumer) => 
            consumer(Contents);

        public IOptional<T> Visit<T>(IFormattedText.StyledContentConsumer<T> consumer, Style style) => 
            consumer(Style.ApplyTo(style), Contents);
    }
}