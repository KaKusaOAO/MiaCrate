using MiaCrate.Client.Platform;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Graphics;

public class VertexFormatElement
{
    public TypeInfo Type { get; }
    public UsageInfo Usage { get; }
    public int Index { get; }
    public int Count { get; }
    public int ByteSize { get; }

    public VertexFormatElement(int index, TypeInfo type, UsageInfo usage, int count)
    {
        if (!SupportsUsage(index, usage)) 
            throw new Exception("Multiple vertex elements of the same type other than UVs are not supported");
        
        Usage = usage;
        Type = type;
        Index = index;
        Count = count;
        ByteSize = type.Size * Count;
    }

    public bool IsPosition => Usage == UsageInfo.Position;

    public void SetupBufferState(int attribIndex, IntPtr pointer, int stride) => 
        Usage.SetupBufferState(Count, Type.Type, stride, pointer, Index, attribIndex);

    public void ClearBufferState(int attribIndex) =>
        Usage.ClearBufferState(Index, attribIndex);

    private bool SupportsUsage(int i, UsageInfo usage) => i == 0 || usage == UsageInfo.Uv;

    public class TypeInfo
    {
        public int Size { get; }
        public string Name { get; }
        public VertexAttribPointerType Type { get; }

        public static readonly TypeInfo Float = new(4, "Float", VertexAttribPointerType.Float);
        public static readonly TypeInfo UByte = new(1, "Unsigned Byte", VertexAttribPointerType.UnsignedByte);
        public static readonly TypeInfo Byte = new(1, "Byte", VertexAttribPointerType.Byte);
        public static readonly TypeInfo UShort = new(2, "Unsigned Short", VertexAttribPointerType.UnsignedShort);
        public static readonly TypeInfo Short = new(2, "Short", VertexAttribPointerType.Short);
        public static readonly TypeInfo UInt = new(4, "Unsigned Int", VertexAttribPointerType.UnsignedInt);
        public static readonly TypeInfo Int = new(4, "Int", VertexAttribPointerType.Int);
        
        private TypeInfo(int size, string name, VertexAttribPointerType type)
        {
            Size = size;
            Name = name;
            Type = type;
        }
    }

    public class UsageInfo
    {
        public string Name { get; }
        private readonly SetupStateDelegate _setupState;
        private readonly ClearStateDelegate _clearState;

        private delegate void SetupStateDelegate(int size, VertexAttribPointerType type, int stride, IntPtr ptr, int elementIndex, int attribIndex);
        private delegate void ClearStateDelegate(int elementIndex, int attribIndex);

        public static readonly UsageInfo Position = new("Position", (size, type, stride, pointer, _, index) =>
        {
            GlStateManager.EnableVertexAttribArray(index);
            GlStateManager.VertexAttribPointer(index, size, type, false, stride, pointer);
        }, (_, index) =>
        {
            GlStateManager.DisableVertexAttribArray(index);
        });
        
        public static readonly UsageInfo Normal = new("Normal", (size, type, stride, pointer, _, index) =>
        {
            GlStateManager.EnableVertexAttribArray(index);
            GlStateManager.VertexAttribPointer(index, size, type, true, stride, pointer);
        }, (_, index) =>
        {
            GlStateManager.DisableVertexAttribArray(index);
        });
        
        public static readonly UsageInfo Color = new("Color", (size, type, stride, pointer, _, index) =>
        {
            GlStateManager.EnableVertexAttribArray(index);
            GlStateManager.VertexAttribPointer(index, size, type, true, stride, pointer);
        }, (_, index) =>
        {
            GlStateManager.DisableVertexAttribArray(index);
        });
        
        public static readonly UsageInfo Uv = new("UV", (size, type, stride, pointer, _, index) =>
        {
            GlStateManager.EnableVertexAttribArray(index);
            if (type == VertexAttribPointerType.Float)
                GlStateManager.VertexAttribPointer(index, size, type, false, stride, pointer);
            else
                GlStateManager.VertexAttribIPointer(index, size, type, stride, pointer);
        }, (_, index) =>
        {
            GlStateManager.DisableVertexAttribArray(index);
        });

        public static readonly UsageInfo Padding = new("Padding", 
            (_, _, _, _, _, _) => { },
            (_, _) => { });

        public static readonly UsageInfo Generic = new("Generic",
            (size, type, stride, ptr, _, attribIndex) =>
            {
                GlStateManager.EnableVertexAttribArray(attribIndex);
                GlStateManager.VertexAttribPointer(attribIndex, size, type, false, stride, ptr);
            },
            (_, attribIndex) => GlStateManager.DisableVertexAttribArray(attribIndex)
        );

        private UsageInfo(string name, SetupStateDelegate setupState, ClearStateDelegate clearState)
        {
            Name = name;
            _setupState = setupState;
            _clearState = clearState;
        }
        
        public void SetupBufferState(int size, VertexAttribPointerType type, int stride, IntPtr ptr, int elementIndex, int attribIndex) => 
            _setupState(size, type, stride, ptr, elementIndex, attribIndex);

        public void ClearBufferState(int elementIndex, int attribIndex) => 
            _clearState(elementIndex, attribIndex);
    }
}