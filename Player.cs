using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 200f;
	[Export] public float Acceleration = 1500f;
	[Export] public float Friction = 1200f;
	[Export] public float ChainPushForce = 150f;
	[Export] public float ChainDetectionRadius = 60f;

	private Sprite2D _sprite;
	private CollisionShape2D _collisionShape;

	public override void _Ready()
	{
		// Set player collision layers
		// Player on layer 2, detects layer 1 (chains)
		CollisionLayer = 2;
		CollisionMask = 1;

		// Get references to child nodes
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		Vector2 inputDirection = GetInputDirection();

		// Move player with acceleration and friction
		if (inputDirection != Vector2.Zero)
		{
			velocity = velocity.MoveToward(inputDirection * Speed, Acceleration * (float)delta);
			UpdateSpriteDirection(inputDirection);

			// Apply force to nearby chain segments
			ApplyForceToNearbyChains(inputDirection);
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
		
		// Also push chains when colliding during movement
		PushCollidingChains(inputDirection);
	}

	private Vector2 GetInputDirection()
	{
		Vector2 direction = Vector2.Zero;

		if (Input.IsActionPressed("ui_right"))
			direction.X += 1;
		if (Input.IsActionPressed("ui_left"))
			direction.X -= 1;
		if (Input.IsActionPressed("ui_down"))
			direction.Y += 1;
		if (Input.IsActionPressed("ui_up"))
			direction.Y -= 1;

		return direction.Normalized();
	}

	private void UpdateSpriteDirection(Vector2 direction)
	{
		if (_sprite == null) return;

		_sprite.FlipH = direction.X < 0;

		// Optional slight scaling effect while moving
		_sprite.Scale = direction != Vector2.Zero ? new Vector2(1.05f, 0.95f) : Vector2.One;
	}

	private void ApplyForceToNearbyChains(Vector2 direction)
	{
		// Use area query to detect nearby physics bodies
		var spaceState = GetWorld2D().DirectSpaceState;
		var shape = new CircleShape2D { Radius = ChainDetectionRadius };

		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = shape,
			Transform = new Transform2D(0, GlobalPosition),
			CollisionMask = 1, // Detect objects on layer 1 (chain segments)
			Exclude = new Godot.Collections.Array<Rid> { GetRid() } // Exclude self
		};

		var results = spaceState.IntersectShape(query, 32);

		foreach (var result in results)
		{
			if (result.ContainsKey("collider"))
			{
				var collider = result["collider"].As<GodotObject>();
				if (collider is RigidBody2D rigidBody)
				{
					// Calculate direction from player to segment
					Vector2 toSegment = rigidBody.GlobalPosition - GlobalPosition;
					float distance = toSegment.Length();
					
					if (distance > 0)
					{
						// Apply force proportional to distance (closer = stronger push)
						float forceMagnitude = ChainPushForce * (1.0f - distance / ChainDetectionRadius);
						Vector2 force = direction.Normalized() * forceMagnitude;
						rigidBody.ApplyImpulse(force, toSegment.Normalized() * 10f);
					}
				}
			}
		}
	}

	private void PushCollidingChains(Vector2 direction)
	{
		// Push any chains we're directly colliding with
		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			var collision = GetSlideCollision(i);
			var collider = collision.GetCollider();
			
			if (collider is RigidBody2D rigidBody)
			{
				// Apply impulse in direction of movement
				Vector2 pushDirection = direction != Vector2.Zero ? direction : collision.GetNormal();
				rigidBody.ApplyImpulse(pushDirection * ChainPushForce * 0.5f);
			}
		}
	}

	// Optional: method when player is detected by lasers
	public void OnLaserDetected()
	{
		Modulate = new Color(1.0f, 0.3f, 0.3f);

		var timer = GetTree().CreateTimer(0.3);
		timer.Timeout += () =>
		{
			if (IsInstanceValid(this))
				Modulate = Colors.White;
		};
	}
}
