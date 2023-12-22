using DotRecast.Core.Numerics;
using DotRecast.Detour;
using Stride.BepuPhysics.Navigation.Components;
using Stride.BepuPhysics.Navigation.Extensions;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stride.BepuPhysics.Navigation.Processors;
public class DotRecastAgentProcessor : EntityProcessor<DotRecastAgent>
{
	public const int MaxPolys = 256;
	public const int MaxSmooth = 2048;

	private DtNavMesh _navMesh;
	private List<long> polys;
	private List<RcVec3f> smoothPath;
	private readonly RcVec3f polyPickExt = new RcVec3f(2, 4, 2);

	public void FindPath(Vector3 start, Vector3 end)
	{
		var queryFilter = new DtQueryDefaultFilter();
		DtNavMeshQuery query = new DtNavMeshQuery(_navMesh);

		query.FindNearestPoly(start.ToDotRecastVector(), polyPickExt, queryFilter, out var startRef, out var _, out var _);

		query.FindNearestPoly(end.ToDotRecastVector(), polyPickExt, queryFilter, out var endRef, out var _, out var _);
		// find the nearest point on the navmesh to the start and end points
		//query.FindPath(start, end, )
		FindFollowPath(_navMesh, query, startRef, endRef, start.ToDotRecastVector(), end.ToDotRecastVector(), queryFilter, true, ref polys, ref smoothPath);

	}

	public DtStatus FindFollowPath(DtNavMesh navMesh, DtNavMeshQuery navQuery, long startRef, long endRef, RcVec3f startPt, RcVec3f endPt, IDtQueryFilter filter, bool enableRaycast,
	ref List<long> polys, ref List<RcVec3f> smoothPath)
	{
		if (startRef == 0 || endRef == 0)
		{
			polys?.Clear();
			smoothPath?.Clear();

			return DtStatus.DT_FAILURE;
		}

		polys ??= new List<long>();
		smoothPath ??= new List<RcVec3f>();

		polys.Clear();
		smoothPath.Clear();

		var opt = new DtFindPathOption(enableRaycast ? DtFindPathOptions.DT_FINDPATH_ANY_ANGLE : 0, float.MaxValue);
		navQuery.FindPath(startRef, endRef, startPt, endPt, filter, ref polys, opt);
		if (0 >= polys.Count)
			return DtStatus.DT_FAILURE;

		// Iterate over the path to find smooth path on the detail mesh surface.
		navQuery.ClosestPointOnPoly(startRef, startPt, out var iterPos, out var _);
		navQuery.ClosestPointOnPoly(polys[polys.Count - 1], endPt, out var targetPos, out var _);

		float STEP_SIZE = 0.5f;
		float SLOP = 0.01f;

		smoothPath.Clear();
		smoothPath.Add(iterPos);
		var visited = new List<long>();

		// Move towards target a small advancement at a time until target reached or
		// when ran out of memory to store the path.
		while (0 < polys.Count && smoothPath.Count < MaxSmooth)
		{
			// Find location to steer towards.
			if (!DtPathUtils.GetSteerTarget(navQuery, iterPos, targetPos, SLOP,
					polys, out var steerPos, out var steerPosFlag, out var steerPosRef))
			{
				break;
			}

			bool endOfPath = (steerPosFlag & DtStraightPathFlags.DT_STRAIGHTPATH_END) != 0
				? true
				: false;
			bool offMeshConnection = (steerPosFlag & DtStraightPathFlags.DT_STRAIGHTPATH_OFFMESH_CONNECTION) != 0
				? true
				: false;

			// Find movement delta.
			RcVec3f delta = RcVec3f.Subtract(steerPos, iterPos);
			float len = MathF.Sqrt(RcVec3f.Dot(delta, delta));
			// If the steer target is end of path or off-mesh link, do not move past the location.
			if ((endOfPath || offMeshConnection) && len < STEP_SIZE)
			{
				len = 1;
			}
			else
			{
				len = STEP_SIZE / len;
			}

			RcVec3f moveTgt = RcVecUtils.Mad(iterPos, delta, len);

			// Move
			navQuery.MoveAlongSurface(polys[0], iterPos, moveTgt, filter, out var result, ref visited);

			iterPos = result;

			polys = DtPathUtils.MergeCorridorStartMoved(polys, visited);
			polys = DtPathUtils.FixupShortcuts(polys, navQuery);

			var status = navQuery.GetPolyHeight(polys[0], result, out var h);
			if (status.Succeeded())
			{
				iterPos.Y = h;
			}

			// Handle end of path and off-mesh links when close enough.
			if (endOfPath && DtPathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
			{
				// Reached end of path.
				iterPos = targetPos;
				if (smoothPath.Count < MaxSmooth)
				{
					smoothPath.Add(iterPos);
				}

				break;
			}
			else if (offMeshConnection && DtPathUtils.InRange(iterPos, steerPos, SLOP, 1.0f))
			{
				// Reached off-mesh connection.
				RcVec3f startPos = RcVec3f.Zero;
				RcVec3f endPos = RcVec3f.Zero;

				// Advance the path up to and over the off-mesh connection.
				long prevRef = 0;
				long polyRef = polys[0];
				int npos = 0;
				while (npos < polys.Count && polyRef != steerPosRef)
				{
					prevRef = polyRef;
					polyRef = polys[npos];
					npos++;
				}

				polys = polys.GetRange(npos, polys.Count - npos);

				// Handle the connection.
				var status2 = navMesh.GetOffMeshConnectionPolyEndPoints(prevRef, polyRef, ref startPos, ref endPos);
				if (status2.Succeeded())
				{
					if (smoothPath.Count < MaxSmooth)
					{
						smoothPath.Add(startPos);
						// Hack to make the dotted path not visible during off-mesh connection.
						if ((smoothPath.Count & 1) != 0)
						{
							smoothPath.Add(startPos);
						}
					}

					// Move position at the other side of the off-mesh link.
					iterPos = endPos;
					navQuery.GetPolyHeight(polys[0], iterPos, out var eh);
					iterPos.Y = eh;
				}
			}

			// Store results.
			if (smoothPath.Count < MaxSmooth)
			{
				smoothPath.Add(iterPos);
			}
		}

		return DtStatus.DT_SUCCSESS;
	}

}
