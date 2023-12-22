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
[DataContract("DotRecastAgent")]
public class DotRecastAgent : EntityComponent
{
	public List<Vector3> Path { get; set; } = new List<Vector3>();
	public List<long> Polys { get; set; } = new List<long>();
}
