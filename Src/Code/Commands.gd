class_name Command
extends Node

#return ["error", "/oh no"]

func dealloc(args: Array, code: Code) -> Array:
	if not args[0] in Data.variables:
		return ["error", "variable does not excist"]
	Data.variables.erase(args[0])
	return [["", Code.TYPES.EMPTY]]


func to(args: Array, code: Code) -> Array:
	var unit := Unit.new(args[1])
	var si := A.copy(unit)
	var factor := si.convert_to_si() 
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value := A.copy_value(res[1][0])
	if not UA.equals(value.unit, si):
		return ["error", "the units are no the same"]
	value.value /= factor
	value.unit = unit
	value.simplify()
	return [[value, Code.TYPES.VALUE]]


func sin(args: Array, code: Code) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = sin(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func tan(args: Array, code: Code) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = tan(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func cos(args: Array, code: Code) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = cos(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func sqrt(args: Array, code: Code) -> Array:
	var res = code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = sqrt(value.value)
	for unit in value.unit.units:
		value.unit.units[unit] /= 2
	return [[value, Code.TYPES.VALUE]]


func sq(args: Array, code: Code) -> Array:
	var res = code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = value.value*value.value
	for unit in value.unit.units:
		value.unit.units[unit] *= 2
	return [[value, Code.TYPES.VALUE]]
