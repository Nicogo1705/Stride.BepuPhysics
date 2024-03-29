﻿using Stride.BepuPhysics.Systems;
using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

namespace Stride.BepuPhysics.Constraints;

[DataContract(Inherited = true)]
[DefaultEntityComponentProcessor(typeof(ConstraintProcessor), ExecutionMode = ExecutionMode.Runtime)]
[ComponentCategory("Bepu - Constraint")]
[AllowMultipleComponents]
public abstract class ConstraintComponentBase : SyncScript
{
    private bool _enabled = true;
    private readonly BodyComponent?[] _bodies;

    public bool Enabled
    {
        get
        {
            return _enabled;
        }
        set
        {
            _enabled = value;
            UntypedConstraintData?.RebuildConstraint();
        }
    }

    public ReadOnlySpan<BodyComponent?> Bodies => _bodies;

    protected ConstraintComponentBase(int bodies) => _bodies = new BodyComponent?[bodies];

    protected BodyComponent? this[int i]
    {
        get => _bodies[i];
        set
        {
            _bodies[i] = value;
            UntypedConstraintData?.RebuildConstraint();
        }
    }

    public override void Update()
    {
        if (UntypedConstraintData?.Exist == false)
            UntypedConstraintData.RebuildConstraint();
    }

    internal abstract void RemoveDataRef();

    internal abstract ConstraintDataBase? UntypedConstraintData { get; }

    internal abstract ConstraintDataBase CreateProcessorData(BepuConfiguration bepuConfiguration);
}