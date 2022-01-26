extends VBoxContainer


func init(category: String) -> void:
	$Label.text = category
	if category == "derived":
		for unit in Global.derived:
			var button := Button.new()
			button.rect_min_size = Vector2(22,22)
			button.text = " " + unit + " "
			button.hint_tooltip = str(Global.derived[unit])
			$Grid.add_child(button)
			button.connect("pressed", self, "_on_Child_pressed", [unit])
	else:
		for unit in Global.categories[category]:
			var button := Button.new()
			button.rect_min_size = Vector2(22,22)
			button.text = " " + unit + " "
			button.hint_tooltip = str(Global.units[unit][0]) + " " + Global.units[unit][1]
			$Grid.add_child(button)
			button.connect("pressed", self, "_on_Child_pressed", [unit])


func _on_Child_pressed(unit) -> void:
	Global.emit_signal("write", unit)
