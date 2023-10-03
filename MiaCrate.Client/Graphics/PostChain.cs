using System.Text.Json.Nodes;
using MiaCrate.Client.Pipeline;
using MiaCrate.Client.Systems;
using MiaCrate.Json;
using MiaCrate.Resources;
using Mochi.Utils;
using OpenTK.Mathematics;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public class PostChain : IDisposable
{
    private readonly IResourceManager _resourceManager;
    private readonly RenderTarget _screenTarget;
    private float _time;
    private float _lastStamp;
    private Matrix4 _shaderOrthoMatrix;
    private int _screenWidth;
    private int _screenHeight;
    private readonly string _name;
    private readonly List<PostPass> _passes = new();
    private readonly Dictionary<string, RenderTarget> _customRenderTargets = new();
    private readonly List<RenderTarget> _fullSizedTargets = new();

    public PostChain(TextureManager textureManager, IResourceManager resourceManager, RenderTarget screenTarget,
        ResourceLocation location)
    {
        _resourceManager = resourceManager;
        _screenTarget = screenTarget;
        _time = 0f;
        _lastStamp = 0f;
        _screenWidth = screenTarget.ViewWidth;
        _screenHeight = screenTarget.ViewHeight;
        _name = location.ToString();
        
        UpdateOrthoMatrix();
        Load(textureManager, location);
    }

    private void UpdateOrthoMatrix()
    {
        _shaderOrthoMatrix = Matrix4.CreateOrthographic(_screenTarget.Width, _screenTarget.Height, 0.1f, 1000f);
    }

    public void Resize(int width, int height)
    {
        _screenWidth = _screenTarget.Width;
        _screenHeight = _screenTarget.Height;
        UpdateOrthoMatrix();
        
        foreach (var pass in _passes)
        {
            pass.SetOrthoMatrix(_shaderOrthoMatrix);
        }
        
        foreach (var target in _fullSizedTargets)
        {
            target.Resize(width, height, Game.OnMacOs);
        }
    }

    private void Load(TextureManager manager, ResourceLocation location)
    {
        var resource = _resourceManager.GetResourceOrThrow(location);
        using var reader = resource.Open();

        try
        {
            var obj = JsonNode.Parse(reader)!.AsObject();
            if (obj.TryGetPropertyValue("targets", out var targetNode) && targetNode is JsonArray)
            {
                var arr = targetNode.AsArray();
                var i = 0;

                foreach (var node in arr)
                {
                    try
                    {
                        ParseTargetNode(node!);
                    }
                    catch (Exception ex)
                    {
                        var chained = ChainedJsonException.ForException(ex);
                        chained.PrependJsonKey($"targets[{i}]");
                        throw chained;
                    }

                    i++;
                }
            }

            if (obj.TryGetPropertyValue("passes", out var passesNode) && passesNode is JsonArray)
            {
                var arr = passesNode.AsArray();
                var i = 0;

                foreach (var node in arr)
                {
                    try
                    {
                        ParsePassNode(manager, node!);
                    }
                    catch (Exception ex)
                    {
                        var chained = ChainedJsonException.ForException(ex);
                        chained.PrependJsonKey($"passes[{i}]");
                        throw chained;
                    }

                    i++;
                }
            }
        }
        catch (Exception ex)
        {
            var chained = ChainedJsonException.ForException(ex);
            chained.SetFileNameAndFlush($"{location.Path} ({resource.SourcePackId})");
            throw chained;
        }
    }

    private void ParsePassNode(TextureManager manager, JsonNode node)
    {
        var obj = node.AsObject();
        var name = obj["name"]!.GetValue<string>();
        var inTarget = obj["intarget"]!.GetValue<string>();
        var outTarget = obj["outtarget"]!.GetValue<string>();

        var target = GetRenderTarget(inTarget);
        var target2 = GetRenderTarget(outTarget);

        if (target == null)
            throw new ChainedJsonException($"Input target '{inTarget}' does not exist");

        if (target2 == null)
            throw new ChainedJsonException($"Output target '{outTarget}' does not exist");

        var postPass = AddPass(name, target, target2);
        var arr = obj["auxtargets"]?.AsArray();
        if (arr != null)
        {
            var i = 0;

            foreach (var targetNode in arr)
            {
                try
                {
                    var targetName = targetNode!["name"]!.GetValue<string>();
                    var id = targetNode["id"]!.GetValue<string>();
                    bool bl;
                    string s6;

                    if (id.EndsWith(":depth"))
                    {
                        bl = true;
                        s6 = id[..id.LastIndexOf(':')];
                    }
                    else
                    {
                        bl = false;
                        s6 = id;
                    }

                    var renderTarget = GetRenderTarget(s6);
                    if (renderTarget == null)
                    {
                        if (bl)
                            throw new ChainedJsonException($"Render target '{s6}' can't be used as depth buffer");

                        var location = new ResourceLocation($"textures/effect/{s6}.png");
                        _resourceManager.GetResource(location).OrElseGet(() =>
                            throw new ChainedJsonException($"Render target or texture '{s6}' does not exist"));

                        RenderSystem.SetShaderTexture(0, location);
                        manager.BindForSetup(location);

                        var texture = manager.GetTexture(location);
                        var j = targetNode["width"]!.GetValue<int>();
                        var k = targetNode["height"]!.GetValue<int>();
                        var bl2 = targetNode["bilinear"]!.GetValue<bool>();

                        if (bl2)
                        {
                            texture.Texture!.ModifySampler((ref SamplerDescription sd) =>
                            {
                                sd.Filter = SamplerFilter.MinLinear_MagLinear_MipLinear;
                            });
                        }
                        else
                        {
                            texture.Texture!.ModifySampler((ref SamplerDescription sd) =>
                            {
                                sd.Filter = SamplerFilter.MinPoint_MagPoint_MipPoint;
                            });
                        }

                        postPass.AddAuxAsset(targetName, texture, t => t.Texture!, j, k);
                    }
                    else if (bl)
                    {
                        postPass.AddAuxAsset(targetName, renderTarget, t => t.DepthBuffer!, renderTarget.Width,
                            renderTarget.Height);
                    }
                    else
                    {
                        postPass.AddAuxAsset(targetName, renderTarget, t => t.ColorTexture!, renderTarget.Width,
                            renderTarget.Height);
                    }

                    i++;
                }
                catch (Exception ex)
                {
                    var chained = ChainedJsonException.ForException(ex);
                    chained.PrependJsonKey($"auxtargets[{i}]");
                    throw chained;
                }
            }
        }

        var uniformsArr = obj["uniforms"]?.AsArray();
        if (uniformsArr != null)
        {
            var i = 0;

            foreach (var uniformNode in uniformsArr)
            {
                try
                {
                    ParseUniformNode(uniformNode!);
                }
                catch (Exception ex)
                {
                    var chained = ChainedJsonException.ForException(ex);
                    chained.PrependJsonKey($"uniforms[{i}]");
                    throw chained;
                }
            }
        }
    }

    private void ParseUniformNode(JsonNode node)
    {
        var obj = node.AsObject();
        var name = obj["name"]!.GetValue<string>();
        var uniform = _passes.Last().Effect.GetUniform(name);
        if (uniform == null)
            throw new ChainedJsonException($"Uniform '{name}' does not exist");

        var fs = new float[4];
        var i = 0;
        var values = obj["values"]!.AsArray();
        
        foreach (var valNode in values)
        {
            try
            {
                fs[i] = valNode!.GetValue<float>();
                i++;
            }
            catch (Exception ex)
            {
                var chained = ChainedJsonException.ForException(ex);
                chained.PrependJsonKey($"values[{i}]");
                throw chained;
            }
        }

        switch (i)
        {
            case 1:
                uniform.Set(fs[0]);
                break;
            
            case 2:
                uniform.Set(fs[0], fs[1]);
                break;
            
            case 3:
                uniform.Set(fs[0], fs[1], fs[2]);
                break;
            
            case 4:
                uniform.Set(fs[0], fs[1], fs[2], fs[3]);
                break;
        }
    }

    public PostPass AddPass(string name, RenderTarget input, RenderTarget output)
    {
        var pass = new PostPass(_resourceManager, name, input, output);
        _passes.Add(pass);
        return pass;
    }

    private RenderTarget? GetRenderTarget(string? target)
    {
        if (target == null) return null;
        return target == "minecraft:main"
            ? _screenTarget
            : _customRenderTargets.GetValueOrDefault(target);
    }

    private void ParseTargetNode(JsonNode node)
    {
        if (node is JsonValue val && val.TryGetValue(out string? str))
        {
            AddTempTarget(str, _screenWidth, _screenHeight);
            return;
        }

        var obj = node.AsObject();
        var name = obj["name"]!.GetValue<string>();
        var i = obj["width"]?.GetValue<int>() ?? _screenWidth;
        var j = obj["height"]?.GetValue<int>() ?? _screenHeight;

        if (_customRenderTargets.ContainsKey(name))
            throw new ChainedJsonException($"{name} is already defined");
        
        AddTempTarget(name, i, j);
    }

    private void AddTempTarget(string name, int width, int height)
    {
        var target = new TextureRenderTarget(width, height, true, Game.OnMacOs);
        target.SetClearColor(0, 0, 0, 0);
        _customRenderTargets[name] = target;

        if (width == _screenWidth && height == _screenHeight)
        {
            _fullSizedTargets.Add(target);
        }
    }

    public void Dispose()
    {
        foreach (var target in _customRenderTargets.Values)
        {
            target.DestroyBuffers();
        }

        foreach (var pass in _passes)
        {
            pass.Dispose();
        }
        
        _passes.Clear();
    }

    public RenderTarget GetTempTarget(string name)
    {
        return _customRenderTargets[name];
    }
}