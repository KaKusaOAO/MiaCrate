using MiaCrate.Core;

namespace MiaCrate.Sounds;

public static partial class SoundEvents
{
    public const int GoatHornVariantCount = 8;
    
    private static SoundEvent Register(string str) => 
        Register(new ResourceLocation(str));

    private static SoundEvent Register(ResourceLocation location) => 
        Register(location, location);

    private static SoundEvent Register(ResourceLocation location, ResourceLocation location2) => 
        Registry.Register(BuiltinRegistries.SoundEvent, location, SoundEvent.CreateVariableRangeEvent(location2));
    
    private static IReferenceHolder<SoundEvent> RegisterForHolder(string str) => 
        RegisterForHolder(new ResourceLocation(str));

    private static IReferenceHolder<SoundEvent> RegisterForHolder(ResourceLocation location) => 
        RegisterForHolder(location, location);

    private static IReferenceHolder<SoundEvent> RegisterForHolder(ResourceLocation location, ResourceLocation location2) => 
        Registry.RegisterForHolder(BuiltinRegistries.SoundEvent, location, SoundEvent.CreateVariableRangeEvent(location2));

    private static List<IReferenceHolder<SoundEvent>> RegisterGoatHornSoundVariants()
    {
        return Enumerable.Range(0, GoatHornVariantCount)
            .Select(i => RegisterForHolder($"item.goat_horn.sound.{i}"))
            .ToList();
    }
}