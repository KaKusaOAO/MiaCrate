using MiaCrate.Client.Models;
using MiaCrate.Client.Models.Json;
using MiaCrate.World.Items;

namespace MiaCrate.Client.Graphics;

public class ItemTransforms
{
    public static readonly ItemTransforms NoTransforms = new();
    
    public ItemTransform ThirdPersonLeftHand { get; }
    public ItemTransform ThirdPersonRightHand { get; }
    public ItemTransform FirstPersonLeftHand { get; }
    public ItemTransform FirstPersonRightHand { get; }
    public ItemTransform Head { get; }
    public ItemTransform Gui { get; }
    public ItemTransform Ground { get; }
    public ItemTransform Fixed { get; }

    private ItemTransforms()
        : this(
            ItemTransform.NoTransform, ItemTransform.NoTransform, 
            ItemTransform.NoTransform, ItemTransform.NoTransform,
            ItemTransform.NoTransform, ItemTransform.NoTransform, 
            ItemTransform.NoTransform, ItemTransform.NoTransform)
    {
        
    }
    
    public ItemTransforms(ItemTransform thirdPersonLeftHand, ItemTransform thirdPersonRightHand, ItemTransform firstPersonLeftHand, ItemTransform firstPersonRightHand, ItemTransform head, ItemTransform gui, ItemTransform ground, ItemTransform @fixed)
    {
        ThirdPersonLeftHand = thirdPersonLeftHand;
        ThirdPersonRightHand = thirdPersonRightHand;
        FirstPersonLeftHand = firstPersonLeftHand;
        FirstPersonRightHand = firstPersonRightHand;
        Head = head;
        Gui = gui;
        Ground = ground;
        Fixed = @fixed;
    }

    public ItemTransforms(ItemTransforms other)
    {
        ThirdPersonLeftHand = other.ThirdPersonLeftHand;
        ThirdPersonRightHand = other.ThirdPersonRightHand;
        FirstPersonLeftHand = other.FirstPersonLeftHand;
        FirstPersonRightHand = other.FirstPersonRightHand;
        Head = other.Head;
        Gui = other.Gui;
        Ground = other.Ground;
        Fixed = other.Fixed;
    }

    internal ItemTransforms(JsonItemTransforms payload)
        : this(
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand),
            CreateTransformFromJson(payload.ThirdPersonLeftHand))
    {
        
    }

    private static ItemTransform CreateTransformFromJson(JsonItemTransform? payload) => 
        payload == null ? ItemTransform.NoTransform : new ItemTransform(payload);

    public ItemTransform GetTransform(ItemDisplayContext context)
    {
        if (context == ItemDisplayContext.ThirdPersonLeftHand)
            return ThirdPersonLeftHand;
        if (context == ItemDisplayContext.ThirdPersonRightHand)
            return ThirdPersonRightHand;
        if (context == ItemDisplayContext.FirstPersonLeftHand)
            return FirstPersonLeftHand;
        if (context == ItemDisplayContext.FirstPersonRightHand)
            return FirstPersonRightHand;
        if (context == ItemDisplayContext.Head)
            return Head;
        if (context == ItemDisplayContext.Gui)
            return Gui;
        if (context == ItemDisplayContext.Ground)
            return Ground;
        if (context == ItemDisplayContext.Fixed)
            return Fixed;

        return ItemTransform.NoTransform;
    }

    public bool HasTransform(ItemDisplayContext context) => GetTransform(context) != ItemTransform.NoTransform;
}