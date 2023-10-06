using MiaCrate.Commands.Arguments;
using MiaCrate.Server;
using MiaCrate.Server.Levels;
using MiaCrate.Texts;
using MiaCrate.World.Dimensions;
using MiaCrate.World.Entities;
using MiaCrate.World.Phys;
using Mochi.Brigadier;
using Mochi.Brigadier.Exceptions;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Commands;

public class CommandSourceStack : ISharedSuggestionProvider
{
    public delegate T BinaryOperator<T>(T a, T b);
    
    public static SimpleCommandExceptionType ErrorNotPlayer { get; } =
        BrigadierUtil.CreateSimpleExceptionType(MiaComponent.Translatable("permissions.requires.player"));
    
    public static SimpleCommandExceptionType ErrorNotEntity { get; } =
        BrigadierUtil.CreateSimpleExceptionType(MiaComponent.Translatable("permissions.requires.entity"));
    
    private readonly ICommandSource _source;
    private readonly Vec3 _worldPosition;
    private readonly Vec2 _rotation;
    private readonly ServerLevel _level;
    private readonly PermissionLevel _permissionLevel;
    private readonly string _textName;
    private readonly IComponent _displayName;
    private readonly Entity? _entity;
    private readonly bool _silent;
    private readonly ResultConsumer<CommandSourceStack>? _consumer;
    private readonly EntityAnchor _anchor;
    private readonly ICommandSigningContext _signingContext;
    private readonly ITaskChainer _chatMessageChainer;
    private readonly Action<int> _returnValueConsumer;

    public Entity? Entity => _entity;
    public ServerPlayer? Player => _entity as ServerPlayer;
    public bool IsPlayer => _entity is ServerPlayer;
    public Vec2 Rotation => _rotation;
    public Vec3 Position => _worldPosition;
    public IEnumerable<string> OnlinePlayerNames => throw new NotImplementedException();
    public GameServer Server { get; }

    public CommandSourceStack(ICommandSource source, Vec3 worldPosition, Vec2 rotation, ServerLevel level,
        PermissionLevel permissionLevel, string textName, IComponent displayName, GameServer server, Entity? entity)
        : this(source, worldPosition, rotation, level, permissionLevel, textName, displayName, server, entity,
            false, (_, _, _) => { }, EntityAnchor.Feet, ICommandSigningContext.Anonymous,
            ITaskChainer.CreateImmediate(server), _ => { }) {}
    
    private CommandSourceStack(ICommandSource source, Vec3 worldPosition, Vec2 rotation, ServerLevel level,
        PermissionLevel permissionLevel, string textName, IComponent displayName, GameServer server, Entity? entity,
        bool silent, ResultConsumer<CommandSourceStack>? consumer, EntityAnchor anchor, 
        ICommandSigningContext signingContext, ITaskChainer chatMessageChainer, Action<int> returnValueConsumer)
    {
        _source = source;
        _worldPosition = worldPosition;
        _rotation = rotation;
        _level = level;
        _permissionLevel = permissionLevel;
        _textName = textName;
        _displayName = displayName;
        Server = server;
        _entity = entity;
        _silent = silent;
        _consumer = consumer;
        _anchor = anchor;
        _signingContext = signingContext;
        _chatMessageChainer = chatMessageChainer;
        _returnValueConsumer = returnValueConsumer;
    }

    public CommandSourceStack WithSource(ICommandSource source)
    {
        return _source == source 
            ? this
            : new CommandSourceStack(source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithEntity(Entity entity)
    {
        return _entity == entity
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }

    
    public CommandSourceStack WithPosition(Vec3 position)
    {
        return _worldPosition == position
            ? this
            : new CommandSourceStack(_source, position, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithRotation(Vec2 rotation)
    {
        return _rotation == rotation
            ? this
            : new CommandSourceStack(_source, _worldPosition, rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }

    public CommandSourceStack WithCallback(ResultConsumer<CommandSourceStack> consumer)
    {
        return _consumer == consumer
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack AddCallback(ResultConsumer<CommandSourceStack> consumer)
    {
        var c = _consumer == null ? consumer : _consumer + consumer;
        return WithCallback(c);
    }
    
    public CommandSourceStack WithCallback(ResultConsumer<CommandSourceStack> consumer, BinaryOperator<ResultConsumer<CommandSourceStack>> merge)
    {
        var c = _consumer == null ? consumer : merge(_consumer, consumer);
        return WithCallback(c);
    }

    public CommandSourceStack WithSuppressedOutput()
    {
        return !_silent && !_source.AlwaysAccepts
            ? new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, true, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer)
            : this;
    }
    
    public CommandSourceStack WithPermission(PermissionLevel permissionLevel)
    {
        return _permissionLevel == permissionLevel
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithMaximumPermission(PermissionLevel permissionLevel)
    {
        return _permissionLevel >= permissionLevel
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithAnchor(EntityAnchor anchor)
    {
        return _anchor == anchor
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, anchor, _signingContext, _chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithLevel(ServerLevel level)
    {
        if (_level == level)
            return this;

        var d = DimensionType.GetTeleportationScale(_level.DimensionType, level.DimensionType);
        var v = new Vec3(_worldPosition.X * d, _worldPosition.Y, _worldPosition.Z * d);
        return new CommandSourceStack(_source, v, _rotation, level, _permissionLevel, _textName,
            _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
            _returnValueConsumer);
    }

    public CommandSourceStack Facing(Entity entity, EntityAnchor anchor) => Facing(anchor.Apply(entity));

    public CommandSourceStack Facing(Vec3 target)
    {
        var v = _anchor.Apply(this);
        var dx = target.X - v.X;
        var dy = target.Y - v.Y;
        var dz = target.Z - v.Z;
        var g = Math.Sqrt(dx * dx + dz * dz);

        var h = Util.WrapDegrees(-Math.Atan2(dy, g) * Mth.RadToDeg);
        var i = Util.WrapDegrees(-Math.Atan2(dz, dx) * Mth.RadToDeg - 90);
        return WithRotation(new Vec2((float) h, (float) i));
    }
    
    public CommandSourceStack WithSigningContext(ICommandSigningContext signingContext, ITaskChainer chatMessageChainer)
    {
        return _signingContext == signingContext && _chatMessageChainer == chatMessageChainer
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, signingContext, chatMessageChainer,
                _returnValueConsumer);
    }
    
    public CommandSourceStack WithReturnValueConsumer(Action<int> returnValueConsumer)
    {
        return _returnValueConsumer == returnValueConsumer
            ? this
            : new CommandSourceStack(_source, _worldPosition, _rotation, _level, _permissionLevel, _textName,
                _displayName, Server, _entity, _silent, _consumer, _anchor, _signingContext, _chatMessageChainer,
                returnValueConsumer);
    }

    public Entity GetEntityOrThrow() => _entity ?? throw ErrorNotEntity.Create();

    public ServerPlayer GetPlayerOrThrow() => Player ?? throw ErrorNotPlayer.Create();

    public bool HasPermission(PermissionLevel level) => _permissionLevel >= level;
}