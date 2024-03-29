﻿using System.Numerics;
using System.Runtime.CompilerServices;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.Trees;

namespace Stride.BepuPhysics.Definitions.Raycast;

internal struct RayClosestHitHandler : IRayHitHandler, ISweepHitHandler
{
    private readonly BepuSimulation _sim;

    public CollisionMask CollisionMask { get; set; }
    public HitInfo? HitInformation { get; set; }

    public RayClosestHitHandler(BepuSimulation sim, CollisionMask collisionMask)
    {
        CollisionMask = collisionMask;
        _sim = sim;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable) => CollisionMask.AllowTest(collidable, _sim);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTest(CollidableReference collidable, int childIndex)
    {
        return true;
    }

    public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
    {
        HitInformation = new(ray.Origin + ray.Direction * t, normal, t, _sim.GetContainer(collidable));
        maximumT = t;
    }

    public void OnHit(ref float maximumT, float t, Vector3 hitLocation, Vector3 hitNormal, CollidableReference collidable)
    {
        HitInformation = new(hitLocation, hitNormal, t, _sim.GetContainer(collidable));
        maximumT = t;
    }

    public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
    {
        // Right now just ignore the hit;
        // We can't just set info to invalid data, it'll be confusing for users,
        // but we might need to find a way to notify that the shape at its resting pose is already intersecting.
    }
}