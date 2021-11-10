extends WindowDialog


func _ready() -> void:
	Data.connect("exporting", self, "_on_Data_exporting")


func _on_Data_exporting() -> void:
	for line in Data.exported:
		$Margin/Text.text += line + "\n"
	popup()
