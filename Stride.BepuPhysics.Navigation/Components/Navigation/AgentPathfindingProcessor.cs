using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;

namespace Stride.BepuPhysics.Demo.Components.Navigation;
public class AgentPathfindingProcessor : EntityProcessor<NavAgent>
{
	public override void Update(GameTime time)
	{
		foreach (var agent in ComponentDatas.Values)
		{
			if (agent.Path.Count == 0)
			{
				agent.Target = agent.TestTarget.Transform.WorldMatrix.TranslationVector;
				agent.ShouldMove = true;
				continue;
			}

			var deltaTime = (float)time.Elapsed.TotalSeconds;
			var curPosition = agent.Entity.Transform.WorldMatrix.TranslationVector;
			var nextWaypointPosition = agent.Path[agent.PathPointIndex];
			var distanceToWaypoint = Vector3.Distance(curPosition, nextWaypointPosition);

			// When the distance between the character and the next waypoint is large enough, move closer to the waypoint
			if (distanceToWaypoint > 0.1)
			{
				var direction = nextWaypointPosition - curPosition;
				direction.Normalize();
				direction *= 5 * deltaTime;

				agent.Entity.Transform.Position += direction;
			}
			else
			{
				// If we are close enough to the waypoint, set the next waypoint or we are done and we do a final cleanup 
				if (agent.PathPointIndex + 1 < agent.Path.Count)
				{
					agent.PathPointIndex++;
				}
				else
				{
					//ClearGoal();
				}
			}
		}
	}
}
