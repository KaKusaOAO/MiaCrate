using Veldrid;

namespace MiaCrate.Client.Graphics;

public static class DefaultVertexFormat
{
    public static readonly VertexFormatElement ElementPosition =
        new(0, VertexFormatElement.TypeInfo.Float, VertexFormatElement.UsageInfo.Position, 3,
            VertexElementFormat.Float3, VertexElementSemantic.Position);

    public static readonly VertexFormatElement ElementColor =
        new(0, VertexFormatElement.TypeInfo.UByte, VertexFormatElement.UsageInfo.Color, 4,
            VertexElementFormat.Byte4_Norm, VertexElementSemantic.Color);

    public static readonly VertexFormatElement ElementUv0 =
        new(0, VertexFormatElement.TypeInfo.Float, VertexFormatElement.UsageInfo.Uv, 2,
            VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate);

    public static readonly VertexFormatElement ElementUv1 =
        new(1, VertexFormatElement.TypeInfo.Short, VertexFormatElement.UsageInfo.Uv, 2,
            VertexElementFormat.Short2, VertexElementSemantic.TextureCoordinate);

    public static readonly VertexFormatElement ElementUv2 =
        new(2, VertexFormatElement.TypeInfo.Short, VertexFormatElement.UsageInfo.Uv, 2,
            VertexElementFormat.Short2, VertexElementSemantic.TextureCoordinate);

    public static readonly VertexFormatElement ElementNormal =
        new(0, VertexFormatElement.TypeInfo.Byte, VertexFormatElement.UsageInfo.Normal, 3,
            VertexElementFormat.SByte2_Norm, VertexElementSemantic.Normal);

    public static readonly VertexFormatElement ElementPadding =
        new(0, VertexFormatElement.TypeInfo.Byte, VertexFormatElement.UsageInfo.Padding, 1,
            VertexElementFormat.Byte2, VertexElementSemantic.Color);

    // Alias
    public static readonly VertexFormatElement ElementUv = ElementUv0;

    public static readonly VertexFormat BlitScreen = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV"] = ElementUv,
            ["Color"] = ElementColor
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV", ElementUv.ElementFormat, ElementUv.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic)
        )
    );

    public static readonly VertexFormat Block = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["UV0"] = ElementUv0,
            ["UV2"] = ElementUv2,
            ["Normal"] = ElementNormal,
            ["Padding"] = ElementPadding
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic),
            new VertexElementDescription("Normal", ElementNormal.ElementFormat, ElementNormal.Semantic)
        )
    );

    public static readonly VertexFormat NewEntity = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["UV0"] = ElementUv0,
            ["UV1"] = ElementUv1,
            ["UV2"] = ElementUv2,
            ["Normal"] = ElementNormal,
            ["Padding"] = ElementPadding
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("UV1", ElementUv1.ElementFormat, ElementUv1.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic),
            new VertexElementDescription("Normal", ElementNormal.ElementFormat, ElementNormal.Semantic)
        )
    );

    public static readonly VertexFormat Particle = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV0"] = ElementUv0,
            ["Color"] = ElementColor,
            ["UV2"] = ElementUv2
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic)
        )
    );

    public static readonly VertexFormat Position = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic)
        )
    );

    public static readonly VertexFormat PositionColor = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic)
        )
    );

    public static readonly VertexFormat PositionColorNormal = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["Normal"] = ElementNormal,
            ["Padding"] = ElementPadding
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("Normal", ElementNormal.ElementFormat, ElementNormal.Semantic)
        )
    );

    public static readonly VertexFormat PositionColorLightmap = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["UV2"] = ElementUv2
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic)
        )
    );

    public static readonly VertexFormat PositionTex = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV0"] = ElementUv0,
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic)
        )
    );

    public static readonly VertexFormat PositionColorTex = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["UV0"] = ElementUv0,
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic)
        )
    );

    public static readonly VertexFormat PositionTexColor = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV0"] = ElementUv0,
            ["Color"] = ElementColor,
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic)
        )
    );

    public static readonly VertexFormat PositionColorTexLightmap = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["Color"] = ElementColor,
            ["UV0"] = ElementUv0,
            ["UV2"] = ElementUv2
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic)
        )
    );

    public static readonly VertexFormat PositionTexLightmapColor = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV0"] = ElementUv0,
            ["UV2"] = ElementUv2,
            ["Color"] = ElementColor,
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("UV2", ElementUv2.ElementFormat, ElementUv2.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic)
        )
    );

    public static readonly VertexFormat PositionTexColorNormal = new(new Dictionary<string, VertexFormatElement>
        {
            ["Position"] = ElementPosition,
            ["UV0"] = ElementUv0,
            ["Color"] = ElementColor,
            ["Normal"] = ElementNormal,
            ["Padding"] = ElementPadding
        }, () => new VertexLayoutDescription(
            new VertexElementDescription("Position", ElementPosition.ElementFormat, ElementPosition.Semantic),
            new VertexElementDescription("UV0", ElementUv0.ElementFormat, ElementUv0.Semantic),
            new VertexElementDescription("Color", ElementColor.ElementFormat, ElementColor.Semantic),
            new VertexElementDescription("Normal", ElementNormal.ElementFormat, ElementNormal.Semantic)
        )
    );
}