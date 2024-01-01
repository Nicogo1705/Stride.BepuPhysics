using Stride.BepuPhysics.Navigation.Components;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Design;

namespace Stride.BepuPhysics.Demo.Components.Navigation;
[DataContract("NavAgent")]
[DefaultEntityComponentProcessor(typeof(AgentPathfindingProcessor), ExecutionMode = ExecutionMode.Runtime)]
public class NavAgent : DotRecastAgent
{
	public int PathPointIndex { get; set; }
	public Entity TestTarget { get; set; }
	[DataMemberIgnore]
	public bool HasPath { get; set; }
}
