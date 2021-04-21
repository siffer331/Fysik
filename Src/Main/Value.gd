class_name Value
extends Object

var value: float
var unit: Unit

func _init(value: float, unit := "[]"):
	self.value = value
	self.unit = Unit.new(unit.substr(1,len(unit)-2))
	self.value *= self.unit.convert_to_si()


func _to_string() -> String:
	for derived in Data.derived:
		if UA.equals(Data.derived[derived], unit):
			return str(value) + " " + derived
	var unit_string = " " + str(unit)
	if unit_string == " 1":
		unit_string = ""
	return str(value) + unit_string
