using Stride.BepuPhysics.Navigation.Processors;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.BepuPhysics.Navigation.Components;
[DefaultEntityComponentProcessor(typeof(DotRecastAgentProcessor), ExecutionMode = ExecutionMode.Runtime)]
[ComponentCategory("Bepu - Navigation")]
[DataContract("DotRecastAgent", Inherited = true)]
public class DotRecastAgent : EntityComponent
{
	/// <summary>
	/// True if a new path needs to be calculated, can be manually changed to force a new path to be calculated.
	/// </summary>
	[DataMemberIgnore]
	public bool ShouldMove { get; set; } = true;
	/// <summary>
	/// The target position for the agent to move to. will trigger IsDirty to be set to true.
	/// </summary>
	[DataMemberIgnore]
	public Vector3 Target 
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}
	[DataMemberIgnore]
	public List<Vector3> Path = new();
	[DataMemberIgnore]
	public List<long> Polys = new();

	private Vector3 _target;
}
