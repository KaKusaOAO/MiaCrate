﻿using OpenTK.Graphics.OpenGL;

namespace MiaCrate.Client.Platform;

public class PolygonOffsetState
{
    public BoolState FillState { get; } = new(EnableCap.PolygonOffsetFill);
    public BoolState LineState { get; } = new(EnableCap.PolygonOffsetLine);
    public float Factor { get; set; }
    public float Units { get; set; }
}