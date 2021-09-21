class_name A
extends Node


static func multiply_u(a: Unit, b: Unit) -> Unit:
	var res := Unit.new()
	for part in a.units:
		if not part in res.units:
			res.units[part] = 0
		res.units[part] += a.units[part]
	for part in b.units:
		if not part in res.units:
			res.units[part] = 0
		res.units[part] += b.units[part]
	res._simplify()
	return res


static func divide_u(a: Unit, b: Unit) -> Unit:
	return multiply_u(a, inverse(b))


static func inverse(a: Unit) -> Unit:
	var res = Unit.new()
	res.units = a.units.duplicate()
	for part in res.units:
		res.units[part] *= -1
	return res


static func copy(a: Unit) -> Unit:
	var res = Unit.new()
	res.units = a.units.duplicate()
	return res


static func copy_value(a: Value) -> Value:
	var res := Value.new(a.value)
	res.unit = copy(a.unit)
	return res


static func add(a: Value, b: Value):
	if not UA.equals(a.unit, b.unit):
		return "/Cant add different units"
	var res = Value.new(a.value+b.value)
	res.unit = copy(a.unit)
	return res


static func subtract(a: Value, b: Value):
	if not UA.equals(a.unit, b.unit):
		return "/Cant subtract different units"
	var res = Value.new(a.value-b.value)
	res.unit = copy(a.unit)
	return res


static func divide_v(a: Value, b: Value):
	if b.value == 0:
		return "/Cant divide with 0"
	var res = Value.new(a.value/b.value)
	res.unit = divide_u(a.unit, b.unit)
	return res


static func multiply_v(a: Value, b: Value) -> Value:
	var res = Value.new(a.value*b.value)
	res.unit = multiply_u(a.unit, b.unit)
	return res


