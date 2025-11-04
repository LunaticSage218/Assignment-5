using Godot;

public partial class ParticleController : GpuParticles2D
{
	private ShaderMaterial _shaderMaterial;
	private float _time = 0f;
	
	[Export] public float WaveIntensity { get; set; } = 0.1f;
	[Export] public float WaveFrequency { get; set; } = 10.0f;
	[Export] public Color ColorStart { get; set; } = new Color(1.0f, 0.5f, 0.0f);
	[Export] public Color ColorEnd { get; set; } = new Color(1.0f, 0.0f, 0.5f);

	public override void _Ready()
	{
		// Load the custom shader
		var shader = GD.Load<Shader>("res://custom_particle.gdshader");
		_shaderMaterial = new ShaderMaterial();
		_shaderMaterial.Shader = shader;
		
		// Set initial shader parameters
		_shaderMaterial.SetShaderParameter("wave_intensity", WaveIntensity);
		_shaderMaterial.SetShaderParameter("wave_frequency", WaveFrequency);
		_shaderMaterial.SetShaderParameter("time_scale", 1.0f);
		_shaderMaterial.SetShaderParameter("color_start", ColorStart);
		_shaderMaterial.SetShaderParameter("color_end", ColorEnd);
		
		// Apply shader material to particles
		Material = _shaderMaterial;
		
		// Configure particle properties
		Amount = 100;
		Lifetime = 7.0;
		Explosiveness = 0.0f;
		Randomness = 0.5f;
		FixedFps = 60;
		
		// Create and configure process material
		var processMaterial = new ParticleProcessMaterial();
		processMaterial.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Sphere;
		processMaterial.EmissionSphereRadius = 10.0f;
		processMaterial.Direction = new Vector3(0, -1, 0);
		processMaterial.Spread = 45.0f;
		processMaterial.InitialVelocityMin = 50.0f;
		processMaterial.InitialVelocityMax = 100.0f;
		processMaterial.Gravity = new Vector3(0, 98.0f, 0);
		processMaterial.ScaleMin = 1.5f;
		processMaterial.ScaleMax = 2.5f;
		
		ProcessMaterial = processMaterial;
		
		// Create a simple texture for particles
		var gradientTexture = new GradientTexture2D();
		var gradient = new Gradient();
		gradient.SetColor(0, Colors.White);
		gradient.SetColor(1, Colors.Transparent);
		gradientTexture.Gradient = gradient;
		gradientTexture.Width = 32;
		gradientTexture.Height = 32;
		Texture = gradientTexture;
		
		// Start emitting
		Emitting = true;
	}

	public override void _Process(double delta)
	{
		_time += (float)delta;
		
		// Animate shader parameters over time
		if (_shaderMaterial != null)
		{
			// Vary wave intensity with sine wave
			float waveIntensity = 0.1f + 0.05f * Mathf.Sin(_time * 2.0f);
			_shaderMaterial.SetShaderParameter("wave_intensity", waveIntensity);
			
			// Cycle through colors
			float colorCycle = (Mathf.Sin(_time * 0.5f) + 1.0f) / 2.0f;
			var color1 = new Color(1.0f, 0.5f * colorCycle, 0.0f, 1.0f);
			var color2 = new Color(1.0f, 0.0f, 0.5f + 0.5f * colorCycle, 1.0f);
			
			_shaderMaterial.SetShaderParameter("color_start", color1);
			_shaderMaterial.SetShaderParameter("color_end", color2);
		}
	}
}
