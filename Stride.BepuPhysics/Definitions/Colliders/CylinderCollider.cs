﻿using BepuPhysics;
using BepuPhysics.Collidables;
using Stride.BepuPhysics.Configurations;
using Stride.Core;
using Stride.Games;

namespace Stride.BepuPhysics.Definitions.Colliders
{
    [DataContract]
    public sealed class CylinderCollider : ColliderBase
    {
        private float _radius = 0.5f;
        private float _length = 1f;

        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                Container?.ContainerData?.TryUpdateContainer();
            }
        }

        public float Length
        {
            get => _length;
            set
            {
                _length = value;
                Container?.ContainerData?.TryUpdateContainer();
            }
        }

        internal override void AddToCompoundBuilder(IGame game, BepuSimulation simulation, ref CompoundBuilder builder, RigidPose localPose)
        {
            builder.Add(new Cylinder(Radius, Length), localPose, Mass);
        }
    }
}
