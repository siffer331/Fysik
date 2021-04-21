extends VBoxContainer


func init(category: String) -> void:
	$Label.text = category
	if category == "derived":
		for unit in Data.derived:
			var button := Button.new()
			button.rect_min_size = Vector2(22,22)
			button.text = " " + unit + " "
			button.hint_tooltip = str(Data.derived[unit])
			$Grid.add_child(button)
			button.connect("pressed", self, "_on_Child_pressed", [unit])
	else:
		for unit in Data.categories[category]:
			var button := Button.new()
			button.rect_min_size = Vector2(22,22)
			button.text = " " + unit + " "
			button.hint_tooltip = str(Data.units[unit][0]) + " " + Data.units[unit][1]
			$Grid.add_child(button)
			button.connect("pressed", self, "_on_Child_pressed", [unit])


func _on_Child_pressed(unit) -> void:
	Data.emit_signal("write", unit)
