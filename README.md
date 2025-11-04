# Assignment 5 - Rendering and Physics

## Part 1: Custom Shader with Particles
The particle shader creates dynamic visual effects using wave distortion that animates over time, a smooth vertical color gradient between two customizable colors, and a pulsing transparency effect. These are achieved through UV manipulation and time-based calculations in the fragment shader.

## Part 2: Physics Chain with Joints
The physics system builds a realistic chain using RigidBody2D segments connected by PinJoint2D joints, starting from a fixed StaticBody2D anchor. Each segment has configured mass, damping, and collision properties for natural movement, and responds to player interactions through both area detection and direct collision impulses.

## Part 3: Raycast Laser Detection
The laser system uses RayCast2D to detect collisions with the player and chain segments, visualizing the beam with a Line2D that updates in real-time. When the player is detected, it triggers an alarm with visual feedback including color changes and flashing effects, using multiple detection methods for reliability.
