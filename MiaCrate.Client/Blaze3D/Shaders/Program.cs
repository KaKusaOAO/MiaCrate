using System.Text;
using System.Text.RegularExpressions;
using MiaCrate.Client.Preprocessor;
using MiaCrate.Client.Systems;
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
        List<ResourceLayoutElementDescription> ResourceLayoutsLayoutElementDescriptions);

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

        var attribs = new List<VertexElementDescription>();
        var list = new List<ResourceLayoutElementDescription>();
        
        var temp = glsl;
        var matches = inRegex.Matches(temp);
        var sb = new StringBuilder();

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
        
        temp = sb.ToString();
        matches = outRegex.Matches(temp);
        sb.Clear();
        
        // Uniform blocks
        counter = 0;
        index = 0;
        var set = stage == ShaderStages.Vertex ? 0 : 1;
        foreach (Match match in matches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;

            sb.Append(temp[index..match.Index]);
            ResourceKind resourceKind;
            if (type == "sampler2D")
            {
                resourceKind = ResourceKind.TextureReadOnly;
                sb.Append($"layout(set = {set}, binding = {counter++}) uniform sampler2D ");
                sb.Append(name);
                sb.Append(";");
            }
            else
            {
                resourceKind = ResourceKind.UniformBuffer;
                sb.Append($"layout(set = {set}, binding = {counter++}) uniform ");
                sb.Append(name);
                sb.AppendLine(" {");
            
                sb.Append("    ");
                sb.Append(type);
                sb.Append(" _").Append(name).AppendLine(";");
                sb.AppendLine("};");
            }
            
            index = match.Index + match.Length;
            list.Add(new ResourceLayoutElementDescription(name, resourceKind, stage));
        }
        
        temp = sb.ToString();
        return new ConvertResult(temp, list);
    }
}