extends Control


func _ready() -> void:
	Global.connect("variable_changed", self, "_on_Global_variable_changed")
	for character in Global.characters:
		var button := Button.new()
		button.text = " " + character + " "
		button.rect_min_size = Vector2(22,22)
		$Margin/Sepperator/Characters/HeaderSplit/Margin/Scroll/Split.add_child(button)
		button.connect("pressed", self, "_on_Button_pressed", [character])


func _on_Button_pressed(symbol) -> void:
	Global.emit_signal("write", symbol)

func _on_Global_variable_changed() -> void:
	var split := $Margin/Sepperator/Variables/HeaderSplit/Margin/Scroll/Split
	for child in split.get_children():
		child.name += "dead"
		child.queue_free()
	for variable in Global.variables:
		var label := Label.new()
		label.name = variable
		label.text = variable + " = " +str(Global.variables[variable])
		split.add_child(label)

