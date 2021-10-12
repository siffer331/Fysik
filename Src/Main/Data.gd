extends Node

signal variable_changed(variable)


signal write(text)

#static setup
var units := {}
var derived := {}
var constants := {}
var si := []
var categories := {}
var symbols := {}
var formulas := {}

#dynamic
var variables := {}


func set_variable(variable: String, value: Value) -> void:
	variables[variable] = value
	emit_signal("variable_changed", variable)
