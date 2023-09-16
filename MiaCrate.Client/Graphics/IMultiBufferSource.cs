using MiaCrate.Data.Optic;

namespace MiaCrate.Client.Graphics;

public interface IMultiBufferSource
{
    public IVertexConsumer GetBuffer(RenderType type);
    
    public class BufferSource : IMultiBufferSource
    {
        public IVertexConsumer GetBuffer(RenderType type)
        {
            throw new NotImplementedException();
        }

        public void EndBatch()
        {
            throw new NotImplementedException();
        }
    }
}