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


func to_latex() -> String:
	var res := str(value)
	if p != 0:
		res += "\\cdot10^{"+str(p)+"}"
	var u := UA.get_readable(unit)
	var top := []
	var bottom := []
	for part in u.units:
		if u.units[part] < 0:
			var text: String = "\\text{"+part+"}"
			if part in Global.characters:
				text = part
			if u.units[part] != -1:
				bottom.append(text+"^{"+str(-u.units[part])+"}")
			else:
				bottom.append(text)
		else:
			var text: String = "\\text{"+part+"}"
			if part in Global.characters:
				text = part
			if u.units[part] != 1:
				top.append(text+"^{"+str(u.units[part])+"}")
			else:
				top.append(text)
	var unit_string := ""
	if len(top) == 0:
		unit_string = "1"
	else:
		for i in range(len(top)):
			unit_string += top[i]
			if i < len(top)-1:
				unit_string += "\\cdot"
	if len(bottom) > 0:
		unit_string = "\\frac{"+unit_string+"}{"
		for i in range(len(bottom)):
			unit_string += bottom[i]
			if i < len(bottom)-1:
				unit_string += "\\cdot"
		unit_string += "}"
	if unit_string == "1":
		unit_string = ""
	return res+unit_string


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
