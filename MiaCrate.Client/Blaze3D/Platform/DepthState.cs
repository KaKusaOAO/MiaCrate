﻿using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public class DepthState
{
    public BoolState State { get; } = new(EnableCap.DepthTest);
    public bool IsMask { get; set; } = true;
    public DepthFunction DepthFunction { get; set; } = DepthFunction.Less;
}