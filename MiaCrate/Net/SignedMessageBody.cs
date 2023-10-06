namespace MiaCrate.Net;

public record SignedMessageBody(string Content, DateTimeOffset Timestamp, long Salt, LastSeenMessages LastSeen);