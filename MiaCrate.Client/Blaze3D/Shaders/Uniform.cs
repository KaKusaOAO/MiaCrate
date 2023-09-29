using System.Runtime.InteropServices;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using Mochi.Utils;
using OpenTK.Mathematics;
using Veldrid;

namespace MiaCrate.Client.Shaders;

public class Uniform : AbstractUniform, IDisposable
{
    public string Name { get; }
    private readonly IShader _parent;
    public DeviceBuffer? Location { get; set; }
    private readonly int _count;
    private readonly UniformType _type;
    private int[]? _intValues;
    private float[]? _floatValues;
    private bool _dirty;
    
    public uint SizeInBytes { get; }

    public Uniform(string name, UniformType type, int count, IShader shader)
    {
        Name = name;
        _type = type;
        _count = count;
        _parent = shader;
        
        var size = count * 4;
        if (size % 16 != 0)
        {
            // Size must be multiple of 16 bytes
            size += 16 - size % 16;
        }

        SizeInBytes = (uint) size;

        if ((int) type <= 3)
        {
            _intValues = new int[count];
            _floatValues = null;
        }
        else
        {
            _intValues = null;
            _floatValues = new float[count];
        }

        MarkDirty();
    }

    private void MarkDirty()
    {
        _dirty = true;
        _parent.MarkDirty();
    }

    public sealed override void Set(float f1)
    {
        _floatValues![0] = f1;
        MarkDirty();
    }

    public sealed override void Set(float f1, float f2)
    {
        _floatValues![0] = f1;
        _floatValues![1] = f2;
        MarkDirty();
    }

    public void Set(int index, float value)
    {
        _floatValues![index] = value;
        MarkDirty();
    }

    public sealed override void Set(float f1, float f2, float f3)
    {
        _floatValues![0] = f1;
        _floatValues![1] = f2;
        _floatValues![2] = f3;
        MarkDirty();
    }
    
    public sealed override void SetSafe(int f1, int f2, int f3, int f4)
    {
        if (_type >= UniformType.Int)
        {
            _intValues![0] = f1;
        }
        
        if (_type >= UniformType.Int2)
        {
            _intValues![1] = f2;
        }
        
        if (_type >= UniformType.Int3)
        {
            _intValues![2] = f3;
        }
        
        if (_type >= UniformType.Int4)
        {
            _intValues![3] = f4;
        }
        
        MarkDirty();
    }

    public sealed override void SetSafe(float f1, float f2, float f3, float f4)
    {
        if (_type >= UniformType.Float)
        {
            _floatValues![0] = f1;
        }
        
        if (_type >= UniformType.Float2)
        {
            _floatValues![1] = f2;
        }
        
        if (_type >= UniformType.Float3)
        {
            _floatValues![2] = f3;
        }
        
        if (_type >= UniformType.Float4)
        {
            _floatValues![3] = f4;
        }
        
        MarkDirty();
    }

    public sealed override void Set(float[] fs)
    {
        if (fs.Length < _count)
        {
            Logger.Warn($"Uniform.Set called with a too-small value array (expected {_count}, got {fs.Length}). Ignoring.");
            return;
        }
        
        Array.Copy(fs, 0, _floatValues!, 0, _floatValues!.Length);
        MarkDirty();
    }

    public sealed override void Set(Matrix4 matrix)
    {
        unsafe
        {
            var numPtr = &matrix.Row0.X;
            var buffer = new float[16];
            Marshal.Copy((IntPtr) numPtr, buffer, 0, buffer.Length);
            Array.Copy(buffer, 0, _floatValues!, 0, 16);
        }
        MarkDirty();
    }
    
    public sealed override void Set(Matrix3 matrix)
    {
        unsafe
        {
            var numPtr = &matrix.Row0.X;
            var buffer = new float[9];
            Marshal.Copy((IntPtr) numPtr, buffer, 0, buffer.Length);
            Array.Copy(buffer, 0, _floatValues!, 0, 9);
        }
        MarkDirty();
    }

    public void Upload()
    {
        if (!_dirty)
        {
            // ?
        }

        _dirty = false;

        if (_type.IsInt())
        {
            UploadAsInteger();
        } else if (_type.IsFloat())
        {
            UploadAsFloat();
        } else if (_type.IsMatrix())
        {
            UploadAsMatrix();
        }
        else
        {
            Logger.Warn($"Uniform.upload called, but type value ({_type}) is not a valid type. Ignoring.");
        }
    }

    private void UploadAsInteger()
    {
        var cl = GlStateManager.CommandList;
        cl.UpdateBuffer(Location, 0, _intValues);

        // switch (_type)
        // {
        //     case UniformType.Int:
        //         // RenderSystem.Uniform1(Location, _intValues!);
        //         break;
        //     case UniformType.Int2:
        //         // RenderSystem.Uniform2(Location, _intValues!);
        //         break;
        //     case UniformType.Int3:
        //         // RenderSystem.Uniform3(Location, _intValues!);
        //         break;
        //     case UniformType.Int4:
        //         // RenderSystem.Uniform4(Location, _intValues!);
        //         break;
        //     default:
        //         Logger.Warn($"Uniform.upload called, but count value ({_count}) is not in thee range of 1 to 4. Ignoring.");
        //         break;
        // }
    }
    
    private void UploadAsFloat()
    {
        var cl = GlStateManager.CommandList;
        cl.UpdateBuffer(Location, 0, _floatValues);
        
        // switch (_type)
        // {
        //     case UniformType.Float:
        //         // RenderSystem.Uniform1(Location, _floatValues!);
        //         break;
        //     case UniformType.Float2:
        //         // RenderSystem.Uniform2(Location, _floatValues!);
        //         break;
        //     case UniformType.Float3:
        //         // RenderSystem.Uniform3(Location, _floatValues!);
        //         break;
        //     case UniformType.Float4:
        //         // RenderSystem.Uniform4(Location, _floatValues!);
        //         break;
        //     default:
        //         Logger.Warn($"Uniform.upload called, but count value ({_count}) is not in thee range of 1 to 4. Ignoring.");
        //         break;
        // }
    }
    
    private void UploadAsMatrix()
    {
        var cl = GlStateManager.CommandList;
        cl.UpdateBuffer(Location, 0, _floatValues);
        
        // switch (_type)
        // {
        //     case UniformType.Matrix2:
        //         // RenderSystem.UniformMatrix2(Location, false, _floatValues!);
        //         break;
        //     case UniformType.Matrix3:
        //         // RenderSystem.UniformMatrix3(Location, false, _floatValues!);
        //         break;
        //     case UniformType.Matrix4:
        //         // RenderSystem.UniformMatrix4(Location, false, _floatValues!);
        //         break;
        // }
    }

    public static void BindAttribLocation(int program, int index, string name)
    {
        // GlStateManager.BindAttribLocation(program, index, name);
    }

    public static int GetUniformLocation(int program, string name)
    {
        // return GlStateManager.GetUniformLocation(program, name);
        return 0;
    }

    public static UniformType GetTypeFromString(string str)
    {
        var result = UniformType.Unknown;
        switch (str)
        {
            case "int":
                result = UniformType.Int;
                break;
            case "float":
                result = UniformType.Float;
                break;
            default:
            {
                if (str.StartsWith("matrix"))
                {
                    if (str.EndsWith("2x2"))
                    {
                        result = UniformType.Matrix2;
                    } else if (str.EndsWith("3x3"))
                    {
                        result = UniformType.Matrix3;
                    } else if (str.EndsWith("4x4"))
                    {
                        result = UniformType.Matrix4;
                    }
                }

                break;
            }
        }

        return result;
    }

    public void Dispose()
    {
        _intValues = null;
        _floatValues = null;
    }

    public static void UploadInteger(int location, int value)
    {
        // RenderSystem.Uniform1(location, value);
    }
}