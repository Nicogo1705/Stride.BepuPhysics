﻿using BepuPhysics;
using BepuPhysics.Collidables;
using Stride.BepuPhysics.Configurations;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Games;

namespace Stride.BepuPhysics.Definitions.Colliders
{
    [DataContract]
    public sealed class BoxCollider : ColliderBase
    {
        private Vector3 _size = new(1, 1, 1);

        public Vector3 Size
        {
            get => _size;
            set
            {
                _size = value;
                Container?.ContainerData?.TryUpdateContainer();
            }
        }

        internal override void AddToCompoundBuilder(IGame game, BepuSimulation simulation, ref CompoundBuilder builder, RigidPose localPose)
        {
            builder.Add(new Box(Size.X, Size.Y, Size.Z), localPose, Mass);
        }
    }
}
