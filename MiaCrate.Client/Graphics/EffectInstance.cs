using System.Text;
using System.Text.Json.Nodes;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Json;
using MiaCrate.Resources;
using Mochi.Extensions;
using Mochi.Utils;
using Veldrid;
using Veldrid.OpenGLBinding;
using Veldrid.SPIRV;

namespace MiaCrate.Client.Graphics;

public class EffectInstance : IEffect, IDisposable
{
    private const string EffectShaderPath = $"{ShaderInstance.ShaderPath}/program/";
    private const bool AlwaysReapply = true;

    private static readonly AbstractUniform DummyUniform = new();
    private static int _lastProgramId = -1;
    private static EffectInstance? _lastAppliedEffect = null;
    
    private readonly Dictionary<string, TextureInstance?> _samplerMap = new();
    private readonly List<string> _samplerNames = new();
    private readonly List<int> _samplerLocations = new();
    private readonly List<Uniform> _uniforms = new();
    private readonly List<DeviceBuffer> _uniformLocations = new();
    private readonly Dictionary<string, Uniform> _uniformMap = new();
    private readonly List<int>? _attributes;
    private readonly List<string>? _attributeNames;
    private bool _dirty;
    private readonly BlendMode _blend;
    private ShaderSetDescription _shaderSetDesc;
    
    public ResourceLayout[] ResourceLayouts { get; }
    public BindableResource[] TextureResources { get; private set; } = Array.Empty<BindableResource>();
    public BindableResource[] SamplerResources { get; private set; } = Array.Empty<BindableResource>();
    public BindableResource[] VertexUniformResources { get; private set; } = Array.Empty<BindableResource>();
    public BindableResource[] FragmentUniformResources { get; private set; } = Array.Empty<BindableResource>();

    public ResourceLayoutElementDescription[] VertexUniformDescriptions { get; private set; } = 
        Array.Empty<ResourceLayoutElementDescription>();
    
    public ResourceLayoutElementDescription[] FragmentUniformDescriptions { get; private set; } = 
        Array.Empty<ResourceLayoutElementDescription>();

    public int Id { get; }
    public string Name { get; }
    public EffectProgram VertexProgram { get; }
    public EffectProgram FragmentProgram { get; }

    Program IShader.VertexProgram => VertexProgram;
    Program IShader.FragmentProgram => FragmentProgram;

    public EffectInstance(IResourceManager manager, string name)
    {
        Name = name;
        var location = new ResourceLocation($"{EffectShaderPath}{name}.json");
        var resource = manager.GetResourceOrThrow(location);

        try
        {
            using var stream = resource.Open();
            var json = JsonNode.Parse(stream)!;
            var vertex = json["vertex"]!.GetValue<string>();
            var fragment = json["fragment"]!.GetValue<string>();

            var samplers = json["samplers"]?.AsArray();
            if (samplers != null)
            {
                var i = 0;
                foreach (var node in samplers)
                {
                    try
                    {
                        ParseSamplerNode(node!);
                    }
                    catch (Exception ex)
                    {
                        var chained = ChainedJsonException.ForException(ex);
                        chained.PrependJsonKey($"samplers[{i}]");

                        if (chained == ex) throw;
                        throw chained;
                    }
                    
                    i++;
                }
            }

            _blend = ParseBlendNode(json["blend"]?.AsObject());
            
            var factory = GlStateManager.ResourceFactory;
            var vConvertResult = GetOrCreate(manager, ProgramType.Vertex, vertex);
            var fConvertResult = GetOrCreate(manager, ProgramType.Fragment, fragment);

            var elements = new SortedDictionary<int, List<ResourceLayoutElementDescription>>(
                vConvertResult.ResourceLayoutsLayoutElementDescriptions);
            
            foreach (var (key, value) in fConvertResult.ResourceLayoutsLayoutElementDescriptions)
            {
                var list = elements.ComputeIfAbsent(key, _ => new List<ResourceLayoutElementDescription>());
                list.AddRange(value);
            }

            var layoutDescs = new List<ResourceLayoutDescription>();
            for (var i = 0; i < 4; i++)
            {
                // Ensure we have a list instance here
                elements.ComputeIfAbsent(i, _ => new List<ResourceLayoutElementDescription>());

                var list = elements[i];
                layoutDescs.Add(new ResourceLayoutDescription(list.ToArray()));
            }

            ResourceLayouts = layoutDescs
                .Select(d => factory.CreateResourceLayout(d))
                .ToArray();
            
            VertexUniformDescriptions = layoutDescs[ShaderInstance.VertexUniformResourceSetIndex].Elements;
            FragmentUniformDescriptions = layoutDescs[ShaderInstance.FragmentUniformResourceSetIndex].Elements;
            
            TextureResources = new BindableResource[layoutDescs[ShaderInstance.TextureResourceSetIndex].Elements.Length]; 
            SamplerResources = new BindableResource[layoutDescs[ShaderInstance.SamplerResourceSetIndex].Elements.Length];
            VertexUniformResources = new BindableResource[VertexUniformDescriptions.Length];
            FragmentUniformResources = new BindableResource[FragmentUniformDescriptions.Length];
            
            var vShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(vConvertResult.Source),
                "main");
            var fShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(fConvertResult.Source),
                "main");

            var shaders = factory.CreateFromSpirv(vShaderDesc, fShaderDesc);
            // var layout = format.CreateVertexLayoutDescription();

            VertexProgram = new EffectProgram(ProgramType.Vertex, shaders[0], name);
            FragmentProgram = new EffectProgram(ProgramType.Fragment, shaders[1], name);
            _shaderSetDesc = new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), shaders);
            
            var attributes = json["attributes"]?.AsArray();
            if (attributes != null)
            {
                var i = 0;
                _attributes = new List<int>();
                _attributeNames = new List<string>();
                
                foreach (var node in attributes)
                {
                    try
                    {
                        _attributeNames.Add(node!.GetValue<string>());
                    }
                    catch (Exception ex)
                    {
                        var chained = ChainedJsonException.ForException(ex);
                        chained.PrependJsonKey($"attributes[{i}]");

                        if (chained == ex) throw;
                        throw chained;
                    }

                    i++;
                }
            }
            else
            {
                _attributes = null;
                _attributeNames = null;
            }

            var uniforms = json["uniforms"]?.AsArray();
            if (uniforms != null)
            {
                var i = 0;

                foreach (var node in uniforms)
                {
                    try
                    {
                        ParseUniformNode(node!);
                    }
                    catch (Exception ex)
                    {
                        var chained = ChainedJsonException.ForException(ex);
                        chained.PrependJsonKey($"uniforms[{i}]");

                        if (chained == ex) throw;
                        throw chained;
                    }
                    
                    i++;
                }
            }
            
            UpdateLocations();
        }
        catch (Exception ex)
        {
            var chained = ChainedJsonException.ForException(ex);
            chained.SetFileNameAndFlush($"{location.Path} ({resource.Source.PackId})");
            
            if (chained == ex) throw;
            throw chained;
        }

        MarkDirty();
    }
    
    public void AttachToProgram()
    {
        FragmentProgram.AttachToEffect(this);
        VertexProgram.AttachToEffect(this);
    }

    private void UpdateLocations()
    {
        RenderSystem.AssertOnRenderThread();
        
        _uniformLocations.Clear();
        foreach (var uniform in _uniforms)
        {
            var name = uniform.Name;
            var arr = VertexUniformResources;
            var source = VertexUniformDescriptions.Select(r => r.Name).ToList();
            var desc = source
                .Where(r => r == name)
                .Select(r => VertexUniformDescriptions.First(d => d.Name == r))
                .Select(Optional.Of)
                .Append(Optional.Empty<ResourceLayoutElementDescription>())
                .First();
            
            if (desc.IsEmpty)
            {
                arr = FragmentUniformResources;
                source = FragmentUniformDescriptions.Select(r => r.Name).ToList();
                desc = source
                    .Where(r => r == name)
                    .Select(r => FragmentUniformDescriptions.First(d => d.Name == r))
                    .Select(Optional.Of)
                    .Append(Optional.Empty<ResourceLayoutElementDescription>())
                    .First();
            }

            if (desc.IsEmpty)
            {
                Logger.Warn($"Shader {Name} could not find uniform named {name} in the specified shader program.");
                uniform.Location = null;
            }
            else
            {
                var index = source.IndexOf(desc.Value.Name);
                
                if (uniform.Location == null)
                {
                    var bufferDesc = new BufferDescription(uniform.SizeInBytes, BufferUsage.UniformBuffer);
                    var buffer = GlStateManager.ResourceFactory.CreateBuffer(bufferDesc);
                    uniform.Location = buffer;
                    arr[index] = buffer;
                }

                _uniformLocations.Add(uniform.Location);
                _uniformMap[name] = uniform;
            }
        }
    }

    public void MarkDirty()
    {
        _dirty = true;
    }

    public static Program.ConvertResult GetOrCreate(IResourceManager manager, ProgramType type, string str)
    {
        var hasValue = type.Programs.TryGetValue(str, out var program);
        if (hasValue) return program!;

        var location = new ResourceLocation(EffectShaderPath + str + type.Extension);
        var resource = manager.GetResourceOrThrow(location);
        using var stream = resource.Open();
        return EffectProgram.CompileShader(type, str, stream, resource.Source.PackId);
    }

    public static BlendMode ParseBlendNode(JsonObject? obj)
    {
        if (obj == null) return new BlendMode();
        var blendFunc = BlendFunction.Add;
        var srcRgb = BlendFactor.One;
        var dstRgb = BlendFactor.Zero;
        var srcAlpha = BlendFactor.One;
        var dstAlpha = BlendFactor.Zero;
        var isDefault = true;
        var isSeparate = false;

        if (obj.TryGetPropertyValue("func", out var funcNode))
        {
            blendFunc = BlendMode.StringToBlendFunc(funcNode!.GetValue<string>());
            if (blendFunc != BlendFunction.Add) isDefault = false;
        }

        if (obj.TryGetPropertyValue("srcrgb", out var srcRgbNode))
        {
            srcRgb = BlendMode.StringToBlendFactorSrc(srcRgbNode!.GetValue<string>());
            if (srcRgb != BlendFactor.One) isDefault = false;
        }
        
        if (obj.TryGetPropertyValue("dstrgb", out var dstRgbNode))
        {
            dstRgb = BlendMode.StringToBlendFactorDest(dstRgbNode!.GetValue<string>());
            if (dstRgb != BlendFactor.Zero) isDefault = false;
        }
        
        if (obj.TryGetPropertyValue("srcalpha", out var srcAlphaNode))
        {
            srcAlpha = BlendMode.StringToBlendFactorSrc(srcAlphaNode!.GetValue<string>());
            if (srcAlpha != BlendFactor.One) isDefault = false;
            isSeparate = true;
        }
        
        if (obj.TryGetPropertyValue("dstalpha", out var dstAlphaNode))
        {
            dstAlpha = BlendMode.StringToBlendFactorDest(dstAlphaNode!.GetValue<string>());
            if (dstAlpha != BlendFactor.Zero) isDefault = false;
            isSeparate = true;
        }

        if (isDefault) return new BlendMode();
        
        return isSeparate
            ? new BlendMode(srcRgb, dstRgb, srcAlpha, dstAlpha, blendFunc)
            : new BlendMode(srcRgb, dstRgb, blendFunc);
    }
    
    private void ParseSamplerNode(JsonNode node)
    {
        var obj = node.AsObject();
        var name = obj["name"]!.GetValue<string>();
        if (!(obj["file"]?.AsValue().TryGetValue<string>(out _) ?? false))
        {
            _samplerMap[name] = null;
        }
        
        _samplerNames.Add(name);
    }

    private void ParseUniformNode(JsonNode node)
    {
        var obj = node.AsObject();
        var name = obj["name"]!.GetValue<string>();
        var type = Uniform.GetTypeFromString(obj["type"]!.GetValue<string>());
        var count = obj["count"]!.GetValue<int>();
        var fs = new float[Math.Max(16, count)];

        var arr = obj["values"]!.AsArray();
        if (arr.Count != count && arr.Count > 1)
            throw new ChainedJsonException($"Invalid amount of values specified (expected {count}, found {arr.Count})");

        var i = 0;
        foreach (var val in arr)
        {
            try
            {
                fs[i] = val!.GetValue<float>();
            }
            catch (Exception ex)
            {
                var chained = ChainedJsonException.ForException(ex);
                chained.PrependJsonKey($"values[{i}]");
            
                if (chained == ex) throw;
                throw chained;
            }

            i++;
        }

        if (count > 1 && arr.Count == 1)
        {
            while (i < count)
            {
                fs[i++] = fs[0];
            }
        }

        var typeOffset = count is > 1 and <= 4 && !type.IsMatrix() ? count - 1 : 0;
        var uniform = new Uniform(name, type + typeOffset, count, this);
        
        if (type.IsInt())
        {
            uniform.SetSafe((int) fs[0], (int) fs[1], (int) fs[2], (int) fs[3]);
        } else if (type.IsFloat())
        {
            uniform.SetSafe(fs[0], fs[1], fs[2], fs[3]);
        }
        else
        {
            uniform.Set(fs);
        }
        
        _uniforms.Add(uniform);
    }

    public void Clear()
    {
        RenderSystem.AssertOnRenderThread();
        ProgramManager.UseProgram(new ShaderSetDescription());
        _lastProgramId = -1;
        _lastAppliedEffect = null;
        
        // for (var i = 0; i < _samplerNames.Count; i++)
        // {
        //     if (!_samplerMap.ContainsKey(_samplerNames[i]) || _samplerMap[_samplerNames[i]] == null) continue;
        //     GlStateManager.ActiveTexture((int) TextureUnit.Texture0 + i);
        //     GlStateManager.BindTexture(0);
        // }
    }

    public void Apply()
    {
        RenderSystem.AssertOnGameThread();
        _dirty = false;
        _lastAppliedEffect = this;
        _blend.Apply();

        if (Id != _lastProgramId)
        {
            ProgramManager.UseProgram(_shaderSetDesc);
            _lastProgramId = Id;
        }
        
        // for (var i = 0; i < _samplerLocations.Count; i++)
        // {
        //     var name = _samplerNames[i];
        //     if (_samplerMap.TryGetValue(name, out var func) && func != null)
        //     {
        //         RenderSystem.ActiveTexture((int) TextureUnit.Texture0 + i);
        //         var j = func!();
        //         if (j != -1)
        //         {
        //             RenderSystem.BindTexture(j);
        //             Uniform.UploadInteger(_samplerLocations[i], i);
        //         }
        //     }
        // }
        
        foreach (var uniform in _uniforms)
        {
            uniform.Upload();
        }
        
        GlStateManager.SetResourceLayouts(ResourceLayouts);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        
        foreach (var uniform in _uniforms)
        {
            uniform.Dispose();
        }
        
        ProgramManager.ReleaseProgram(this);
    }
}