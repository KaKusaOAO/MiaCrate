using Mochi.Texts;

namespace MiaCrate.Net;

public record PlayerChatMessage(SignedMessageLink Link, MessageSignature? Signature, SignedMessageBody SignedBody,
    IComponent UnsignedContent, FilterMask FilterMask);