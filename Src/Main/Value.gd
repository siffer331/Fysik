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
	var factor: Array = self.unit.convert_to_si()
	self.value *= factor[0]
	p += factor[1]
	simplify()


func get_value() -> float:
	return value*pow(10,p)


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
	var unit_string = " " + str(UA.get_readable(unit))
	var pow_string = "*10^"+str(p)
	var f := 1.0
	if p == 0:
		pow_string = ""
	if p == -3:
		f = 0.001
		pow_string = ""
	if unit_string == " 1":
		unit_string = ""
	return str(value*f) + pow_string + unit_string
