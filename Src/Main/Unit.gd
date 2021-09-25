class_name Unit
extends Object

var units := {}

func _init(name := "") -> void:
	if name == "1":
		name = ""
	var parts = name.split("/")
	for part in parts[0].split("*"):
		if part == "":
			continue
		part = part.split("^")
		if not part[0] in units:
			units[part[0]] = 0
		var val = 1
		if len(part) > 1:
			val = int(part[1])
		units[part[0]] += val
	if len(parts) > 1:
		for part in parts[1].split("*"):
			if part == "":
				continue
			part = part.split("^")
			if not part[0] in units:
				units[part[0]] = 0
			var val = 1
			if len(part) > 1:
				val = int(part[1])
			units[part[0]] -= val
	_simplify()


func _to_string() -> String:
	var top = ""
	var bot = ""
	for part in units:
		if units[part] > 0:
			top += part
			if units[part] > 1:
				top += "^" + str(units[part])
			top += "*"
		else:
			bot += part
			if units[part] < -1:
				bot += "^" + str(-units[part])
			bot += "*"
	if top == "":
		top = "1"
	else:
		top.erase(len(top)-1,1)
	if bot != "":
		bot.erase(len(bot)-1,1)
		return top + "/" + bot
	return top


func convert_to_si() -> float:
	var factor = 1
	for unit in units.keys():
		if unit in Data.units:
			factor *= pow(Data.units[unit][0], units[unit])
			var si = Data.units[unit][1]
			if si != unit:
				if not si in units:
					units[si] = 0
				units[si] += units[unit]
				units.erase(unit)
		if unit in Data.derived:
			for d_unit in Data.derived[unit].units:
				if not d_unit in units:
					units[d_unit] = 0
				units[d_unit] += Data.derived[unit].units[d_unit]
				units.erase(unit)
	_simplify()
	return factor


func _simplify() -> void:
	for part in units.keys():
		if units[part] == 0:
			units.erase(part)
	if "1" in units:
		units.erase("1")


