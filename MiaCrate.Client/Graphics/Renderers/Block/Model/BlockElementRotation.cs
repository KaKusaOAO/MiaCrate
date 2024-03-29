using MiaCrate.Client.Extensions;
using MiaCrate.Client.Models;
using MiaCrate.Client.Models.Json;
using MiaCrate.Core;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public record BlockElementRotation(Vector3 Origin, DirectionAxis Axis, float Angle, bool Rescale)
{
    internal BlockElementRotation(JsonBlockElementRotation payload)
        : this(
            payload.Origin.ToVector3(), 
            ValidateAxis(payload.Axis),
            ValidateAngle(payload.Angle),
            payload.Rescale) {}

    private static DirectionAxis ValidateAxis(string axis) => DirectionAxis.ByName(axis.ToLowerInvariant()) ??
                                                     throw new Exception($"Invalid rotation axis: {axis}");

    private static float ValidateAngle(float angle)
    {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (angle != 0 && Math.Abs(angle) != 22.5f && Math.Abs(angle) != 45)
            throw new Exception($"Invalid rotation {angle} found, only -45/-22.5/0/22.5/45 allowed");

        return angle;
    }
}