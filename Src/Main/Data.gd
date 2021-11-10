extends Node

signal variable_changed()

const prefixes := {
	'Y': 24,
	'Z': 21,
	'E': 18,
	'P': 15,
	'T': 12,
	'G': 9,
	'M': 6,
	'k': 3,
	'h': 2,
	'D': 1,
	'd': -1,
	'c': -2,
	'm': -3,
	'μ': -6,
	'n': -9,
	'p': -12,
	'f': -15,
	'a': -18,
	'z': -21,
	'y': -24,
}


signal write(text)

#static setup
var units := {}
var derived := {}
var constants := {}
var si := []
var categories := {}
var symbols := {}
var scaleable := []
var formulas := {}
var characters := ["π", "Ω", "μ", "τ", "ε"]
var derived_order := []
var exported := []

#dynamic
var variables := {}
var defaults := {}


func set_variable(variable: String, value: Value) -> void:
	variables[variable] = value
	emit_signal("variable_changed")
