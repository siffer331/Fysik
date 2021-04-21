extends Control


func _ready() -> void:
	var category_load = load("res://Src/RightPanel/Category.tscn")
	for category in Data.categories:
		var category_node: Control = category_load.instance()
		category_node.init(category)
		$HeaderSplit/Margin/Scroll/Split.add_child(category_node)
	var category_node: Control = category_load.instance()
	category_node.init("derived")
	$HeaderSplit/Margin/Scroll/Split.add_child(category_node)
