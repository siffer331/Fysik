class_name Main
extends Control


func _init():
	print("test")
	_get_formulas()
	_get_symbols()
	_get_units()
	#print("Units:")
	#print(Global.units)
	#print("Derived:")
	#print(Global.derived)
	#print("SI:")
	#print(Global.si)
	#print("Constants")
	#print(Global.constants)
	#print("Categories:")
	#print(Global.categories)
	#print("Symbols:")
	#print(Global.symbols)
	#print("Formulas")
	#print(Global.formulas)
	#print("Order")
	print(Global.derived_order)

class DerivedSorter:
	static func comparator(a: String, b: String) -> bool:
		return UA.size(a) > UA.size(b)


func _get_formulas() -> void:
	var formulas = _get_Global("formulas")
	for formula in formulas:
		Global.formulas[formula] = {"categories": [], "versions": {}, "defaults": {}}
		for line in formulas[formula]:
			if line[0] == "#":
				line.erase(0,1)
				Global.formulas[formula].categories.append(line)
			elif line[0] == "$":
				line.erase(0,1)
				var sides: Array = line.split("=")
				var parts: Array = sides[1].split(" ")
				Global.formulas[formula].defaults[sides[0]] = Value.new(parts[0], "["+parts[1]+"]")
			else:
				var sides = line.split("=")
				Global.formulas[formula].versions[sides[0]] = sides[1]


func _get_units() -> void:
	var categories = _get_Global("units")
	for line in categories["constants"]:
		line = line.split(" ")
		Global.constants[line[0]] = Value.new(line[2])
	for category in categories:
		if category in ["derived", "constants"]:
			continue
		Global.categories[category] = []
		for i in range(len(categories[category])):
			var line: Array = categories[category][i].split(" ")
			if i == 0:
				Global.categories[category].append(line[0])
				Global.units[line[0]] = [1, line[0]]
				Global.si.append(line[0])
			elif i == 1:
				Global.scaleable.append(line[0])
			else:
				Global.categories[category].append(line[1])
				Global.units[line[1]] = [
					num(line[3])*Global.units[line[4]][0]/num(line[0]),
					Global.units[line[4]][1]
				]
	for line in categories["derived"]:
		line = line.split(" = ")
		Global.derived[line[0]] = UA.derive(Unit.new(line[1]))
		Global.scaleable.append(line[0])
		Global.derived_order.append(line[0])
	Global.derived_order.sort_custom(DerivedSorter, "comparator")
	Global.constants.clear()
	for line in categories["constants"]:
		line = line.split(" ")
		Global.constants[line[0]] = Value.new(line[2], "["+line[3]+"]")
		Global.constants[line[0]]


func _get_symbols() -> void:
	var symbols = _get_Global("symbols")
	for symbol in symbols:
		Global.symbols[symbol] = [symbols[symbol][0], symbols[symbol][1].right(6)]


func _get_Global(data_name: String) -> Dictionary:
	var file := File.new()
	file.open("res://Data/"+data_name+".config", File.READ)
	var content := file.get_as_text()
	file.close()
	var res := {}
	var category = ""
	for line in content.split("\n"):
		if line == "":
			continue
		if line[0] == "[":
			category = line.substr(1,len(line)-2)
			res[category] = []
		else:
			res[category].append(line)
	return res


func num(s: String) -> float:
	if s.is_valid_float():
		return float(s)
	var res = 1
	for part in s.split("*"):
		if part.is_valid_float():
			res *= float(part)
		elif part in Global.constants:
			res *= Global.constants[part].get_value()
	return res
