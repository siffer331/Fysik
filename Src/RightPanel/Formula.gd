extends VBoxContainer


func init(formula: String) -> void:
	name = formula
	$Label.text = formula
	var path := "res://Data/Formulas/" + formula + ".png"
	if Directory.new().file_exists(path):
		var texture = load(path)
		$Texture.texture = texture
		var aspect = texture.get_height()*1.0/texture.get_width()
		$Texture.rect_min_size.y = $Texture.rect_size.x*aspect
	var tool_tip := ""
	for categori in Data.formulas[formula].categories:
		tool_tip += categori + ", "
	tool_tip.erase(len(tool_tip)-2,2)
	tool_tip += "\n"
	for key in Data.formulas[formula].versions.keys():
		tool_tip += key + ", "
	tool_tip.erase(len(tool_tip)-2,2)
	tool_tip += "\n"
	var key = Data.formulas[formula].versions.keys()[0]
	tool_tip += key + " = " + Data.formulas[formula].versions[key]
	$Label.hint_tooltip = tool_tip



func _on_Label_pressed():
	Data.emit_signal("write", name)
