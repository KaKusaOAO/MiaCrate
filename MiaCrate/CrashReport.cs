using System.Text;

namespace MiaCrate;

public class CrashReport
{
    public string Title { get; }
    public Exception Exception { get; }

    public CrashReport(string title, Exception exception)
    {
        Title = title;
        Exception = exception;
    }

    private static string GetErrorComment()
    {
        var strings = new[]
        {
            "Who set us up the TNT?", "Everything's going to plan. No, really, that was supposed to happen.",
            "Uh... Did I do that?", "Oops.", "Why did you do that?", "I feel sad now :(", "My bad.", "I'm sorry, Dave.",
            "I let you down. Sorry :(", "On the bright side, I bought you a teddy bear!", "Daisy, daisy...",
            "Oh - I know what I did wrong!", "Hey, that tickles! Hehehe!", "I blame Dinnerbone.",
            "You should try our sister game, Minceraft!", "Don't be sad. I'll do better next time, I promise!",
            "Don't be sad, have a hug! <3", "I just don't know what went wrong :(", "Shall we play a game?",
            "Quite honestly, I wouldn't worry myself about that.", "I bet Cylons wouldn't have this problem.",
            "Sorry :(", "Surprise! Haha. Well, this is awkward.", "Would you like a cupcake?",
            $"Hi. I'm {MiaCore.ProductName}, and I'm a crashaholic.", "Ooh. Shiny.", "This doesn't make any sense!",
            "Why is it breaking :(", "Don't do that.", "Ouch. That hurt :(", "You're mean.",
            "This is a token for 1 free hug. Redeem at your nearest Mojangsta: [~~HUG~~]", "There are four lights!",
            "But it works on my machine."
        };

        try
        {
            return strings[Util.GetNanos() % strings.Length];
        }
        catch
        {
            return "Witty comment unavailable :(";
        }
    }

    public string GetExceptionMessage()
    {
        var ex = Exception;
        if (ex.Message == null)
        {
            if (ex is NullReferenceException) ex = new MessageOverridenException(Title, ex);
            else if (ex is StackOverflowException) ex = new MessageOverridenException(Title, ex);
            else if (ex is OutOfMemoryException) ex = new MessageOverridenException(Title, ex);
        }

        var stackTrace = ex.StackTrace;
        var exType = ex.GetType();
        if (ex is MessageOverridenException wrapped)
        {
            exType = wrapped.InnerException!.GetType();
            stackTrace = wrapped.InnerException!.StackTrace;
        }

        var sb = new StringBuilder();
        sb.Append(exType.FullName);
        sb.Append(": ");
        sb.AppendLine(ex.Message);
        sb.AppendLine(stackTrace);
        return sb.ToString();
    }

    public string GetFriendlyReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---- {MiaCore.ProductName} Crash Report ----");
        sb.Append("// ");
        sb.AppendLine(GetErrorComment());
        sb.AppendLine();
        sb.Append("Time: ");
        sb.AppendLine(DateTimeOffset.Now.ToString("R"));
        sb.Append("Description: ");
        sb.AppendLine(Title);
        sb.AppendLine();
        sb.AppendLine(GetExceptionMessage());
        sb.AppendLine();
        sb.AppendLine("A detailed walkthrough of the error, its code path and all known details is as follows:");

        for (var i = 0; i < 87; i++)
            sb.Append('-');
        
        sb.AppendLine();
        sb.AppendLine();
        WriteDetails(sb);
        return sb.ToString();
    }

    public void WriteDetails(StringBuilder builder)
    {
        
    }
    
    public static void Preload()
    {
        MemoryReserve.Allocate();
        new CrashReport("Don't panic!", new Exception()).GetFriendlyReport();
    }

    public static CrashReport ForException(Exception ex, string title)
    {
        if (ex is AggregateException aggregateException)
        {
            ex = aggregateException.InnerExceptions.First();
        }

        if (ex is ReportedException reported)
        {
            return reported.Report;
        }

        return new CrashReport(title, ex);
    }
}