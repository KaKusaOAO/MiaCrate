namespace MiaCrate.Client.Oshi;

public record PhysicalProcessor(int PhysicalPackageNumber, int PhysicalProcessorNumber, int Efficiency = 0, string IdString = "");