extends Camera3D

# Camera movement settings
@export_category("Camera Movement Settings")
## Defines the speed of camera movement
@export var camera_speed: float = 50.0

# Camera rotation settings
@export_category("Camera Rotation Settings")
## Defines how fast the camera rotates
@export var rotation_speed: float = 0.005

# Camera position constraints
@export_category("Movement Constraints")
## Minimum X position allowed
@export var min_x: float = -1000.0
## Maximum X position allowed
@export var max_x: float = 1000.0
## Minimum Y position allowed
@export var min_y: float = 0.0
## Maximum Y position allowed
@export var max_y: float = 500.0
## Minimum Z position allowed
@export var min_z: float = -1000.0
## Maximum Z position allowed
@export var max_z: float = 1000.0

# Mouse state tracking
var mouse_captured: bool = false

func _ready():
	# Initial setup
	pass

func _process(delta):
	# Handle keyboard movement
	var movement = Vector3.ZERO
	
	if Input.is_action_pressed("move_right"):
		movement += transform.basis.x
	if Input.is_action_pressed("move_left"):
		movement -= transform.basis.x
	if Input.is_action_pressed("move_forward"):
		movement -= transform.basis.z
	if Input.is_action_pressed("move_backward"):
		movement += transform.basis.z
	if Input.is_action_pressed("ui_select"): # Space key for up
		movement += Vector3.UP
	if Input.is_key_pressed(KEY_CTRL): # Ctrl key for down
		movement -= Vector3.UP
	
	# Apply movement
	if movement.length() > 0:
		movement = movement.normalized() * camera_speed * delta
		position += movement
		
		# Apply constraints
		position.x = clamp(position.x, min_x, max_x)
		position.y = clamp(position.y, min_y, max_y)
		position.z = clamp(position.z, min_z, max_z)

func _input(event):
	# Camera rotation with right mouse button (in any direction)
	if event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
		# Horizontal rotation (around Y axis)
		rotate_y(-event.relative.x * rotation_speed)
		
		# Vertical rotation (around X axis)
		# Rotate around local X axis
		var x_rotation = -event.relative.y * rotation_speed
		
		# Calculate what the new X rotation would be
		var current_rot = rotation.x
		var new_rot = current_rot + x_rotation
		
		# Prevent flipping by limiting vertical rotation between -89 and 89 degrees
		if new_rot > -1.55 and new_rot < 1.55:
			rotate_object_local(Vector3(1, 0, 0), x_rotation)
