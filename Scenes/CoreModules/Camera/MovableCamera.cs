using Godot;

namespace TerrainGenerationApp.Scenes.CoreModules.Camera;

public partial class MovableCamera : Camera3D
{
    [ExportCategory("Camera Movement Settings")]
    // Defines the speed of camera movement
    [Export] public float CameraSpeed { get; set; } = 35.0f;

    [ExportCategory("Camera Rotation Settings")]
    // Defines how fast the camera rotates
    [Export] public float RotationSpeed { get; set; } = 0.005f;

    [ExportCategory("Movement Constraints")]
    // Minimum X position allowed
    [Export] public float MinX { get; set; } = -1000.0f;
    // Maximum X position allowed
    [Export] public float MaxX { get; set; } = 1000.0f;
    // Minimum Y position allowed
    [Export] public float MinY { get; set; } = 0.0f;
    // Maximum Y position allowed
    [Export] public float MaxY { get; set; } = 500.0f;
    // Minimum Z position allowed
    [Export] public float MinZ { get; set; } = -1000.0f;
    // Maximum Z position allowed
    [Export] public float MaxZ { get; set; } = 1000.0f;

    // Mouse state tracking
    private bool _mouseCapured = false;

    public override void _Ready()
    {
        // Initial setup
    }

    public override void _Process(double delta)
    {
        // Handle keyboard movement
        Vector3 movement = Vector3.Zero;

        if (Input.IsActionPressed("move_right"))
        {
            movement += Transform.Basis.X;
        }
        if (Input.IsActionPressed("move_left"))
        {
            movement -= Transform.Basis.X;
        }
        if (Input.IsActionPressed("move_forward"))
        {
            if (Input.IsActionPressed("slow_movement"))
            {
                movement -= Transform.Basis.Z * 0.3f;
            }
            else
            {
                movement -= Transform.Basis.Z;
            }
        }
        if (Input.IsActionPressed("move_backward"))
        {
            if (Input.IsActionPressed("slow_movement"))
            {
                movement += Transform.Basis.Z * 0.15f;
            }
            else
            {
                movement += Transform.Basis.Z;
            }
        }
        if (Input.IsActionPressed("ui_select")) // Space key for up
        {
            movement += Vector3.Up;
        }
        if (Input.IsKeyPressed(Key.Ctrl)) // Ctrl key for down
        {
            movement -= Vector3.Up;
        }

        // Apply movement
        if (movement.Length() > 0)
        {
            movement = movement.Normalized() * CameraSpeed * (float)delta;
            if (Input.IsActionPressed("slow_movement"))
            {
                movement *= 0.3f;
            }
            if (Input.IsActionPressed("accelerate_movement"))
            {
                movement *= 2.0f;
            }

            Position += movement;

            // Apply constraints
            Position = new Vector3(
                Mathf.Clamp(Position.X, MinX, MaxX),
                Mathf.Clamp(Position.Y, MinY, MaxY),
                Mathf.Clamp(Position.Z, MinZ, MaxZ)
            );
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Camera rotation with right mouse button (in any direction)
        if (@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
        {
            // Horizontal rotation (around Y axis)
            RotateY(-mouseMotion.Relative.X * RotationSpeed);

            // Vertical rotation (around X axis)
            // Rotate around local X axis
            float xRotation = -mouseMotion.Relative.Y * RotationSpeed;

            // Calculate what the new X rotation would be
            float currentRot = Rotation.X;
            float newRot = currentRot + xRotation;

            // Prevent flipping by limiting vertical rotation between -89 and 89 degrees
            if (newRot > -1.55f && newRot < 1.55f)
            {
                RotateObjectLocal(Vector3.Right, xRotation);
            }
        }
    }

    public void SetMovementLimits(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
    {
        MinX = minX;
        MaxX = maxX;
        MinY = minY;
        MaxY = maxY;
        MinZ = minZ;
        MaxZ = maxZ;

        var curPos = Position;
        Position = new Vector3(
            Mathf.Clamp(curPos.X, MinX, MaxX),
            Mathf.Clamp(curPos.Y, MinY, MaxY),
            Mathf.Clamp(curPos.Z, MinZ, MaxZ)
        );
    }
}