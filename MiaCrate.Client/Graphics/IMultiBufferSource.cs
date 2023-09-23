using MiaCrate.Client.Systems;
using MiaCrate.Data.Optic;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public interface IMultiBufferSource
{
    public IVertexConsumer GetBuffer(RenderType type);

    public static BufferSource Immediate(BufferBuilder builder) =>
        ImmediateWithBuffers(new Dictionary<RenderType, BufferBuilder>(), builder);

    public static BufferSource ImmediateWithBuffers(Dictionary<RenderType, BufferBuilder> dict, BufferBuilder builder) => 
        new(builder, dict);

    public class BufferSource : IMultiBufferSource
    {
        private readonly Dictionary<RenderType, BufferBuilder> _fixedBuffers;
        private readonly BufferBuilder _builder;
        private readonly HashSet<BufferBuilder> _startedBuffers = new();
        private IOptional<RenderType> _lastState = Optional.Empty<RenderType>();

        public BufferSource(BufferBuilder builder, Dictionary<RenderType, BufferBuilder> fixedBuffers)
        {
            _builder = builder;
            _fixedBuffers = fixedBuffers;
        }
        
        public IVertexConsumer GetBuffer(RenderType type)
        {
            var optional = type.AsOptional();
            var builder = GetBuilderRaw(type);
            if (_lastState == optional && type.CanConsolidateConsecutiveGeometry) 
                return builder;
            
            if (_lastState.IsPresent)
            {
                var t = _lastState.Value;
                if (_fixedBuffers.ContainsKey(t))
                {
                    EndBatch(t);
                }
            }

            if (_startedBuffers.Add(builder))
            {
                builder.Begin(type.Mode, type.Format);
            }

            _lastState = optional;
            return builder;
        }

        private BufferBuilder GetBuilderRaw(RenderType type) => _fixedBuffers.GetValueOrDefault(type, _builder);

        public void EndBatch()
        {
            _lastState.IfPresent(type =>
            {
                var consumer = GetBuffer(type);
                if (consumer == _builder)
                    EndBatch(type);
            });

            foreach (var type in _fixedBuffers.Keys)
            {
                EndBatch(type);
            }
        }
        
        public void EndBatch(RenderType type)
        {
            var builder = GetBuilderRaw(type);
            var bl = _lastState == type.AsOptional();
            if (bl || builder != _builder)
            {
                if (_startedBuffers.Remove(builder))
                {
                    type.End(builder, RenderSystem.VertexSorting);
                    if (bl)
                    {
                        _lastState = Optional.Empty<RenderType>();
                    }
                }
            }
        }
        
    }
}