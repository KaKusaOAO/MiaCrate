using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Json;
using MiaCrate.Resources;
using Mochi.Extensions;
using Mochi.Utils;
using Veldrid;
using Veldrid.SPIRV;

namespace MiaCrate.Client.Graphics;

public class ShaderInstance : IShader, IDisposable
{
    public const int VTextureSamplerResourceSetIndex = 0;
    public const int FTextureSamplerResourceSetIndex = 1;
    public const int VertexUniformResourceSetIndex = 2;
    public const int FragmentUniformResourceSetIndex = 3;
    
    public const string ShaderPath = "shaders";
    private const string ShaderCorePath = $"{ShaderPath}/core/";
    private const string ShaderIncludePath = $"{ShaderPath}/include/";
    private const bool AlwaysReapply = true;

    private static readonly AbstractUniform DummyUniform = new();
    private static ShaderInstance? _lastAppliedShader = null;
    private static int _lastProgramId = -1;
    
    private readonly Dictionary<string, TextureInstance> _samplerMap = new();
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
    private readonly Dictionary<string, Dictionary<string, int>> _samplerLookup;
    private readonly Dictionary<string, ShaderStages> _samplerStageLookup;

    public ResourceLayout[] ResourceLayouts { get; }
    public BindableResource[] VTextureResources { get; }
    public BindableResource[] FTextureResources { get; }
    public BindableResource[] VertexUniformResources { get; }
    public BindableResource[] FragmentUniformResources { get; }

    public ResourceLayoutElementDescription[] VertexUniformDescriptions { get; }
    
    public ResourceLayoutElementDescription[] FragmentUniformDescriptions { get; }

    public int Id { get; }
    public string Name { get; }
    public Program VertexProgram { get; }

    public Program FragmentProgram { get; }
    public VertexFormat VertexFormat { get; }
    
    public Uniform? ModelViewMatrix { get; }
    public Uniform? ProjectionMatrix { get; }
    public Uniform? InverseViewRotationMatrix { get; }
    public Uniform? TextureMatrix { get; }
    public Uniform? ScreenSize { get; }
    public Uniform? ColorModulator { get; }
    public Uniform? Light0Direction { get; }
    public Uniform? Light1Direction { get; }

    public ShaderInstance(IResourceProvider provider, string name, VertexFormat format)
    {
        Name = name;
        VertexFormat = format;
        var location = new ResourceLocation(ShaderCorePath + name + ".json");
        
        // The arrangement is:
        // Set 0 -> Textures
        // Set 1 -> Samplers
        // Set 2 -> Uniforms for Vertex Shader
        // Set 3 -> Uniforms for Fragment Shader

        try
        {
            using var stream = provider.Open(location);
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
            var vConvertResult = GetOrCreate(provider, ProgramType.Vertex, vertex);
            var fConvertResult = GetOrCreate(provider, ProgramType.Fragment, fragment);
            
            var elements = new SortedDictionary<int, List<ResourceLayoutElementDescription>>(
                vConvertResult.ResourceLayoutsLayoutElementDescriptions
                    .ToDictionary(e => e.Key, 
                        e => e.Value.DistinctBy(b => b.Name).ToList()));
            
            foreach (var (key, value) in fConvertResult.ResourceLayoutsLayoutElementDescriptions)
            {
                var list = elements.ComputeIfAbsent(key, _ => new List<ResourceLayoutElementDescription>());
                list.AddRange(value.DistinctBy(b => b.Name));
            }

            var layoutDescs = new List<ResourceLayoutDescription>();
            for (var i = 0; i <= FragmentUniformResourceSetIndex; i++)
            {
                // Ensure we have a list instance here
                elements.ComputeIfAbsent(i, _ => new List<ResourceLayoutElementDescription>());

                var list = elements[i];
                layoutDescs.Add(new ResourceLayoutDescription(list.ToArray()));
            }

            ResourceLayouts = layoutDescs
                .Select(d => factory.CreateResourceLayout(d))
                .ToArray();
            
            VertexUniformDescriptions = layoutDescs[VertexUniformResourceSetIndex].Elements;
            FragmentUniformDescriptions = layoutDescs[FragmentUniformResourceSetIndex].Elements;
            
            VTextureResources = new BindableResource[layoutDescs[VTextureSamplerResourceSetIndex].Elements.Length]; 
            FTextureResources = new BindableResource[layoutDescs[FTextureSamplerResourceSetIndex].Elements.Length]; 
            VertexUniformResources = new BindableResource[VertexUniformDescriptions.Length];
            FragmentUniformResources = new BindableResource[FragmentUniformDescriptions.Length];

            // Sampler to location lookup
            var vDict = new Dictionary<string, int>();
            var fDict = new Dictionary<string, int>();
            var dict = new Dictionary<string, Dictionary<string, int>>();
            var sDict = new Dictionary<string, ShaderStages>();
            
            for (var i = 0; i < layoutDescs[VTextureSamplerResourceSetIndex].Elements.Length; i++)
            {
                var elem = layoutDescs[VTextureSamplerResourceSetIndex].Elements[i];
                vDict[elem.Name] = i;
                dict[elem.Name] = vDict;
                sDict[elem.Name] = ShaderStages.Vertex;
            }
            
            for (var i = 0; i < layoutDescs[FTextureSamplerResourceSetIndex].Elements.Length; i++)
            {
                var elem = layoutDescs[FTextureSamplerResourceSetIndex].Elements[i];
                fDict[elem.Name] = i;
                dict[elem.Name] = fDict;
                sDict[elem.Name] = ShaderStages.Fragment;
            }

            _samplerLookup = dict;
            _samplerStageLookup = sDict;

            var vShaderDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.ASCII.GetBytes(vConvertResult.Source),
                "main");
            var fShaderDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.ASCII.GetBytes(fConvertResult.Source),
                "main");

            var shaders = factory.CreateFromSpirv(vShaderDesc, fShaderDesc);
            var layout = format.CreateVertexLayoutDescription();

            shaders[0].Name = $"Vertex Shader - {Name}";
            shaders[1].Name = $"Fragment Shader - {Name}";

            VertexProgram = new Program(ProgramType.Vertex, shaders[0], name);
            FragmentProgram = new Program(ProgramType.Fragment, shaders[1], name);
            _shaderSetDesc = new ShaderSetDescription(new []{layout}, shaders);
            
            // Then we read attributes
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
            
            // Then we read uniforms
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

            // GlStateManager.ResourceFactory.CreateShader()
            // Id = ProgramManager.CreateProgram();
            // GlStateManager.ObjectLabel(ObjectLabelIdentifier.Program, Id, Name);
            //
            // if (_attributeNames != null)
            // {
            //     var i = 0;
            //     foreach (var attribName in format.ElementAttributeNames)
            //     {
            //         Uniform.BindAttribLocation(Id, i, attribName);
            //         _attributes!.Add(i++);
            //     }
            // }
            //
            // ProgramManager.LinkShader(this);
            UpdateLocations();
        }
        catch (Exception ex)
        {
            var chained = ChainedJsonException.ForException(ex);
            chained.SetFileNameAndFlush(location.Path);
            
            if (chained == ex) throw;
            throw chained;
        }

        MarkDirty();
        
        // Resolve all possible uniforms for once
        ModelViewMatrix = GetUniform("ModelViewMat");
        ProjectionMatrix = GetUniform("ProjMat");
        InverseViewRotationMatrix = GetUniform("IViewRotMat");
        TextureMatrix = GetUniform("TextureMat");
        ScreenSize = GetUniform("ScreenSize");
        ColorModulator = GetUniform("ColorModulator");
        Light0Direction = GetUniform("Light0_Direction");
        Light1Direction = GetUniform("Light1_Direction");
    }

    public static async Task<ShaderInstance> CreateAsync(IResourceProvider provider, string name, VertexFormat format)
    {
        await Task.Yield();
        return new ShaderInstance(provider, name, format);
    }
    
    private void UpdateLocations()
    {
        // RenderSystem.AssertOnRenderThread();
        
        _uniformLocations.Clear();
        foreach (var uniform in _uniforms)
        {
            var name = uniform.Name;
            var arr = VertexUniformResources;
            var source = VertexUniformDescriptions.Select(r => r.Name).ToList();
            var desc = source
                .Where(r => r == $"_{name}")
                .Select(r => VertexUniformDescriptions.First(d => d.Name == r))
                .Select(Optional.Of)
                .Append(Optional.Empty<ResourceLayoutElementDescription>())
                .First();
            
            if (desc.IsEmpty)
            {
                arr = FragmentUniformResources;
                source = FragmentUniformDescriptions.Select(r => r.Name).ToList();
                desc = source
                    .Where(r => r == $"_{name}")
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
                    buffer.Name = $"UniformBuffer - {name} in {Name}";
                    uniform.Location = buffer;
                    arr[index] = buffer;
                }

                _uniformLocations.Add(uniform.Location);
                _uniformMap[name] = uniform;
            }
        }
    }

    public Uniform? GetUniform(string name)
    {
        // RenderSystem.AssertOnRenderThread();
        return _uniformMap.TryGetValue(name, out var result) ? result : null;
    }

    public AbstractUniform SafeGetUniform(string name)
    {
        RenderSystem.AssertOnGameThread(); // ??
        return GetUniform(name) ?? DummyUniform;
    }

    public static Program.ConvertResult GetOrCreate(IResourceProvider provider, ProgramType type, string name)
    {
        if (type.Programs.TryGetValue(name, out var program)) return program;
        
        var path = ShaderCorePath + name + type.Extension;
        var location = new ResourceLocation(path);
        var resource = provider.GetResourceOrThrow(location);

        var dir = Path.GetDirectoryName(path)!;
        using var stream = resource.Open();
        return Program.CompileShader(type, name, stream, resource.Source.PackId, 
            new PreprocessorInstance(provider, dir)
        );
    }

    private class PreprocessorInstance : GlslPreprocessor
    {
        private readonly HashSet<string> _importedPaths = new();
        private readonly string _path;
        private readonly IResourceProvider _provider;

        public PreprocessorInstance(IResourceProvider provider, string path)
        {
            _path = path;
            _provider = provider;
        }
        
        public override string? ApplyImport(bool flag, string path)
        {
            var str = flag ? path : ShaderIncludePath;
            path = PathHelper.Normalize(str + path);
            if (!_importedPaths.Add(path)) return null;

            var location = new ResourceLocation(path);

            try
            {
                using var stream = _provider.Open(location);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Logger.Error($"Could not open GLSL import {path}: {ex.Message}");
                return $"#error {ex.Message}";
            }
        }
    }

    public void MarkDirty()
    {
        _dirty = true;
    }
    
    public void Clear()
    {
        RenderSystem.AssertOnRenderThread();
        ProgramManager.UseProgram(new ShaderSetDescription());
        _lastProgramId = -1;
        _lastAppliedShader = null;
        
        // var i = GlStateManager.GetActiveTexture();
        // for (var j = 0; j < _samplerNames.Count; j++)
        // {
        //     if (!_samplerMap.ContainsKey(_samplerNames[j]) || _samplerMap[_samplerNames[j]] == null) continue;
        //     GlStateManager.ActiveTexture((int) TextureUnit.Texture0 + j);
        //     GlStateManager.BindTexture(0);
        // }
        //
        // GlStateManager.ActiveTexture(i);
    }
    
    public void Apply()
    {
        RenderSystem.AssertOnGameThread();
        _dirty = false;
        _lastAppliedShader = this;
        _blend.Apply();

        if (Id != _lastProgramId)
        {
            ProgramManager.UseProgram(_shaderSetDesc);
            _lastProgramId = Id;
        }

        for (var j = 0; j < _samplerNames.Count; j++)
        {
            var name = _samplerNames[j];
            if (_samplerMap.TryGetValue(name, out var sampler))
            {
                sampler.EnsureUpToDate();
                var index = _samplerLookup[name][name];

                var targetStage = _samplerStageLookup[name];
                switch (targetStage)
                {
                    case ShaderStages.Vertex:
                        VTextureResources[index - 1] = sampler.TextureView;
                        VTextureResources[index] = sampler.Sampler;
                        break;
                    case ShaderStages.Fragment:
                        FTextureResources[index - 1] = sampler.TextureView;
                        FTextureResources[index] = sampler.Sampler;
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid stage {targetStage}");
                }
            }
        }

        foreach (var uniform in _uniforms)
        {
            uniform.Upload();
        }
        
        GlStateManager.SetResourceLayouts(ResourceLayouts);
    }

    public void SetSampler(string str, TextureInstance? obj)
    {
        if (obj == null)
        {
            if (_samplerMap.Remove(str))
            {
                MarkDirty();
            }
        }
        else
        {
            _samplerMap[str] = obj;
            MarkDirty();
        }
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

    public void AttachToProgram()
    {
        
    }

    public void Dispose()
    {
        foreach (var uniform in _uniforms)
        {
            uniform.Dispose();
        }

        _uniforms.Clear();
        ProgramManager.ReleaseProgram(this);
    }

    private static readonly Dictionary<ResourceSetDescription, ResourceSet> _resourceSetCache = new();

    public void SetupPipeline(PrimitiveTopology mode)
    {
        GlStateManager.SetPrimitiveTopology(mode);
        GlStateManager.SetPipeline();

        var cl = GlStateManager.CommandList;
        var factory = GlStateManager.ResourceFactory;
        var shader = this;
        
        foreach (var resource in VTextureResources)
        {
            if (resource is Texture {IsDisposed: true})
                throw new InvalidOperationException("Texture is disposed!");

            if (resource is Sampler {IsDisposed: true})
                throw new InvalidOperationException("Sampler is disposed!");
        }
        
        foreach (var resource in FTextureResources)
        {
            if (resource is Texture {IsDisposed: true})
                throw new InvalidOperationException("Texture is disposed!");

            if (resource is Sampler {IsDisposed: true})
                throw new InvalidOperationException("Sampler is disposed!");
        }
        
        var vTexResSetDesc = 
            new ResourceSetDescription(ResourceLayouts[VTextureSamplerResourceSetIndex], VTextureResources);
        var fTexResSetDesc = 
            new ResourceSetDescription(ResourceLayouts[FTextureSamplerResourceSetIndex], FTextureResources);
        var vUniformResSetDesc = 
            new ResourceSetDescription(ResourceLayouts[VertexUniformResourceSetIndex], VertexUniformResources);
        var fUniformResSetDesc = 
            new ResourceSetDescription(ResourceLayouts[FragmentUniformResourceSetIndex], FragmentUniformResources);

        var vTexResSet = _resourceSetCache.ComputeIfAbsent(vTexResSetDesc, d => factory.CreateResourceSet(d));
        var fTexResSet = _resourceSetCache.ComputeIfAbsent(fTexResSetDesc, d => factory.CreateResourceSet(d));
        var vUniformResSet = _resourceSetCache.ComputeIfAbsent(vUniformResSetDesc, d => factory.CreateResourceSet(d));
        var fUniformResSet = _resourceSetCache.ComputeIfAbsent(fUniformResSetDesc, d => factory.CreateResourceSet(d));

        cl.SetGraphicsResourceSet(VTextureSamplerResourceSetIndex, vTexResSet);
        cl.SetGraphicsResourceSet(FTextureSamplerResourceSetIndex, fTexResSet);
        cl.SetGraphicsResourceSet(VertexUniformResourceSetIndex, vUniformResSet);
        cl.SetGraphicsResourceSet(FragmentUniformResourceSetIndex, fUniformResSet);
    }
}