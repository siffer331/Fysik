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

