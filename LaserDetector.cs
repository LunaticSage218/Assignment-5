using Godot;

public partial class LaserDetector : Node2D
{
	[Export] public float LaserLength = 500f;
	[Export] public Color LaserColorNormal = Colors.Green;
	[Export] public Color LaserColorAlert = Colors.Red;
	[Export] public NodePath PlayerPath;
	[Export] public float LaserRotation = 0f; // Angle in degrees

	private RayCast2D _rayCast;
	private Line2D _laserBeam;
	private Node2D _hitIndicator;
	private Node2D _player;
	private bool _isAlarmActive = false;
	private Timer _alarmTimer;
	private float _alarmFlashTime = 0f;

	public override void _Ready()
	{
		// Set default position if at origin
		if (GlobalPosition == Vector2.Zero)
		{
			GlobalPosition = new Vector2(200, 300);
		}
		
		SetupRaycast();
		SetupVisuals();
		
		// Get player reference - try multiple methods
		if (PlayerPath != null && !PlayerPath.IsEmpty)
		{
			_player = GetNode<Node2D>(PlayerPath);
		}
		
		// If PlayerPath not set, try to find player by name
		if (_player == null)
		{
			_player = GetTree().Root.FindChild("Player", true, false) as Node2D;
		}
		
		// Setup alarm timer
		_alarmTimer = new Timer();
		_alarmTimer.WaitTime = 0.5f;
		_alarmTimer.OneShot = true;
		_alarmTimer.Timeout += ResetAlarm;
		AddChild(_alarmTimer);
	}

	private void SetupRaycast()
	{
		// Create and configure RayCast2D
		_rayCast = new RayCast2D();
		_rayCast.Enabled = true;
		_rayCast.HitFromInside = false; // Changed to false for proper detection
		
		// Set target position based on rotation
		float angleRad = Mathf.DegToRad(LaserRotation);
		Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
		_rayCast.TargetPosition = direction * LaserLength;
		
		// Set collision mask to detect both player (layer 2) and chains (layer 1)
		_rayCast.CollisionMask = 3; // Binary: 11 = layers 1 and 2
		
		AddChild(_rayCast);
	}

	private void SetupVisuals()
	{
		// Create Line2D for laser visualization
		_laserBeam = new Line2D();
		_laserBeam.Width = 3.0f;
		_laserBeam.DefaultColor = LaserColorNormal;
		_laserBeam.ZIndex = 10;
		
		// Add initial points
		_laserBeam.AddPoint(Vector2.Zero);
		_laserBeam.AddPoint(Vector2.Zero);
		
		AddChild(_laserBeam);
		
		// Create hit indicator (small circle)
		_hitIndicator = new Node2D();
		var indicator = new Polygon2D();
		var circle = new System.Collections.Generic.List<Vector2>();
		int segments = 8;
		for (int i = 0; i < segments; i++)
		{
			float angle = (float)i / segments * Mathf.Tau;
			circle.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 5);
		}
		indicator.Polygon = circle.ToArray();
		indicator.Color = LaserColorNormal;
		_hitIndicator.AddChild(indicator);
		_hitIndicator.Visible = false;
		
		AddChild(_hitIndicator);
	}

	public override void _PhysicsProcess(double delta)
	{
		// Force raycast update
		_rayCast.ForceRaycastUpdate();
		
		// Check if raycast is colliding
		bool isColliding = _rayCast.IsColliding();
		Vector2 endPoint;
		
		if (isColliding)
		{
			// Get collision point relative to laser origin
			endPoint = _rayCast.GetCollisionPoint() - GlobalPosition;
			
			// Show hit indicator at collision point
			_hitIndicator.Visible = true;
			_hitIndicator.GlobalPosition = _rayCast.GetCollisionPoint();
			
			// Check if hit object is player
			var collider = _rayCast.GetCollider();
			
			// Check multiple ways to ensure we catch the player
			bool isPlayer = false;
			
			if (_player != null && collider != null)
			{
				// Direct comparison
				if (collider == _player)
				{
					isPlayer = true;
				}
				// Check if it's a CharacterBody2D
				else if (collider is CharacterBody2D characterBody)
				{
					// Check by instance ID
					if (characterBody.GetInstanceId() == _player.GetInstanceId())
					{
						isPlayer = true;
					}
					// Check by name
					else if (characterBody.Name == _player.Name)
					{
						isPlayer = true;
					}
				}
			}
			
			if (isPlayer)
			{
				if (!_isAlarmActive)
				{
					TriggerAlarm();
					
					// Optionally call player's detection method
					if (_player is Player player)
					{
						player.OnLaserDetected();
					}
				}
			}
			else if (_isAlarmActive && !_alarmTimer.IsStopped())
			{
				// Keep alarm active briefly after losing sight
			}
			else if (_isAlarmActive && _alarmTimer.IsStopped())
			{
				// Reset alarm if it hit something else and timer expired
				ResetAlarm();
			}
		}
		else
		{
			// Show full length if no collision
			float angleRad = Mathf.DegToRad(LaserRotation);
			Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
			endPoint = direction * LaserLength;
			_hitIndicator.Visible = false;
			
			// Reset alarm if nothing is being detected
			if (_isAlarmActive && _alarmTimer.IsStopped())
			{
				ResetAlarm();
			}
		}
		
		UpdateLaserBeam(endPoint);
		
		// Update flashing effect
		if (_isAlarmActive)
		{
			_alarmFlashTime += (float)delta;
			float flash = (Mathf.Sin(_alarmFlashTime * 10.0f) + 1.0f) / 2.0f;
			_laserBeam.DefaultColor = LaserColorAlert.Lerp(Colors.White, flash * 0.5f);
		}
	}

	private void UpdateLaserBeam(Vector2 endPoint)
	{
		// Update Line2D points based on raycast
		// Laser always starts at origin and goes to collision or max length
		_laserBeam.SetPointPosition(0, Vector2.Zero);
		_laserBeam.SetPointPosition(1, endPoint);
	}

	private void TriggerAlarm()
	{
		_isAlarmActive = true;
		_alarmFlashTime = 0f;
		
		// Change laser color
		_laserBeam.DefaultColor = LaserColorAlert;
		
		if (_hitIndicator.GetChildCount() > 0 && _hitIndicator.GetChild(0) is Polygon2D indicator)
		{
			indicator.Color = LaserColorAlert;
		}
		
		// Print alarm message
		GD.Print("ALARM! Player detected!");
		
		// Start timer to keep alarm active
		_alarmTimer.Start();
	}

	private void ResetAlarm()
	{
		_isAlarmActive = false;
		
		// Reset laser to normal color
		_laserBeam.DefaultColor = LaserColorNormal;
		
		if (_hitIndicator.GetChildCount() > 0 && _hitIndicator.GetChild(0) is Polygon2D indicator)
		{
			indicator.Color = LaserColorNormal;
		}
		
		GD.Print("Alarm reset - area clear");
	}
}
