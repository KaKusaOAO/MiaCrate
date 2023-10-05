namespace MiaCrate.World;

public interface IStructureAccess
{
    public StructureStart? GetStartForStructure(Structure structure);

    public void SetStartForStructure(Structure structure, StructureStart start);
}