using OpenTK.Mathematics;

namespace MiaCrate.Client;

public interface IVertexSorting
{
    public static IVertexSorting DistanceToOrigin { get; } = ByDistance(0, 0, 0);
    public static IVertexSorting OrthoZ { get; } = ByDistance(IDistanceFunction.Create(v => -v.Z));
    
    int[] Sort(Vector3[] vectors);

    public static IVertexSorting ByDistance(float x, float y, float z) =>
        ByDistance(new Vector3(x, y, z));
    
    public static IVertexSorting ByDistance(Vector3 v) => 
        ByDistance(IDistanceFunction.Create(x => x.LengthSquared));

    public static IVertexSorting ByDistance(IDistanceFunction func) => new ByDist(func);
    
    private class ByDist : IVertexSorting
    {
        private readonly IDistanceFunction _func;

        public ByDist(IDistanceFunction func)
        {
            _func = func;
        }

        public int[] Sort(Vector3[] vectors)
        {
            var fs = new float[vectors.Length];
            var iArr = Enumerable.Range(0, vectors.Length);
            
            for (var i = 0; i < vectors.Length; i++)
            {
                fs[i] = _func.Apply(vectors[i]);
            }

            return iArr.OrderBy(i => fs[i]).ToArray();
        }
    }
    
    public interface IDistanceFunction
    {
        float Apply(Vector3 v);

        public static IDistanceFunction Create(Func<Vector3, float> func) => new Instance(func);

        private class Instance : IDistanceFunction
        {
            private readonly Func<Vector3, float> _func;

            public Instance(Func<Vector3, float> func)
            {
                _func = func;
            }

            public float Apply(Vector3 v) => _func(v);
        }
    }
}