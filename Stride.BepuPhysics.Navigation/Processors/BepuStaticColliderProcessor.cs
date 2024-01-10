using SharpFont;
using Stride.BepuPhysics.Components.Containers;
using Stride.BepuPhysics.Definitions;
using Stride.BepuPhysics.Navigation.Definitions;
using Stride.BepuPhysics.Processors;
using Stride.Core.Annotations;
using Stride.Core.Collections;
using Stride.Core.Mathematics;
using Stride.Engine;
using System.Collections.Generic;
using static BepuPhysics.Collidables.CompoundBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stride.BepuPhysics.Navigation.Processors;
public class BepuStaticColliderProcessor : EntityProcessor<StaticContainerComponent>
{
	public delegate void CollectionChangedEventHandler(StaticContainerComponent component);

	public event CollectionChangedEventHandler? ColliderAdded;
	public event CollectionChangedEventHandler? ColliderRemoved;

	/// <summary>
	/// This is done based on the assumption that storing the data is cheaper than generating it from Bepu.
	/// More testing is needed to confirm this.
	/// </summary>
	public FastList<BodyShapeData> BodyShapes = new();
	public Dictionary<ContainerComponent, NavigationColliderObject[]> Colliders = new();

	private SceneSystem? _sceneSystem;
	private EntityProcessor? _entityProcessor;
	private BepuShapeCacheSystem? _bepuShapeCacheSystem;

	public BepuStaticColliderProcessor()
	{
		// this is done to ensure that this processor runs after the BepuPhysicsProcessors
		Order = 20001;
	}

	protected override void OnSystemAdd()
	{
		base.OnSystemAdd();

		_sceneSystem = Services.GetService<SceneSystem>();
		_entityProcessor = _sceneSystem.SceneInstance.GetProcessor<EntityProcessor>();
		_bepuShapeCacheSystem = Services.GetService<BepuShapeCacheSystem>();

		foreach(var entity in _entityProcessor.EntityManager)
		{
			var container = entity.Get<StaticContainerComponent>();
			if (container != null)
			{
				AddColliders(entity, container);
			}
		}
	}

	protected override StaticContainerComponent GenerateComponentData(Entity entity, StaticContainerComponent component)
	{
		return base.GenerateComponentData(entity, component);
	}

	protected override bool IsAssociatedDataValid(Entity entity, StaticContainerComponent component, StaticContainerComponent associatedData)
	{
		//// need to check for both StaticColliderComponent and StaticMeshContainerComponent
		//if((StaticMeshContainerComponent)component is not null)
		//{
		//	return true;
		//}

		return component is not null;
	}

	protected override void OnEntityComponentAdding(Entity entity, [NotNull] StaticContainerComponent component, [NotNull] StaticContainerComponent data)
	{
		if(!Colliders.ContainsKey(data))
		{
			AddColliders(entity, data);
		}
	}

	protected override void OnEntityComponentRemoved(Entity entity, [NotNull] StaticContainerComponent component, [NotNull] StaticContainerComponent data)
	{
		Colliders.Remove(data);
		ColliderRemoved?.Invoke(data);
	}

	private void AddColliders(Entity entity, ContainerComponent container)
	{
		List<(BodyShapeData data, BodyShapeTransform transform)> shapes = new();
		_bepuShapeCacheSystem.AppendCachedShapesFor(container, shapes);
		NavigationColliderObject[] colliders = new NavigationColliderObject[shapes.Count];
		entity.Transform.UpdateWorldMatrix();

		container.Entity.Transform.WorldMatrix.Decompose(out var scale, out Matrix rotMatrix, out var transformPos);
		var transMatrix = Matrix.Translation(transformPos);

		for (int i = 0; i < shapes.Count; i++)
		{
			var shapeAndOffset = shapes[i];
			var local = shapeAndOffset;

			Matrix.Transformation(ref local.transform.Scale, ref local.transform.RotationLocal, ref local.transform.PositionLocal, out var containerMatrix);

			var collider = new NavigationColliderObject(shapes[i].data.Vertices, shapes[i].data.Indices);
			collider.ContainerBaseMatrix = containerMatrix;

			collider.WorldMatrix = collider.ContainerBaseMatrix;
			collider.WorldMatrix *= rotMatrix;
			collider.WorldMatrix *= transMatrix;
			//collider.WorldMatrix.ScaleVector = scale;

			collider.WorldMatrix.Decompose(out var finalScale, out Quaternion finalRot, out var finalTransform);

			for(int j = 0; j < collider.Vertices.Length; j++)
			{
				collider.Vertices[j].Position = Vector3.Transform(collider.Vertices[j].Position, finalRot);
				collider.Vertices[j].Position += collider.WorldMatrix.TranslationVector;
			}

			colliders[i] = collider;
		}

		Colliders.Add(container, colliders);
	}

}
