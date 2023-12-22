using DotRecast.Core.Numerics;
using Stride.Core.Mathematics;
using System.Runtime.CompilerServices;

namespace Stride.BepuPhysics.Navigation.Extensions;
public static class StrideDotRecastExtensions
{
	public static RcVec3f ToDotRecastVector(this Vector3 vec)
	{
		//return Unsafe.As<RcVec3f, Vector3>(ref vec);
		return new RcVec3f(vec.X, vec.Y, vec.Z);
	}
}
