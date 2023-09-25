using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Brigadier.Exceptions;
using Mochi.Nbt;
using Mochi.Nbt.Serializations;
using StringReader = Mochi.Brigadier.StringReader;

namespace MiaCrate.Nbt;

public partial class NbtParser
{
    public static SimpleCommandExceptionType ErrorTrailingData { get; } =
        BrigadierUtil.CreateSimpleExceptionType(
            // Unexpected trailing data
            Component.Translatable("argument.nbt.trailing")
        );
    
    public static SimpleCommandExceptionType ErrorExpectedKey { get; } =
        BrigadierUtil.CreateSimpleExceptionType(
            // Expected key
            Component.Translatable("argument.nbt.expected.key")
        );
    
    public static SimpleCommandExceptionType ErrorExpectedValue { get; } =
        BrigadierUtil.CreateSimpleExceptionType(
            // Expected value
            Component.Translatable("argument.nbt.expected.value")
        );
    
    public static Dynamic2CommandExceptionType ErrorInsertMixedList { get; } =
        BrigadierUtil.CreateDynamic2ExceptionType((a, b) =>
            // Can't insert %s into list of %s
            Component.Translatable("argument.nbt.list.mixed", a, b)
        );
    
    public static Dynamic2CommandExceptionType ErrorInsertMixedArray { get; } =
        BrigadierUtil.CreateDynamic2ExceptionType((a, b) =>
            // Can't insert %s into %s
            Component.Translatable("argument.nbt.array.mixed", a, b)
        );
    
    public static DynamicCommandExceptionType ErrorInvalidArray { get; } =
        BrigadierUtil.CreateDynamicExceptionType(a =>
            // Invalid array type '%s'
            Component.Translatable("argument.nbt.array.invalid", a)
        );

    public const char ElementSeparator = ',';
    public const char NameValueSeparator = ':';
    private const char ListOpen = '[';
    private const char ListClose = ']';
    private const char StructOpen = '{';
    private const char StructClose = '}';

    private static Regex DoublePatternNoSuffix { get; } = CreateDoublePatternNoSuffixRegex();
    private static Regex DoublePattern { get; } = CreateDoublePatternRegex();
    private static Regex FloatPattern { get; } = CreateFloatPatternRegex();
    private static Regex BytePattern { get; } = CreateBytePatternRegex();
    private static Regex LongPattern { get; } = CreateLongPatternRegex();
    private static Regex ShortPattern { get; } = CreateShortPatternRegex();
    private static Regex IntPattern { get; } = CreateIntPatternRegex();

    public static ICodec<NbtCompound> Codec { get; } = Data.Codec.String.CoSelectSelectMany(
        s =>
        {
            try
            {
                return DataResult.Success(ParseTag(s), Lifecycle.Stable);
            }
            catch (Exception ex)
            {
                return DataResult.Error<NbtCompound>(() => ex.Message);
            }
        }, c => c.ToString());

    private readonly StringReader _reader;

    public NbtParser(StringReader reader)
    {
        _reader = reader;
    }

    public static NbtCompound ParseTag(string str) => new NbtParser(new StringReader(str)).ReadSingleStruct();

    private NbtCompound ReadSingleStruct()
    {
        var tag = ReadStruct();
        _reader.SkipWhitespace();

        if (_reader.CanRead())
            throw ErrorTrailingData.CreateWithContext(_reader);

        return tag;
    }

    public NbtCompound ReadStruct()
    {
        Expect(StructOpen);
        
        var tag = new NbtCompound();
        _reader.SkipWhitespace();

        while (_reader.CanRead() && _reader.Peek() != StructClose)
        {
            var i = _reader.Cursor;
            var key = ReadKey();

            if (string.IsNullOrEmpty(key))
            {
                _reader.Cursor = i;
                throw ErrorExpectedKey.CreateWithContext(_reader);
            }

            Expect(NameValueSeparator);
            tag[key] = ReadValue();

            if (!HasElementSeparator()) break;

            if (!_reader.CanRead())
                throw ErrorExpectedKey.CreateWithContext(_reader);
        }
        
        Expect(StructClose);
        return tag;
    }

    private string ReadKey()
    {
        _reader.SkipWhitespace();
        if (!_reader.CanRead())
            throw ErrorExpectedKey.CreateWithContext(_reader);

        return _reader.ReadString();
    }

    private NbtTag ReadValue()
    {
        _reader.SkipWhitespace();
        if (!_reader.CanRead())
            throw ErrorExpectedValue.CreateWithContext(_reader);

        var c = _reader.Peek();
        if (c == StructOpen) return ReadStruct();

        return c == ListOpen
            ? ReadList()
            : ReadTypedValue();
    }

    private NbtTag ReadList()
    {
        return _reader.CanRead(3) && !StringReader.IsQuotedStringStart(_reader.Peek(1)) && _reader.Peek(2) == ';'
            ? ReadArrayTag()
            : ReadListTag();
    }

    private NbtTag ReadArrayTag()
    {
        Expect(ListOpen);

        var i = _reader.Cursor;
        var c = _reader.Read();
        _reader.Read();
        _reader.SkipWhitespace();

        if (!_reader.CanRead())
            throw ErrorExpectedValue.CreateWithContext(_reader);

        switch (c)
        {
            case 'B':
                return new NbtByteArray(ReadArray<byte>(ArrayTagType.Byte, TagType.Byte));
            case 'L':
                return new NbtLongArray(ReadArray<long>(ArrayTagType.Long, TagType.Float));
            case 'I':
                return new NbtIntArray(ReadArray<int>(ArrayTagType.Int, TagType.Int));
            default:
                _reader.Cursor = i;
                throw ErrorInvalidArray.CreateWithContext(_reader, $"{c}");
        }
    }

    private NbtList ReadListTag()
    {
        Expect(ListOpen);
        _reader.SkipWhitespace();

        if (!_reader.CanRead())
            throw ErrorExpectedValue.CreateWithContext(_reader);

        var tag = new NbtList();
        TagType? type = null;

        while (_reader.Peek() != ListClose)
        {
            var i = _reader.Cursor;
            var value = ReadValue();
            type ??= value.Type;

            if (type != value.Type)
            {
                _reader.Cursor = i;
                throw ErrorInsertMixedList.CreateWithContext(_reader, value.Type, type);
            }
            
            tag.Add(value);
            if (!HasElementSeparator()) break;

            if (!_reader.CanRead())
                throw ErrorExpectedValue.CreateWithContext(_reader);
        }
        
        Expect(ListClose);
        return tag;
    }

    private T[] ReadArray<T>(ArrayTagType arrType, TagType valueType)
    {
        var list = new List<T>();
        
        while (true)
        {
            if (_reader.Peek() != ListClose)
            {
                var i = _reader.Cursor;
                var tag = ReadValue();

                if (valueType != tag.Type)
                {
                    _reader.Cursor = i;
                    throw ErrorInsertMixedArray.CreateWithContext(_reader, tag.Type, arrType);
                }

                if (tag is NbtByte b)
                {
                    var val = b.Value;
                    list.Add(Unsafe.As<byte, T>(ref val));
                }
                else if (tag is NbtLong l)
                {
                    var val = l.Value;
                    list.Add(Unsafe.As<long, T>(ref val));
                }
                else if (tag is NbtInt it)
                {
                    var val = it.Value;
                    list.Add(Unsafe.As<int, T>(ref val));
                }

                if (HasElementSeparator())
                {
                    if (!_reader.CanRead())
                    {
                        throw ErrorExpectedValue.CreateWithContext(_reader);
                    }
                    
                    continue;
                }
            }
            
            Expect(ListClose);
            return list.ToArray();
        }
    }

    private bool HasElementSeparator()
    {
        _reader.SkipWhitespace();
        if (!_reader.CanRead() || _reader.Peek() != ElementSeparator) 
            return false;
        
        _reader.Skip();
        _reader.SkipWhitespace();
        return true;
    }
    
    private NbtTag ReadTypedValue()
    {
        _reader.SkipWhitespace();

        var i = _reader.Cursor;
        if (StringReader.IsQuotedStringStart(_reader.Peek()))
            return NbtTag.Create(_reader.ReadQuotedString());

        var str = _reader.ReadUnquotedString();
        if (string.IsNullOrEmpty(str))
        {
            _reader.Cursor = i;
            throw ErrorExpectedValue.CreateWithContext(_reader);
        }

        return Type(str);
    }

    private NbtTag Type(string str)
    {
        try
        {
            if (FloatPattern.IsMatch(str))
                return NbtTag.Create(float.Parse(str[..^1]));

            if (BytePattern.IsMatch(str))
                return NbtTag.Create(byte.Parse(str[..^1]));

            if (LongPattern.IsMatch(str))
                return NbtTag.Create(long.Parse(str[..^1]));

            if (ShortPattern.IsMatch(str))
                return NbtTag.Create(short.Parse(str[..^1]));

            if (IntPattern.IsMatch(str))
                return NbtTag.Create(int.Parse(str));

            if (DoublePattern.IsMatch(str))
                return NbtTag.Create(double.Parse(str[..^1]));

            if (DoublePatternNoSuffix.IsMatch(str))
                return NbtTag.Create(double.Parse(str));

            if (str.ToLowerInvariant() == "true")
                return NbtTag.Create(true);

            if (str.ToLowerInvariant() == "false")
                return NbtTag.Create(false);
        }
        catch (FormatException)
        {
            // Return as a string tag later.
        }

        return NbtTag.Create(str);
    }

    private void Expect(char c)
    {
        _reader.SkipWhitespace();
        _reader.Expect(c);
    }

    [GeneratedRegex("[-+]?(?:[0-9]+[.]|[0-9]*[.][0-9]+)(?:e[-+]?[0-9]+)?", RegexOptions.IgnoreCase)]
    private static partial Regex CreateDoublePatternNoSuffixRegex();
    
    [GeneratedRegex("[-+]?(?:[0-9]+[.]?|[0-9]*[.][0-9]+)(?:e[-+]?[0-9]+)?d", RegexOptions.IgnoreCase)]
    private static partial Regex CreateDoublePatternRegex();

    [GeneratedRegex("[-+]?(?:[0-9]+[.]?|[0-9]*[.][0-9]+)(?:e[-+]?[0-9]+)?f", RegexOptions.IgnoreCase)]
    private static partial Regex CreateFloatPatternRegex();
    
    [GeneratedRegex("[-+]?(?:0|[1-9][0-9]*)b", RegexOptions.IgnoreCase)]
    private static partial Regex CreateBytePatternRegex();
    
    [GeneratedRegex("[-+]?(?:0|[1-9][0-9]*)l", RegexOptions.IgnoreCase)]
    private static partial Regex CreateLongPatternRegex();
    
    [GeneratedRegex("[-+]?(?:0|[1-9][0-9]*)s", RegexOptions.IgnoreCase)]
    private static partial Regex CreateShortPatternRegex();
    
    [GeneratedRegex("[-+]?(?:0|[1-9][0-9]*)")]
    private static partial Regex CreateIntPatternRegex();

    private enum ArrayTagType
    {
        Byte = TagType.ByteArray,
        Long = TagType.LongArray,
        Int = TagType.IntArray
    }
}