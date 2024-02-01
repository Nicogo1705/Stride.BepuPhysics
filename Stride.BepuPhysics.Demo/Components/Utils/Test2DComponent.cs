using Stride.BepuPhysics.Definitions.Colliders;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Materials;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.ProceduralModels;
using System;
using System.Collections.Generic;

namespace Stride.BepuPhysics.Demo.Components.Utils;

public class Test2DComponent : SyncScript
{
    private const float Depth = 1;
    private const float MaxDistance = 100;
    private const string ShapeName = "BepuCube";
    private const int SimulationIndex = 2;
    private BepuConfiguration? _bepuConfig;
    private CameraComponent? _camera;
    private int _debugX = 5;
    private int _debugY = 300;
    private int _spawnCount = 10;
    private Vector3 _boxSize = new(0.2f, 0.2f, Depth);
    private Vector3 _rectangleSize = new(0.2f, 0.3f, Depth);
    private List<Shape2DModel> _shapes = [];

    public override void Start()
    {
        _bepuConfig = Game.Services.GetService<BepuConfiguration>();
        _camera = Entity.Get<CameraComponent>();
        _shapes = [
            new() { Type = Primitive2DModelType.Square, Color = Color.Green, Size = (Vector2)_boxSize },
            new() { Type = Primitive2DModelType.Rectangle, Color = Color.Orange, Size = (Vector2)_rectangleSize },
            new() { Type = Primitive2DModelType.Circle, Color = Color.Red, Size = (Vector2)_boxSize / 2 },
            new() { Type = Primitive2DModelType.Triangle, Color = Color.Purple, Size = (Vector2)_boxSize }
        ];
    }

    public override void Update()
    {
        if (!Input.HasKeyboard) return;

        if (Input.IsMouseButtonDown(MouseButton.Left))
        {
            ProcessRaycast(MouseButton.Left, Input.MousePosition);
        }

        if (Input.IsKeyDown(Keys.J))
        {
            Add2DShapes(Primitive2DModelType.Square, _spawnCount);

            //SetCubeCount(scene);
        }
        else if (Input.IsKeyDown(Keys.H))
        {
            Add2DShapes(Primitive2DModelType.Rectangle, _spawnCount);

            //SetCubeCount(scene);
        }
        else if (Input.IsKeyDown(Keys.C))
        {
            Add2DShapes(Primitive2DModelType.Circle, _spawnCount);

            //SetCubeCount(scene);
        }
        else if (Input.IsKeyDown(Keys.Y))
        {
            Add2DShapes(Primitive2DModelType.Triangle, _spawnCount);

            //SetCubeCount(scene);
        }
        else if (Input.IsKeyDown(Keys.I))
        {
            Add2DShapes(count: 30);

            //SetCubeCount(scene);
        }
        //else if (Input.IsKeyReleased(Keys.X))
        //{
        //    foreach (var entity in Entity.Scene.Entities.SelectMany(s => s.GetChildren()).Where(w => w.Name == ShapeName).ToList())
        //    {
        //        entity.Scene = null;
        //    }

        //    foreach (var entity in Entity.Scene.Entities.Where(w => w.Name == ShapeName).ToList())
        //    {
        //        entity.Scene = null;
        //    }
        //}

        RenderNavigation();

    }

    void RenderNavigation()
    {
        var space = 0;
        space += 20;
        DebugText.Print($"J - Generate 2D squares", new Int2(x: _debugX, y: _debugY + space));
        space += 20;
        DebugText.Print($"H - Generate 2D rectangles", new Int2(x: _debugX, y: _debugY + space));
        space += 20;
        DebugText.Print($"C - Generate 2D circles", new Int2(x: _debugX, y: _debugY + space));
        space += 20;
        DebugText.Print($"Y - Generate 2D triangles", new Int2(x: _debugX, y: _debugY + space));
        space += 20;
        DebugText.Print($"I - Generate random 2D shapes", new Int2(x: _debugX, y: _debugY + space));
        space += 20;
        DebugText.Print($"Mouse click (optionally hold) - Add impulse", new Int2(x: _debugX, y: _debugY + space));
    }

    void ProcessRaycast(MouseButton mouseButton, Vector2 screenPosition)
    {
        if (_bepuConfig is null || _camera is null) return;

        if (mouseButton == MouseButton.Left)
        {
            var invertedMatrix = Matrix.Invert(_camera.ViewProjectionMatrix);

            Vector3 position;
            position.X = screenPosition.X * 2f - 1f;
            position.Y = 1f - screenPosition.Y * 2f;
            position.Z = 0f;

            var vectorNear = Vector3.Transform(position, invertedMatrix);
            vectorNear /= vectorNear.W;

            position.Z = 1f;

            var vectorFar = Vector3.Transform(position, invertedMatrix);
            vectorFar /= vectorFar.W;

            var buffer = System.Buffers.ArrayPool<HitInfo>.Shared.Rent(100);

            _bepuConfig.BepuSimulations[SimulationIndex].RaycastPenetrating(vectorNear.XYZ(), vectorFar.XYZ() - vectorNear.XYZ(), MaxDistance, buffer, out var hits);

            if (hits.Length > 0)
            {
                var space = 0;

                for (int j = 0; j < hits.Length; j++)
                {
                    var hitInfo = hits[j];

                    if (hitInfo.Container.Entity.Name == ShapeName)
                    {
                        DebugText.Print($"Hit! Distance : {hitInfo.Distance}  |  normal : {hitInfo.Normal}  |  Entity : {hitInfo.Container.Entity}", new Int2(x: _debugX, y: 450 + space));

                        space += 20;

                        var rigidBody = hitInfo.Container.Entity.Get<Body2DComponent>();

                        if (rigidBody == null) continue;

                        var direction = new Vector3(0, 20, 0);

                        rigidBody.ApplyImpulse(direction * 10, new());
                        rigidBody.LinearVelocity = direction * 1;
                    }
                }
            }
            else
            {
                DebugText.Print("No raycast hit", new Int2(x: _debugX, y: 450));
            }
        }
    }

    void Add2DShapes(Primitive2DModelType? type = null, int count = 5)
    {
        //var entity = new Entity();

        for (int i = 1; i <= count; i++)
        {
            var shapeModel = GetShape(type);

            if (shapeModel == null) return;

            var entity = Create2DPrimitiveWithBepu(shapeModel.Type,
                new()
                {
                    Size = shapeModel.Size,
                    Material = CreateMaterial(Game, shapeModel.Color)
                });

            //if (type == null || i == 1)
            //{
            //    entity = game.Create2DPrimitiveWithBepu(shapeModel.Type, new() { Size = shapeModel.Size, Material = game.CreateMaterial(shapeModel.Color) });
            //}
            //else
            //{
            //    entity = entity.Clone();
            //}

            entity.Name = ShapeName;
            entity.Transform.Position = GetRandomPosition();
            entity.Scene = Entity.Scene;
        }
    }

    private Shape2DModel? GetShape(Primitive2DModelType? type = null)
    {
        if (type == null)
        {
            int randomIndex = Random.Shared.Next(_shapes.Count);

            return _shapes[randomIndex];
        }

        return _shapes.Find(x => x.Type == type);
    }

    private Entity Create2DPrimitiveWithBepu(Primitive2DModelType type, Primitive2DCreationOptionsWithBepu? options = null)
    {
        options ??= new();

        options.Component.SimulationIndex = SimulationIndex;

        var modelBase = Build(type, options.Size, options.Depth);

        var model = modelBase.Generate(Game.Services);

        if (options.Material != null)
        {
            model.Materials.Add(options.Material);
        }

        var entity = new Entity(options.EntityName);

        if (type == Primitive2DModelType.Circle)
        {
            var childEntity = new Entity("Child") { new ModelComponent(model) { /*RenderGroup = options.RenderGroup*/ } };
            childEntity.Transform.Rotation = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90));
            entity.AddChild(childEntity);
        }
        else
        {
            entity.Add(new ModelComponent(model) { /*RenderGroup = options.RenderGroup*/ });
        }

        if (!options.IncludeCollider) return entity;

        if (type == Primitive2DModelType.Triangle)
        {
            // This is needed when using ConvexHullCollider
            //var meshData = TriangularPrismProceduralModel.New(options.Size is null ? new(1, 1, options.Depth) : new(options.Size.Value.X, options.Size.Value.Y, options.Depth));

            //var points = meshData.Vertices.Select(w => w.Position).ToList();
            //var uintIndices = meshData.Indices.Select(w => (uint)w).ToList();
            //var collider = new ConvexHullColliderShapeDesc()
            //{
            //    Model = model, // seems doing nothing
            //    ConvexHulls = [],
            //    ConvexHullsIndices = []
            //};

            //collider.ConvexHulls.Add([points]);
            //collider.ConvexHullsIndices.Add([uintIndices]);

            //List<IAssetColliderShapeDesc> descriptions = [];

            //descriptions.Add(collider);

            //var collider2 = new ConvexHullCollider() { Hull = new PhysicsColliderShape(descriptions) };

            //var compoundCollier = options.Component.Collider as CompoundCollider;

            //compoundCollier.Colliders.Add(collider2);

            // Or you can use just his for MeshCollider
            options.Component.Collider = new MeshCollider() { Model = model, Closed = true };

            entity.Add(options.Component);
        }
        else
        {
            var colliderShape = Get2DColliderShapeWithBepu(type, options.Size, options.Depth);

            if (colliderShape is null) return entity;

            var compoundCollier = options.Component.Collider as CompoundCollider;

            compoundCollier.Colliders.Add(colliderShape);

            entity.Add(options.Component);
        }

        return entity;
    }

    private static Vector3 GetRandomPosition() => new(Random.Shared.Next(-5, 5), Random.Shared.Next(10, 30), 0);

    private static PrimitiveProceduralModelBase Build(Primitive2DModelType type, Vector2? size = null, float depth = 0)
    => type switch
    {
        //Primitive2DModelType.Capsule => size is null ? new CapsuleProcedural2DModel() : new() { Radius = size.Value.X },
        Primitive2DModelType.Circle => new CylinderProceduralModel() { Radius = size is null ? 0.5f : size.Value.X, Height = depth },
        //Primitive2DModelType.Polygon => size is null ? new PolygonProceduralModel() : new() { Radius = size.Value.X, Sides = (int)size.Value.Y },
        //Primitive2DModelType.Quad => size is null ? new QuadProceduralModel() : new() { Size = size.Value.XY() },
        Primitive2DModelType.Rectangle => new CubeProceduralModel() { Size = size is null ? new(2, 1, depth) : new(size.Value.X, size.Value.Y, depth) },
        Primitive2DModelType.Square => new CubeProceduralModel() { Size = size is null ? new(1, 1, depth) : new(size.Value.X, size.Value.Y, depth) },
        //Primitive2DModelType.Square => size is null ? new SquareProceduralModel() : new() { Size = size.Value },
        Primitive2DModelType.Triangle => new TriangularPrismProceduralModel() { Size = size is null ? new(1, 1, depth) : new(size.Value.X, size.Value.Y, depth) },
        _ => throw new InvalidOperationException($"Unsupported Primitive2DModelType: {type}")
    };

    private static ColliderBase? Get2DColliderShapeWithBepu(Primitive2DModelType type, Vector2? size = null, float depth = 0)
    {
        return type switch
        {
            Primitive2DModelType.Rectangle => size is null ? new BoxCollider() : new() { Size = new(size.Value.X, size.Value.Y, depth) },
            Primitive2DModelType.Square => size is null ? new BoxCollider() : new() { Size = new(size.Value.X, size.Value.Y, depth) },
            Primitive2DModelType.Circle => size is null ? new CylinderCollider() : new()
            {
                Radius = size.Value.X,
                Length = depth,
                RotationLocal = Quaternion.RotationAxis(Vector3.UnitX, MathUtil.DegreesToRadians(90))
            },
            //Primitive2DModelType.Triangle => triangleCollider ?? new TriangleCollider(),
            _ => throw new InvalidOperationException(),
        };
    }

    public static Material CreateMaterial(IGame game, Color color, float specular = 1.0f, float microSurface = 0.65f)
    {
        var materialDescription = new MaterialDescriptor
        {
            Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature(),
                    Specular =  new MaterialMetalnessMapFeature(new ComputeFloat(specular)),
                    SpecularModel = new MaterialSpecularMicrofacetModelFeature(),
                    MicroSurface = new MaterialGlossinessMapFeature(new ComputeFloat(microSurface))
                }
        };

        return Material.New(game.GraphicsDevice, materialDescription);
    }
}

public enum Primitive2DModelType
{
    Capsule,
    Circle,
    Polygon,
    Quad,
    Rectangle,
    Square,
    Triangle
}

public class Primitive2DCreationOptionsWithBepu
{
    /// <summary>
    /// Gets or sets the name of the entity.
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Gets or sets the material to be applied to the primitive model.
    /// </summary>
    public Material? Material { get; set; }

    /// <summary>
    /// Determines whether to include a collider component in the entity. Defaults to true.
    /// </summary>
    public bool IncludeCollider { get; set; } = true;

    /// <summary>
    /// Gets or sets the size of the primitive model. If null, default dimensions are used.
    /// </summary>
    public Vector2? Size { get; set; }

    /// <summary>
    /// Gets or sets the render group for the entity. Defaults to RenderGroup.Group0.
    /// </summary>
    public RenderGroup RenderGroup { get; set; } = RenderGroup.Group0;

    /// <summary>
    /// Gets or sets the physics component to be added to the entity.
    /// </summary>
    public ContainerComponent Component { get; set; } = new Body2DComponent() { Collider = new CompoundCollider() };

    public float Depth { get; set; } = 1;
}

public class Shape2DModel
{
    public required Primitive2DModelType Type { get; set; }
    public required Color Color { get; set; }
    public required Vector2 Size { get; set; }
    public Model? Model { get; set; }
}

public class TriangularPrismProceduralModel : PrimitiveProceduralModelBase
{
    public Vector3 Size { get; set; } = Vector3.One;

    private static readonly Vector2[] TextureCoordinates = [new(1, 0), new(1, 1), new(0, 1), new(0, 0)];

    protected override GeometricMeshData<VertexPositionNormalTexture> CreatePrimitiveMeshData() => New(Size, UvScale.X, UvScale.Y);

    /// <summary>
    /// Creates a triangular prism.
    /// </summary>
    /// <param name="width">The width of the base triangle.</param>
    /// <param name="height">The height of the triangular face.</param>
    /// <param name="depth">The depth of the prism along the Z-axis.</param>
    /// <returns>A triangular prism.</returns>
    public static GeometricMeshData<VertexPositionNormalTexture> New(Vector3 size, float uScale = 1.0f, float vScale = 1.0f, bool toLeftHanded = false)
    {
        // There are 3 vertices for each of the 2 triangle faces (6 total), and 4 vertices for each of the 3 rectangle faces (12 total).
        var vertices = new VertexPositionNormalTexture[18];

        // There are 3 indices for each triangle face (6 total), and 6 indices (2 triangles) for each rectangle face (18 total).
        var indices = new int[24];

        var textureCoordinates = new Vector2[4];

        for (var i = 0; i < 4; i++)
        {
            textureCoordinates[i] = TextureCoordinates[i] * new Vector2(uScale, vScale);
        }

        var equilateralHeight = (float)Math.Sqrt(size.X * size.X - Math.Pow(size.X / 2, 2)) / 2;

        size /= 2.0f;

        // Calculate the height of the equilateral triangle
        //var apex = (float)equilateralHeight - size.Y;

        // Vertices for the two triangle faces
        // Triangle face 1 (front)
        vertices[0] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, size.Z), Vector3.UnitZ, textureCoordinates[0]);
        vertices[1] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, size.Z), Vector3.UnitZ, textureCoordinates[1]);
        vertices[2] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, size.Z), Vector3.UnitZ, textureCoordinates[2]);

        // Triangle face 2 (back)
        vertices[3] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, -size.Z), -Vector3.UnitZ, textureCoordinates[0]);
        vertices[4] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, -size.Z), -Vector3.UnitZ, textureCoordinates[1]);
        vertices[5] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, -size.Z), -Vector3.UnitZ, textureCoordinates[2]);


        // Vertices for the three rectangle faces
        // Rectangle face 1 (bottom)
        vertices[6] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, size.Z), -Vector3.UnitY, textureCoordinates[0]);
        vertices[7] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, -size.Z), -Vector3.UnitY, textureCoordinates[1]);
        vertices[8] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, -size.Z), -Vector3.UnitY, textureCoordinates[2]);
        vertices[9] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, size.Z), -Vector3.UnitY, textureCoordinates[3]);

        // Rectangle face 2 (left side)
        vertices[10] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, size.Z), -Vector3.UnitX, textureCoordinates[0]);
        vertices[11] = new VertexPositionNormalTexture(new Vector3(-size.X, -equilateralHeight, -size.Z), -Vector3.UnitX, textureCoordinates[1]);
        vertices[12] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, -size.Z), -Vector3.UnitX, textureCoordinates[2]);
        vertices[13] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, size.Z), -Vector3.UnitX, textureCoordinates[3]);

        // Rectangle face 3 (right side)
        vertices[14] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, size.Z), Vector3.UnitX, textureCoordinates[0]);
        vertices[15] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, size.Z), Vector3.UnitX, textureCoordinates[1]);
        vertices[16] = new VertexPositionNormalTexture(new Vector3(0, equilateralHeight, -size.Z), Vector3.UnitX, textureCoordinates[2]);
        vertices[17] = new VertexPositionNormalTexture(new Vector3(size.X, -equilateralHeight, -size.Z), Vector3.UnitX, textureCoordinates[3]);

        //// Triangle face indices
        indices[0] = 0; indices[1] = 1; indices[2] = 2; // Front
        indices[3] = 3; indices[4] = 5; indices[5] = 4; // Back

        //// Rectangle face indices
        // Bottom
        indices[6] = 6; indices[7] = 9; indices[8] = 8;
        indices[9] = 6; indices[10] = 8; indices[11] = 7;

        // Left
        indices[12] = 10; indices[13] = 11; indices[14] = 12;
        indices[15] = 10; indices[16] = 12; indices[17] = 13;

        // Right
        indices[18] = 14; indices[19] = 15; indices[20] = 16;
        indices[21] = 14; indices[22] = 16; indices[23] = 17;

        return new GeometricMeshData<VertexPositionNormalTexture>(vertices, indices, toLeftHanded) { Name = "TriangularPrism" };
    }
}
