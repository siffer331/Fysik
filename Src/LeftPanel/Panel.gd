extends MarginContainer


func _ready() -> void:
	Data.connect("variable_changed", self, "_on_Data_variable_changed")


func _on_Data_variable_changed() -> void:
	var split = get_child(0)
	for child in split.get_children():
		child.name += "dead"
		child.queue_free()
	for variable in Data.variables:
		var label := Label.new()
		label.name = variable
		label.text = variable + " = " +str(Data.variables[variable])
		split.add_child(label)

