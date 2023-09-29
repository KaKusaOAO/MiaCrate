using System.Text;
using System.Text.RegularExpressions;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Systems;
using Mochi.Extensions;
using Veldrid;

namespace MiaCrate.Client.Shaders;

public class Program : IDisposable
{
    private const int MaxLogLength = 32678;
    private readonly ProgramType _type;
    public string Name { get; }
    public Shader Id { get; private set; }

    public Program(ProgramType type, Shader id, string name)
    {
        Name = name;
        Id = id;
        _type = type;
    }

    public void Dispose()
    {
        RenderSystem.AssertOnRenderThread();
        GC.SuppressFinalize(this);
        Id.Dispose();
        _type.Programs.Remove(Name);
    }

    public static ConvertResult CompileShader(ProgramType type, string name, Stream stream, string str2,
        GlslPreprocessor preprocessor)
    {
        RenderSystem.AssertOnRenderThread();
        var result = InternalCompileShader(type, name, stream, str2, preprocessor);
        type.Programs[name] = result;
        return result;
    }

    protected static ConvertResult InternalCompileShader(ProgramType type, string name, Stream stream, string str2,
        GlslPreprocessor preprocessor)
    {
        string source;

        try
        {
            using var reader = new StreamReader(stream);
            source = reader.ReadToEnd();
        }
        catch (Exception)
        {
            throw new IOException($"Could not load program {type.Name}");
        }

        var processed = string.Join('\n', preprocessor.Process(source));
        return ConvertToVeldridUsable(processed, 
            type == ProgramType.Vertex ? ShaderStages.Vertex : ShaderStages.Fragment);
    }

    public record ConvertResult(string Source,
        Dictionary<int, List<ResourceLayoutElementDescription>> ResourceLayoutsLayoutElementDescriptions);

    private static VertexElementFormat FromType(string type)
    {
        return type switch
        {
            "vec2" => VertexElementFormat.Float2,
            "vec3" => VertexElementFormat.Float3,
            "vec4" => VertexElementFormat.Float4,
            "ivec2" => VertexElementFormat.Int2
        };
    }
    
    private static ConvertResult ConvertToVeldridUsable(string glsl, ShaderStages stage)
    {
        var inRegex = new Regex("in\\s(.*?)\\s(.*?);");
        var outRegex = new Regex("out\\s(.*?)\\s(.*?);");
        var uniformRegex = new Regex("uniform\\s(.*?)\\s(.*?);");

        var temp = glsl;
        var matches = inRegex.Matches(temp);
        var sb = new StringBuilder();
        // sb.AppendLine("#version 450").AppendLine();

        // In attributes
        var counter = 0;
        var index = 0;
        foreach (Match match in matches)
        {
            sb.Append(temp[index..match.Index]);
            sb.Append($"layout(location = {counter++}) ");
            sb.Append(match.Value);
            index = match.Index + match.Length;
        }

        sb.Append(temp[index..]);
        temp = sb.ToString();
        matches = outRegex.Matches(temp);
        sb.Clear();

        // Out attributes
        counter = 0;
        index = 0;
        foreach (Match match in matches)
        {
            sb.Append(temp[index..match.Index]);
            sb.Append($"layout(location = {counter++}) ");
            sb.Append(match.Value);
            index = match.Index + match.Length;
        }
        
        sb.Append(temp[index..]);
        temp = sb.ToString();
        matches = uniformRegex.Matches(temp);
        sb.Clear();
        
        // Uniform blocks
        counter = 0;
        index = 0;
        
        var set = stage == ShaderStages.Vertex 
            ? ShaderInstance.VertexUniformResourceSetIndex 
            : ShaderInstance.FragmentUniformResourceSetIndex;

        var bindings = new Dictionary<int, int>();
        var elements = new Dictionary<int, List<ResourceLayoutElementDescription>>();
        var handleSamplerCalls = new List<(string, string)>();
        
        foreach (Match match in matches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;

            sb.Append(temp[index..match.Index]);
            
            if (type == "sampler2D")
            {
                var texName = name.Replace("Sampler", "Texture");
                
                var texBind = bindings.ComputeIfAbsent(ShaderInstance.TextureResourceSetIndex, _ => 0);
                sb.Append($"layout(set = {ShaderInstance.TextureResourceSetIndex}, binding = {texBind}) uniform texture2D ");
                sb.Append(texName);
                sb.AppendLine(";");
                
                var samplerBind = bindings.ComputeIfAbsent(ShaderInstance.SamplerResourceSetIndex, _ => 0);
                sb.Append($"layout(set = {ShaderInstance.SamplerResourceSetIndex}, binding = {samplerBind}) uniform sampler ");
                sb.Append(name);
                sb.Append(";");

                bindings[ShaderInstance.TextureResourceSetIndex]++;
                bindings[ShaderInstance.SamplerResourceSetIndex]++;

                var textures = elements.ComputeIfAbsent(ShaderInstance.TextureResourceSetIndex,
                    _ => new List<ResourceLayoutElementDescription>());
                var samplers = elements.ComputeIfAbsent(ShaderInstance.SamplerResourceSetIndex,
                    _ => new List<ResourceLayoutElementDescription>());
                textures.Add(new ResourceLayoutElementDescription(texName, ResourceKind.TextureReadOnly, stage));
                samplers.Add(new ResourceLayoutElementDescription(name, ResourceKind.Sampler, stage));

                handleSamplerCalls.Add((texName, name));
            }
            else
            {
                sb.Append($"layout(set = {set}, binding = {counter++}) uniform ");
                sb.Append('_').Append(name);
                sb.AppendLine(" {");
            
                sb.Append("    ");
                sb.Append(type);
                sb.Append(' ').Append(name).AppendLine(";");
                sb.AppendLine("};");
                
                var uniforms = elements.ComputeIfAbsent(set,
                    _ => new List<ResourceLayoutElementDescription>());
                uniforms.Add(new ResourceLayoutElementDescription($"_{name}", ResourceKind.UniformBuffer, stage));
            }
            
            index = match.Index + match.Length;
        }
        
        sb.Append(temp[index..]);
        temp = sb.ToString();
        sb.Clear();

        foreach (var samplerName in handleSamplerCalls)
        {
            temp = temp
                .Replace($"texture({samplerName.Item2},",
                    $"texture(sampler2D({samplerName.Item1}, {samplerName.Item2}),")

                .Replace($"texelFetch({samplerName.Item2},",
                    $"texelFetch(sampler2D({samplerName.Item1}, {samplerName.Item2}),")

                .Replace($"textureProj({samplerName.Item2},",
                    $"textureProj(sampler2D({samplerName.Item1}, {samplerName.Item2}),")

                .Replace($"minecraft_sample_lightmap({samplerName.Item2},",
                    $"minecraft_sample_lightmap({samplerName.Item1}, {samplerName.Item2},");
        }

        if (temp.Contains("vec4 minecraft_sample_lightmap(sampler2D lightMap"))
        {
            temp = temp.Replace("vec4 minecraft_sample_lightmap(sampler2D lightMap,",
                    "vec4 minecraft_sample_lightmap(texture2D lightTex, sampler lightSampler,")
                .Replace("texture(lightMap, ", "texture(sampler2D(lightTex, lightSampler), ");
        }

        temp = temp.Replace("gl_VertexID", "gl_VertexIndex");
        
        // Replace version
        temp = "#version 450\n" + string.Join('\n', temp.Split('\n')
            .Where(l => !l.Trim().StartsWith("#version ")));

        return new ConvertResult(temp, elements);
    }
}