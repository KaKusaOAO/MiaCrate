namespace MiaCrate.Data;

public class DataFixerUpper : IDataFixer
{
    public static int GetLowestSchemaSameVersion(SortedDictionary<int, Schema> schemas, int versionKey)
    {
        if (versionKey < schemas.Keys.First()) return schemas.Keys.First();
        return schemas.Keys.TakeWhile(i => i < versionKey + 1).Last();
    }
}