class_name HeightColor extends Resource

@export var color:Color = Color.DARK_GREEN
@export var min_height:int = 0
@export var name:String = "Ground"


func _init(min_height:int = 0, color:Color = Color.DARK_GREEN, name:String = "Ground") -> void:
	self.min_height = min_height
	self.color = color
	self.name = name
	
