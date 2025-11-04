shader_type canvas_item;

// Uniform variables for customization
uniform float wave_intensity = 1.0;
uniform float wave_frequency = 20.0;
uniform float time_scale = 1.0;
uniform vec4 color_start : source_color = vec4(1.0, 0.5, 0.0, 1.0);
uniform vec4 color_end : source_color = vec4(1.0, 0.0, 0.5, 1.0);

void fragment() {
    // Get UV coordinates
    vec2 uv = UV;
    
    // Apply wave distortion based on time
    // Creates a sine wave that moves horizontally
    uv.x += sin(uv.y * wave_frequency + TIME * time_scale) * wave_intensity;
    
    // Create color gradient based on vertical position
    // Smoothly interpolate between start and end colors
    float gradient = uv.y;
    vec4 gradient_color = mix(color_start, color_end, gradient);
    
    // Sample the original texture
    vec4 tex_color = texture(TEXTURE, uv);
    
    // Apply gradient to texture color
    COLOR = tex_color * gradient_color;
    
    // Add pulsing alpha effect based on time
    COLOR.a *= tex_color.a * (0.8 + 0.2 * sin(TIME * 2.0));
}