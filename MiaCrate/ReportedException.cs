namespace MiaCrate;

public class ReportedException : Exception
{
    public CrashReport Report { get; }

    public ReportedException(CrashReport report) : base(report.Title, report.Exception)
    {
        Report = report;
    }
}