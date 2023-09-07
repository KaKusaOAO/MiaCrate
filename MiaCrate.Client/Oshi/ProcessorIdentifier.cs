using System.Text;
using System.Text.RegularExpressions;
using MiaCrate.Client.Utils;

namespace MiaCrate.Client.Oshi;

public class ProcessorIdentifier
{
    public ProcessorIdentifier(string vendor, string name, string family, string model, string stepping, string processorId, bool is64Bit, long vendorFreq = -1L)
    {
        Vendor = vendor;
        Name = name;
        Family = family;
        Model = model;
        Stepping = stepping;
        ProcessorId = processorId;
        Is64Bit = is64Bit;

        var sb = new StringBuilder();
        if (vendor == "GenuineIntel")
        {
            sb.Append(is64Bit ? "Intel64" : "x86");
        }
        else
        {
            sb.Append(vendor);
        }

        sb.Append(" Family ").Append(family);
        sb.Append(" Model ").Append(model);
        sb.Append(" Stepping ").Append(stepping);
        Identifier = sb.ToString();

        if (vendorFreq > 0)
        {
            VendorFreq = vendorFreq;
        }
        else
        {
            var matches = Regex.Matches(name, "@ (.*)$");
            if (matches.Any())
            {
                var match = matches.First();
                var unit = match.Groups[1].Value;
                VendorFreq = ParseUtils.ParseHertz(unit);
            }
            else
            {
                VendorFreq = -1;
            }
        }
    }

    public string Vendor { get; init; }
    public string Name { get; init; }
    public string Family { get; init; }
    public string Model { get; init; }
    public string Stepping { get; init; }
    public string ProcessorId { get; init; }
    public bool Is64Bit { get; init; }
    public long VendorFreq { get; init; }
    public string Identifier { get; }

    public void Deconstruct(out string vendor, out string name, out string family, out string model, out string stepping, out string processorId, out bool is64Bit, out long vendorFreq)
    {
        vendor = Vendor;
        name = Name;
        family = Family;
        model = Model;
        stepping = Stepping;
        processorId = ProcessorId;
        is64Bit = Is64Bit;
        vendorFreq = VendorFreq;
    }
}