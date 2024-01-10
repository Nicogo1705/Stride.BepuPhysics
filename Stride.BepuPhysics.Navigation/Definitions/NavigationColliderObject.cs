using Stride.BepuPhysics.Definitions;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Buffer = Stride.Graphics.Buffer;

namespace Stride.BepuPhysics.Navigation.Definitions;
public class NavigationColliderObject
{
	public Matrix WorldMatrix = Matrix.Identity;
	public Matrix ContainerBaseMatrix = Matrix.Identity;

	public readonly VertexPosition3[] Vertices;
	public readonly int[] Indices;

	public NavigationColliderObject(VertexPosition3[] vertices, int[] indices)
	{
		Vertices = vertices;
		Indices = indices;
	}
}
