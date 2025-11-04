using Godot;

public partial class Main : Node2D
{
	[Export] public Vector2 ChainPosition = new Vector2(400, 100);
	[Export] public Vector2 LaserPosition = new Vector2(200, 300);
	[Export] public Vector2 PlayerPosition = new Vector2(300, 400);
	[Export] public Vector2 ParticlePosition = new Vector2(500, 200);

	public override void _Ready()
	{
		// Get references to your nodes (adjust node names as needed)
		var physicsChain = GetNode<PhysicsChain>("PhysicsDemo");
		var laserDetector = GetNode<LaserDetector>("LaserSystem");
		var player = GetNode<Player>("Player");
		var particleSystem = GetNode<ParticleController>("ParticleSystem");

		// Set positions
		physicsChain.GlobalPosition = ChainPosition;
		laserDetector.GlobalPosition = LaserPosition;
		player.GlobalPosition = PlayerPosition;
		particleSystem.GlobalPosition = ParticlePosition;
	}
}
