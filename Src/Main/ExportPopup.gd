extends WindowDialog


func _ready() -> void:
	Global.connect("exporting", self, "_on_Global_exporting")


func _on_Global_exporting() -> void:
	$Margin/Text.text = ""
	for line in Global.exported:
		$Margin/Text.text += line + "\n"
	popup()
