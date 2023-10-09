using MiaCrate.Client.Realms;
using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using MiaCrate.Common;
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

    public void BindForSetup(ResourceLocation location) => 
        RenderSystem.EnsureOnRenderThread(() => InternalBind(location));

    private void InternalBind(ResourceLocation location)
    {
        // TODO: Maybe replace these with a GetTexture() call?
        if (!_byPath.TryGetValue(location, out var texture))
        {
            texture = new SimpleTexture(location);
            Register(location, texture);
        }
        
        texture.Bind();
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

    public ResourceLocation RegisterDynamic(string name, DynamicTexture texture)
    {
        if (!_prefixRegister.TryGetValue(name, out var i))
        {
            // Ensure 'i' is set to 0 just in case that is an undefined behavior 
            i = 0;
        }

        _prefixRegister[name] = ++i;

        var location = new ResourceLocation($"dynamic/{name}_{i}");
        Register(location, texture);
        return location;
    }

    public AbstractTexture GetTexture(ResourceLocation location)
    {
        if (!_byPath.TryGetValue(location, out var texture))
        {
            texture = new SimpleTexture(location);
            Register(location, texture);
        }

        return texture;
    }

    internal Dictionary<ResourceLocation, AbstractTexture> GetAllTextures() => _byPath;

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
        if (_byPath.ContainsKey(location)) return Task.CompletedTask;
        
        var preloaded = new PreloadedTexture(_resourceManager, location, executor);
        _byPath[location] = preloaded;
        return preloaded.Task
            .ThenRunAsync(() => Register(location, preloaded), IExecutor.Create(Execute));
    }

    private static void Execute(IRunnable runnable)
    {
        Game.Instance.Execute(IRunnable.Create(() =>
        {
            RenderSystem.RecordRenderCall(runnable.Run);
        }));
    }

    public Task ReloadAsync(IPreparableReloadListener.IPreparationBarrier barrier, IResourceManager manager, IProfilerFiller preparationProfiler,
        IProfilerFiller reloadProfiler, IExecutor preparationExecutor, IExecutor reloadExecutor)
    {
        var source = new TaskCompletionSource();
        TitleScreen
            .PreloadResourcesAsync(this, preparationExecutor)
            .ThenApplyAsync(() => barrier.Wait(Unit.Instance))
            .ThenAcceptAsync(_ =>
            {
                var missing = MissingTextureAtlasSprite.Texture;
                RealmsPopupScreen.UpdateCarouselImages(_resourceManager);
                
                var removal = new List<ResourceLocation>();

                foreach (var (location, texture) in _byPath.ToList())
                {
                    if (texture == missing &&
                        location != MissingTextureAtlasSprite.Location)
                    {
                        removal.Add(location);
                    }
                    else
                    {
                        texture.Reset(this, manager, location, reloadExecutor);
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
            }, IExecutor.Create(r =>
            {
                RenderSystem.RecordRenderCall(r.Run);
            }));
        
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