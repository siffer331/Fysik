class_name UA
extends Node


static func equals(a: Unit, b: Unit) -> bool:
	return compare_dictionary(a.units, b.units)


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
	for unit in u.units:
		var val: int = u.units[unit]
		if unit in Data.derived:
			var d := derive(Data.derived[unit])
			for unit2 in d.units:
				if not unit2 in res.units:
					res.units[unit2] = 0
				res.units[unit2] += val*d.units[unit2]
		else:
			if not unit in res.units:
				res.units[unit] = 0
			res.units[unit] += val
	return res

