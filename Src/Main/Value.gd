class_name Value
extends Object

var value: float
var p := 0
var unit: Unit

func _init(value: String, unit := "[]"):
	var parts = value.split("1e")
	if parts[0] == "":
		self.value = 1
	else:
		self.value = float(parts[0])
	if len(parts) > 1:
		p = int(parts[1])
	self.unit = Unit.new(unit.substr(1,len(unit)-2))
	self.unit = UA.derive(self.unit)
	self.value *= self.unit.convert_to_si()
	simplify()


func simplify() -> void:
	var dif = p%3
	p -= dif
	value *= pow(10,dif)
	
	while abs(value) >= 1000:
		value /= 1000
		p += 3
	while abs(value) < 1 and abs(value) > 0:
		value *= 1000
		p -= 3


func _to_string() -> String:
	for derived in Data.derived:
		if UA.equals(Data.derived[derived], unit):
			return str(value) + " " + derived
	var unit_string = " " + str(unit)
	var pow_string = "*10^"+str(p)
	if p == 0:
		pow_string = ""
	if unit_string == " 1":
		unit_string = ""
	return str(value) + pow_string + unit_string
