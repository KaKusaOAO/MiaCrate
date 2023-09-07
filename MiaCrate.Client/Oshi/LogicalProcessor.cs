namespace MiaCrate.Client.Oshi;

public record LogicalProcessor(int ProcessorNumber, int PhysicalProcessorNumber, int PhysicalPackageNumber,
    int NumaNode = 0, int ProcessorGroup = 0);