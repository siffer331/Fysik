extends VBoxContainer


func init(formula: String) -> void:
	name = formula
	$Label.text = formula
	var texture = load("res://Data/Formulas/" + formula + ".png")
	if texture:
		$Texture.texture = texture
		var aspect = texture.get_height()*1.0/texture.get_width()
		$Texture.rect_min_size.y = $Texture.rect_size.x*aspect
	var tool_tip := ""
	for categori in Data.formluas[formula].categories:
		tool_tip += categori + ", "
	tool_tip.erase(len(tool_tip)-2,2)
	$Label.hint_tooltip = tool_tip



func _on_Label_pressed():
	Data.emit_signal("write", name)
