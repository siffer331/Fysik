class_name UA
extends Node


static func size(a: String) -> int:
	var count := 0
	for unit in Data.derived[a].units:
		count += abs(Data.derived[a].units[unit])
	return count


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


static func inverse(a: Unit) -> Unit:
	var res = Unit.new()
	res.units = a.units.duplicate()
	for part in res.units:
		res.units[part] *= -1
	return res


static func divide_u(a: Unit, b: Unit) -> Unit:
	return multiply_u(a, inverse(b))


static func equals(a: Unit, b: Unit) -> bool:
	return compare_dictionary(a.units, b.units)


static func get_readable(a: Unit) -> Unit:
	var res = Unit.new()
	res.units = a.units.duplicate(true)
	for derived in Data.derived_order:
		var u = derive(Data.derived[derived])
		while dictionary_less(u.units, res.units):
			res = divide_u(res, u)
			if not derived in res:
				res.units[derived] = 0
			res.units[derived] += 1
		for k in u.units:
			u.units[k] *= -1
		while dictionary_less(u.units, res.units):
			res = divide_u(res, u)
			if not derived in res:
				res.units[derived] = 0
			res.units[derived] -= 1
	return res


static func dictionary_less(a: Dictionary, b: Dictionary, debug := false) -> bool:
	if debug:
		print(a, " ", b)
	for key in a:
		if not b.has(key):
			if debug:
				print(1, " ", key)
			return false
		if abs(a[key]) > abs(b[key]) or sign(a[key]) != sign(b[key]):
			if debug:
				print(2, " ", key)
			return false
	return true


static func compare_dictionary(a: Dictionary, b: Dictionary) -> bool:
	if a.size() != b.size():
		return false
	for key in a:
		if not b.has(key):
			return false
		if a[key] != b[key]:
			return false
	return true


static func derive(u: Unit) -> Unit:
	var res := Unit.new()
	var changed := true
	for unit in u.units:
		var val: int = u.units[unit]
		if unit in Data.derived:
			for unit2 in Data.derived[unit].units:
				if not unit2 in res.units:
					res.units[unit2] = 0
				res.units[unit2] += val*Data.derived[unit].units[unit2]
		else:
			if not unit in res.units:
				res.units[unit] = 0
			res.units[unit] += val
	res._simplify()
	return res

