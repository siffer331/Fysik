class_name Main
extends Control


func _init():
	_get_formulas()
	_get_symbols()
	_get_units()
	#print("Units:")
	#print(Data.units)
	#print("Derived:")
	#print(Data.derived)
	#print("SI:")
	#print(Data.si)
	#print("Constants")
	#print(Data.constants)
	#print("Categories:")
	#print(Data.categories)
	#print("Symbols:")
	#print(Data.symbols)
	#print("Formulas")
	#print(Data.formulas)
	#print("Order")
	#print(Data.derived_order)

class DerivedSorter:
	static func comparator(a: String, b: String) -> bool:
		var a_count := 0
		var b_count := 0
		for unit in Data.derived[a].units:
			a_count += abs(Data.derived[a].units[unit])
		for unit in Data.derived[b].units:
			b_count += abs(Data.derived[b].units[unit])
		return a_count > b_count


func _get_formulas() -> void:
	var formulas = _get_data("formulas")
	for formula in formulas:
		Data.formulas[formula] = {"categories": [], "versions": {}, "defaults": {}}
		for line in formulas[formula]:
			if line[0] == "#":
				line.erase(0,1)
				Data.formulas[formula].categories.append(line)
			elif line[0] == "$":
				line.erase(0,1)
				var sides: Array = line.split("=")
				var parts: Array = sides[1].split(" ")
				Data.formulas[formula].defaults[sides[0]] = Value.new(parts[0], "["+parts[1]+"]")
			else:
				var sides = line.split("=")
				Data.formulas[formula].versions[sides[0]] = sides[1]


func _get_units() -> void:
	var categories = _get_data("units")
	for line in categories["constants"]:
		line = line.split(" ")
		Data.constants[line[0]] = Value.new(line[2])
	for category in categories:
		if category in ["derived", "constants"]:
			continue
		Data.categories[category] = []
		for i in range(len(categories[category])):
			var line: Array = categories[category][i].split(" ")
			if i == 0:
				Data.categories[category].append(line[0])
				Data.units[line[0]] = [1, line[0]]
				Data.si.append(line[0])
			elif i == 1:
				Data.scaleable.append(line[0])
			else:
				Data.categories[category].append(line[1])
				Data.units[line[1]] = [
					num(line[3])*Data.units[line[4]][0]/num(line[0]),
					Data.units[line[4]][1]
				]
	for line in categories["derived"]:
		line = line.split(" = ")
		Data.derived[line[0]] = UA.derive(Unit.new(line[1]))
		Data.scaleable.append(line[0])
		Data.derived_order.append(line[0])
	Data.derived_order.sort_custom(DerivedSorter, "comparator")
	Data.constants.clear()
	for line in categories["constants"]:
		line = line.split(" ")
		Data.constants[line[0]] = Value.new(line[2], "["+line[3]+"]")
		Data.constants[line[0]]


func _get_symbols() -> void:
	var symbols = _get_data("symbols")
	for symbol in symbols:
		Data.symbols[symbol] = [symbols[symbol][0], symbols[symbol][1].right(6)]


func _get_data(data_name: String) -> Dictionary:
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
		elif part in Data.constants:
			res *= Data.constants[part].get_value()
	return res
