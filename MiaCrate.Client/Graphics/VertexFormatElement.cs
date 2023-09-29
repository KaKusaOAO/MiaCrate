using Veldrid;

namespace MiaCrate.Client.Graphics;

public class VertexFormatElement
{
    public TypeInfo Type { get; }
    public UsageInfo Usage { get; }
    public int Index { get; }
    public int Count { get; }
    public VertexElementFormat ElementFormat { get; }
    public VertexElementSemantic Semantic { get; }
    public int ByteSize { get; }

    public VertexFormatElement(int index, TypeInfo type, UsageInfo usage, int count, VertexElementFormat elementFormat, VertexElementSemantic semantic)
    {
        if (!SupportsUsage(index, usage)) 
            throw new Exception("Multiple vertex elements of the same type other than UVs are not supported");
        
        Usage = usage;
        Type = type;
        Index = index;
        Count = count;
        ElementFormat = elementFormat;
        Semantic = semantic;
        ByteSize = type.Size * Count;
    }

    public bool IsPosition => Usage == UsageInfo.Position;

    public void SetupBufferState(int attribIndex, IntPtr pointer, int stride) => 
        Usage.SetupBufferState(Count, stride, pointer, Index, attribIndex);

    public void ClearBufferState(int attribIndex) =>
        Usage.ClearBufferState(Index, attribIndex);

    private bool SupportsUsage(int i, UsageInfo usage) => i == 0 || usage == UsageInfo.Uv;

    public class TypeInfo
    {
        public int Size { get; }
        public string Name { get; }

        public static readonly TypeInfo Float = new(4, "Float");
        public static readonly TypeInfo UByte = new(1, "Unsigned Byte");
        public static readonly TypeInfo Byte = new(1, "Byte");
        public static readonly TypeInfo UShort = new(2, "Unsigned Short");
        public static readonly TypeInfo Short = new(2, "Short");
        public static readonly TypeInfo UInt = new(4, "Unsigned Int");
        public static readonly TypeInfo Int = new(4, "Int");
        
        private TypeInfo(int size, string name)
        {
            Size = size;
            Name = name;
        }
    }

    public class UsageInfo
    {
        public string Name { get; }
        private readonly SetupStateDelegate _setupState;
        private readonly ClearStateDelegate _clearState;

        private delegate void SetupStateDelegate(int size, int stride, IntPtr ptr, int elementIndex, int attribIndex);
        private delegate void ClearStateDelegate(int elementIndex, int attribIndex);

        public static readonly UsageInfo Position = new("Position", (size, stride, pointer, _, index) =>
        {
            
        }, (_, index) =>
        {
            
        });
        
        public static readonly UsageInfo Normal = new("Normal", (size, stride, pointer, _, index) =>
        {
            
        }, (_, index) =>
        {
            
        });
        
        public static readonly UsageInfo Color = new("Color", (size, stride, pointer, _, index) =>
        {
            
        }, (_, index) =>
        {
            
        });
        
        public static readonly UsageInfo Uv = new("UV", (size, stride, pointer, _, index) =>
        {
            
        }, (_, index) =>
        {
            
        });

        public static readonly UsageInfo Padding = new("Padding", 
            (_, _, _, _, _) => { },
            (_, _) => { });

        public static readonly UsageInfo Generic = new("Generic",
            (size, stride, ptr, _, attribIndex) =>
            {
                
            },
            (_, attribIndex) =>
            {
                
            });

        private UsageInfo(string name, SetupStateDelegate setupState, ClearStateDelegate clearState)
        {
            Name = name;
            _setupState = setupState;
            _clearState = clearState;
        }
        
        public void SetupBufferState(int size, int stride, IntPtr ptr, int elementIndex, int attribIndex) => 
            _setupState(size, stride, ptr, elementIndex, attribIndex);

        public void ClearBufferState(int elementIndex, int attribIndex) => 
            _clearState(elementIndex, attribIndex);
    }
}