** Assignment 5

** Part 1: Custom Shader with Particles
The shader creates dynamic particle effects using wave distortion that moves over time, a vertical color gradient that blends between two customizable colors, and a pulsing alpha effect for added visual interest. These effects are achieved through UV manipulation and time-based animations in the fragment shader.

** Part 2: Physics Chain with Joints
The physics system creates a realistic chain using RigidBody2D segments connected by PinJoint2D joints, starting with a StaticBody2D anchor. Each segment has configured mass, damping, and collision properties for natural movement, and responds to player interactions through both area detection and direct collision impulses.

** Part 3: Raycast Laser Detection
The laser system uses RayCast2D to detect collisions with the player and chain segments, visualizing the beam with a Line2D that updates in real-time. When the player is detected, it triggers an alarm with visual feedback (color changes and flashing) and uses multiple detection methods for reliable player identification across different scenarios.
