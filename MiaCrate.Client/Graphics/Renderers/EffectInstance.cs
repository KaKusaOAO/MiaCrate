﻿using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Json;
using MiaCrate.Resources;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Graphics;

public class EffectInstance : IEffect, IDisposable
{
    private const string EffectShaderPath = $"{ShaderInstance.ShaderPath}/program/";
    private const bool AlwaysReapply = true;

    private static readonly AbstractUniform DummyUniform = new();
    private static int _lastProgramId = -1;
    private static EffectInstance? _lastAppliedEffect = null;
    
    private readonly Dictionary<string, Func<int>?> _samplerMap = new();
    private readonly List<string> _samplerNames = new();
    private readonly List<int> _samplerLocations = new();
    private readonly List<Uniform> _uniforms = new();
    private readonly List<int> _uniformLocations = new();
    private readonly Dictionary<string, Uniform> _uniformMap = new();
    private readonly List<int>? _attributes;
    private readonly List<string>? _attributeNames;
    private bool _dirty;
    private readonly BlendMode _blend;
    
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

            _blend = ParseBlendNode(json["blend"]?.AsObject());
            VertexProgram = GetOrCreate(manager, ProgramType.Vertex, vertex);
            FragmentProgram = GetOrCreate(manager, ProgramType.Fragment, fragment);
            Id = ProgramManager.CreateProgram();
            ProgramManager.LinkShader(this);
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
        var list = new List<int>();

        for (var i = 0; i < _samplerNames.Count; i++)
        {
            var name = _samplerNames[i];
            var location = Uniform.GetUniformLocation(Id, name);
            if (location == -1)
            {
                Logger.Warn($"Shader {Name} could not find sampler named {name} in the specified shader program.");
                _samplerMap.Remove(name);
                list.Add(i);
            }
            else
            {
                _samplerLocations.Add(location);
            }
        }

        for (var i = list.Count - 1; i >= 0; i--)
        {
            _samplerNames.RemoveAt(list[i]);
        }
        
        foreach (var uniform in _uniforms)
        {
            var name = uniform.Name;
            var location = Uniform.GetUniformLocation(Id, name);
            if (location == -1)
            {
                Logger.Warn($"Shader {Name} could not find uniform named {name} in the specified shader program.");
            }
            else
            {
                _uniformLocations.Add(location);
                uniform.Location = location;
                _uniformMap[name] = uniform;
            }
        }
    }

    public void MarkDirty()
    {
        _dirty = true;
    }

    public Uniform? GetUniform(string name)
    {
        RenderSystem.AssertOnRenderThread();
        return _uniformMap.GetValueOrDefault(name);
    }

    public static EffectProgram GetOrCreate(IResourceManager manager, ProgramType type, string str)
    {
        var hasValue = type.Programs.TryGetValue(str, out var program);
        var effectProgram = (program as EffectProgram)!;
        if (hasValue && program is not EffectProgram)
            throw new Exception("Program is not of type EffectProgram");
        
        if (hasValue) return effectProgram;

        var location = new ResourceLocation(EffectShaderPath + str + type.Extension);
        var resource = manager.GetResourceOrThrow(location);
        using var stream = resource.Open();
        return EffectProgram.CompileShader(type, str, stream, resource.Source.PackId);
    }

    public static BlendMode ParseBlendNode(JsonObject? obj)
    {
        if (obj == null) return new BlendMode();
        var blendFunc = BlendEquationMode.FuncAdd;
        var srcRgb = BlendingFactorSrc.One;
        var dstRgb = BlendingFactorDest.Zero;
        var srcAlpha = BlendingFactorSrc.One;
        var dstAlpha = BlendingFactorDest.Zero;
        var isDefault = true;
        var isSeparate = false;

        if (obj.TryGetPropertyValue("func", out var funcNode))
        {
            blendFunc = BlendMode.StringToBlendFunc(funcNode!.GetValue<string>());
            if (blendFunc != BlendEquationMode.FuncAdd) isDefault = false;
        }

        if (obj.TryGetPropertyValue("srcrgb", out var srcRgbNode))
        {
            srcRgb = BlendMode.StringToBlendFactorSrc(srcRgbNode!.GetValue<string>());
            if (srcRgb != BlendingFactorSrc.One) isDefault = false;
        }
        
        if (obj.TryGetPropertyValue("dstrgb", out var dstRgbNode))
        {
            dstRgb = BlendMode.StringToBlendFactorDest(dstRgbNode!.GetValue<string>());
            if (dstRgb != BlendingFactorDest.Zero) isDefault = false;
        }
        
        if (obj.TryGetPropertyValue("srcalpha", out var srcAlphaNode))
        {
            srcAlpha = BlendMode.StringToBlendFactorSrc(srcAlphaNode!.GetValue<string>());
            if (srcAlpha != BlendingFactorSrc.One) isDefault = false;
            isSeparate = true;
        }
        
        if (obj.TryGetPropertyValue("dstalpha", out var dstAlphaNode))
        {
            dstAlpha = BlendMode.StringToBlendFactorDest(dstAlphaNode!.GetValue<string>());
            if (dstAlpha != BlendingFactorDest.Zero) isDefault = false;
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
        ProgramManager.UseProgram(0);
        _lastProgramId = -1;
        _lastAppliedEffect = null;
        
        for (var i = 0; i < _samplerNames.Count; i++)
        {
            if (!_samplerMap.ContainsKey(_samplerNames[i]) || _samplerMap[_samplerNames[i]] == null) continue;
            GlStateManager.ActiveTexture((int) TextureUnit.Texture0 + i);
            GlStateManager.BindTexture(0);
        }
    }

    public void Apply()
    {
        RenderSystem.AssertOnGameThread();
        _dirty = false;
        _lastAppliedEffect = this;
        _blend.Apply();

        if (Id != _lastProgramId)
        {
            ProgramManager.UseProgram(Id);
            _lastProgramId = Id;
        }
        
        for (var i = 0; i < _samplerLocations.Count; i++)
        {
            var name = _samplerNames[i];
            if (_samplerMap.TryGetValue(name, out var func) && func != null)
            {
                RenderSystem.ActiveTexture((int) TextureUnit.Texture0 + i);
                var j = func!();
                if (j != -1)
                {
                    RenderSystem.BindTexture(j);
                    Uniform.UploadInteger(_samplerLocations[i], i);
                }
            }
        }
        
        foreach (var uniform in _uniforms)
        {
            uniform.Upload();
        }
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