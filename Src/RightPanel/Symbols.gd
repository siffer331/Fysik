extends Control


func _ready():
	for symbol in Data.symbols:
		var button := Button.new()
		button.text = " " + symbol + " "
		button.hint_tooltip = Data.symbols[symbol][0] + "\n" + Data.symbols[symbol][1]
		button.rect_min_size = Vector2(22,22)
		$HeaderSplit/Margin/Scroll/Coloumns.add_child(button)
		button.connect("pressed", self, "_on_Button_pressed", [symbol])


func _on_Button_pressed(symbol) -> void:
	Data.emit_signal("write", symbol)
