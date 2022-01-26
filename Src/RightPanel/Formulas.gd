extends Control


func _ready() -> void:
	var formula_load = load("res://Src/RightPanel/Formula.tscn")
	for formula in Global.formulas:
		var formula_node: Control = formula_load.instance()
		formula_node.init(formula)
		$HeaderSplit/Margin/Scroll/Split.add_child(formula_node)


func _on_Search_text_changed(s: String) -> void:
	for formula in $HeaderSplit/Margin/Scroll/Split.get_children():
		formula.visible = s in formula.name or s == ""
		for category in Global.formulas[formula.name].categories:
			if s in category:
				formula.show()
				break
