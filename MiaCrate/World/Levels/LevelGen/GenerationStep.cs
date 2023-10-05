namespace MiaCrate.World;

public class GenerationStep
{
    public sealed class Decoration : IStringRepresentable
    {
        public string SerializedName => Name;
        public string Name { get; }

        private Decoration(string name)
        {
            Name = name;
        }
    }
}