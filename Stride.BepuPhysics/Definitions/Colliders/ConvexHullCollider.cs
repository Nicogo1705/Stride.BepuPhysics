﻿using System.Runtime.InteropServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Stride.BepuPhysics.Systems;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Physics;
using NRigidPose = BepuPhysics.RigidPose;

namespace Stride.BepuPhysics.Definitions.Colliders;

[DataContract]
public sealed class ConvexHullCollider : ColliderBase
{
    private Vector3 _scale = new(1, 1, 1);
    public PhysicsColliderShape? Hull { get; set; }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Container?.TryUpdateContainer();
        }
    }

    internal override void AddToCompoundBuilder(ShapeCacheSystem shape, BufferPool pool, ref CompoundBuilder builder, NRigidPose localPose)
    {
#warning maybe don't rely on cache actually, instead cache the convexhull struct itself ? See if that can be reused
        var data = shape.BorrowHull(this);
        var points = MemoryMarshal.Cast<VertexPosition3, System.Numerics.Vector3>(data.Vertices);

        if (_scale != Vector3.One) // Bepu doesn't support scaling on the collider itself, we have to create a temporary array and scale the points before passing it on
        {
            var copy = points.ToArray();
            var scaleAsNumerics = _scale.ToNumericVector();
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] *= scaleAsNumerics;
            }

            points = copy;
        }

        builder.Add(new ConvexHull(points, pool, out _), localPose, Mass);
    }
}