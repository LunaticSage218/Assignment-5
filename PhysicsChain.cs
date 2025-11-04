using Godot;
using System.Collections.Generic;

public partial class PhysicsChain : Node2D
{
	[Export] public int ChainSegments = 6;
	[Export] public float SegmentDistance = 30f;
	[Export] public Vector2 ChainStartPosition = new Vector2(400, 100);
	[Export] public float SegmentMass = 0.8f;
	[Export] public float JointSoftness = 0.5f;

	private readonly List<RigidBody2D> _segments = new();
	private readonly List<PinJoint2D> _joints = new();

	public override void _Ready()
	{
		// Call after a small delay to ensure proper physics initialization
		CallDeferred(nameof(CreateChain));
	}

	private void CreateChain()
	{
		Node2D previousBody = null;

		for (int i = 0; i < ChainSegments; i++)
		{
			if (i == 0)
			{
				// Anchor (StaticBody2D)
				var anchor = new StaticBody2D
				{
					Position = ChainStartPosition,
					Name = "Anchor"
				};
				
				// Make sure anchor is on layer 1 for player detection
				anchor.CollisionLayer = 1;
				anchor.CollisionMask = 0;
				
				AddChild(anchor);

				var shape = new CollisionShape2D();
				shape.Shape = new RectangleShape2D { Size = new Vector2(20, 20) };
				anchor.AddChild(shape);

				var visual = new ColorRect
				{
					Size = new Vector2(20, 20),
					Position = new Vector2(-10, -10),
					Color = new Color(0.5f, 0.5f, 0.5f)
				};
				anchor.AddChild(visual);

				previousBody = anchor;
			}
			else
			{
				// Rigid segment
				var segment = new RigidBody2D
				{
					Position = ChainStartPosition + new Vector2(0, i * SegmentDistance),
					Mass = SegmentMass,
					GravityScale = 1f,
					LinearDamp = 0.1f,
					AngularDamp = 0.1f,
					Name = $"Segment_{i}",
					CenterOfMassMode = RigidBody2D.CenterOfMassModeEnum.Custom,
					CenterOfMass = Vector2.Zero
				};

				// Set physics material
				segment.PhysicsMaterialOverride = new PhysicsMaterial
				{
					Bounce = 0.2f,
					Friction = 0.3f
				};

				// CRITICAL: Set collision layers so player can detect and push
				segment.CollisionLayer = 1;  // On layer 1
				segment.CollisionMask = 1;   // Detect other segments on layer 1

				// Enable continuous collision detection for better physics
				segment.ContinuousCd = RigidBody2D.CcdMode.CastRay;
				segment.MaxContactsReported = 4;
				segment.ContactMonitor = true;

				AddChild(segment);
				_segments.Add(segment);

				// Collision shape
				var shape = new CollisionShape2D();
				shape.Shape = new RectangleShape2D { Size = new Vector2(15, 25) };
				segment.AddChild(shape);

				// Visual
				var visual = new ColorRect
				{
					Size = new Vector2(15, 25),
					Position = new Vector2(-7.5f, -12.5f),
					Color = new Color(0.8f, 0.3f, 0.2f)
				};
				segment.AddChild(visual);

				// Joint - connect to previous segment
				var joint = new PinJoint2D
				{
					Name = $"Joint_{i}",
					NodeA = previousBody.GetPath(),
					NodeB = segment.GetPath(),
					Softness = JointSoftness,
					Bias = 0.3f,
					DisableCollision = false  // Allow segments to collide with each other
				};

				// Position joint between the two bodies
				if (i == 1)
				{
					// First joint connects anchor to first segment
					joint.Position = previousBody.Position + new Vector2(0, SegmentDistance / 2);
				}
				else
				{
					// Subsequent joints
					joint.Position = previousBody.Position + new Vector2(0, SegmentDistance / 2);
				}

				AddChild(joint);
				_joints.Add(joint);

				previousBody = segment;
			}
		}

		GD.Print($"Created chain with {_segments.Count} segments and {_joints.Count} joints");
	}

	// External access
	public void ApplyForceToSegment(int index, Vector2 force)
	{
		if (index >= 0 && index < _segments.Count)
		{
			_segments[index].ApplyImpulse(force);
		}
	}

	public void ApplyForceToAll(Vector2 force)
	{
		foreach (var segment in _segments)
		{
			segment.ApplyImpulse(force);
		}
	}

	public void ApplyCentralForceToSegment(int index, Vector2 force)
	{
		if (index >= 0 && index < _segments.Count)
		{
			_segments[index].ApplyCentralForce(force);
		}
	}

	public int GetSegmentCount()
	{
		return _segments.Count;
	}

	public RigidBody2D GetSegment(int index)
	{
		if (index >= 0 && index < _segments.Count)
			return _segments[index];
		return null;
	}
}
