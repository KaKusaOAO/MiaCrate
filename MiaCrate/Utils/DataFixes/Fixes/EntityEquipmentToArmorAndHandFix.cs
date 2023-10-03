using MiaCrate.Data;

namespace MiaCrate.DataFixes;

public class EntityEquipmentToArmorAndHandFix : DataFix
{
    public EntityEquipmentToArmorAndHandFix(Schema outputSchema, bool changesType) 
        : base(outputSchema, changesType)
    {
    }
}