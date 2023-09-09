using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Resources;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class TextureManager : IPreparableReloadListener, ITickable, IDisposable
{
    public static readonly ResourceLocation IntentionalMissingTexture = new("");
    private readonly Dictionary<ResourceLocation, AbstractTexture> _byPath = new();
    private readonly HashSet<ITickable> _tickableTextures = new();
    private readonly Dictionary<string, int> _prefixRegister = new();
    private readonly IResourceManager _resourceManager;

    public TextureManager(IResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    private AbstractTexture LoadTexture(ResourceLocation location, AbstractTexture texture)
    {
        try
        {
            texture.Load(_resourceManager);
            return texture;
        }
        catch (IOException ex)
        {
            if (location != IntentionalMissingTexture)
            {
                Logger.Warn($"Failed to load texture: {location}");
                Logger.Warn(ex);
            }

            return MissingTextureAtlasSprite.Texture;
        }
        catch (Exception ex)
        {
            var report = CrashReport.ForException(ex, "Registering texture");
            throw new ReportedException(report);
        }
    }

    public void Register(ResourceLocation location, AbstractTexture texture)
    {
        texture = LoadTexture(location, texture);

        var old = _byPath.GetValueOrDefault(location);
        _byPath[location] = texture;

        if (texture == old) return;
        
        if (old != null && old != MissingTextureAtlasSprite.Texture) 
            SafeDispose(location, old);

        if (texture is ITickable tickable) 
            _tickableTextures.Add(tickable);
    }

    private void SafeDispose(ResourceLocation location, AbstractTexture texture)
    {
        if (texture != MissingTextureAtlasSprite.Texture)
        {
            if (texture is ITickable tickable)
            {
                _tickableTextures.Remove(tickable);
            }

            try
            {
                texture.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to close texture {location}");
                Logger.Warn(ex);
            }
        }
        
        texture.ReleaseId();
    }

    public Task PreloadAsync(ResourceLocation location, IExecutor executor)
    {
        if (!_byPath.ContainsKey(location)) return Task.CompletedTask;
        
        var preloaded = new PreloadedTexture(_resourceManager, location, executor);
        _byPath[location] = preloaded;
            
        return preloaded.Task.ContinueWith(_ =>
        {
            Execute(IRunnable.Create(() =>
            {
                Register(location, preloaded);
            }));
        });
    }

    private static void Execute(IRunnable runnable)
    {
        Game.Instance.Execute(IRunnable.Create(() =>
        {
            RenderSystem.RecordRenderCall(runnable.Run);
        }));
    }

    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller profiler,
        IProfilerFiller profile2, IExecutor executor, IExecutor executor2)
    {
        var source = new TaskCompletionSource();
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.WhenAll(
                    TitleScreen.PreloadResourcesAsync(this, executor),
                    PreloadAsync(AbstractWidget.WidgetsLocation, executor)
                );
                await barrier.Wait(Unit.Instance);
                RenderSystem.RecordRenderCall(() =>
                {
                    var removal = new List<ResourceLocation>();

                    foreach (var (location, texture) in _byPath)
                    {
                        if (texture == MissingTextureAtlasSprite.Texture &&
                            location != MissingTextureAtlasSprite.Location)
                        {
                            removal.Add(location);
                        }
                        else
                        {
                            texture.Reset(this, manager, location, executor2);
                        }
                    }

                    foreach (var item in removal)
                    {
                        _byPath.Remove(item);
                    }

                    Game.Instance.Tell(IRunnable.Create(() =>
                    {
                        source.SetResult();
                    }));
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                source.SetException(ex);
            }
        });
        
        return source.Task;
    }

    public void Tick()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}