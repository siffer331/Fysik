extends Control


func _init():
	_get_formulas()
	_get_symbols()
	_get_units()
	print("Units:")
	print(Data.units)
	print("Derived:")
	print(Data.derived)
	print("SI:")
	print(Data.si)
	print("Constants")
	print(Data.constants)
	print("Categories:")
	print(Data.categories)
	print("Symbols:")
	print(Data.symbols)
	print("Formulas")
	print(Data.formluas)


func _get_formulas() -> void:
	var formulas = _get_data("formulas")
	for formula in formulas:
		Data.formluas[formula] = {"categories": [], "versions": {}}
		for line in formulas[formula]:
			if line[0] == "#":
				line.erase(0,1)
				Data.formluas[formula].categories.append(line)
			else:
				var sides = line.split("=")
				Data.formluas[formula].versions[sides[0]] = sides[1]


func _get_units() -> void:
	var categories = _get_data("units")
	for line in categories["constants"]:
		line = line.split(" ")
		Data.constants[line[0]] = [num(line[2]), line[3]]
	categories.erase("constants")
	for category in categories:
		if category == "derived":
			continue
		Data.categories[category] = []
		for line in categories[category]:
			line = line.split(" ")
			if len(line) == 1:
				Data.categories[category].append(line[0])
				Data.units[line[0]] = [1, line[0]]
				Data.si.append(line[0])
				continue
			Data.categories[category].append(line[1])
			Data.units[line[1]] = [
				num(line[3])*Data.units[line[4]][0]/num(line[0]),
				Data.units[line[4]][1]
			]
	for line in categories["derived"]:
		line = line.split(" = ")
		Data.derived[line[0]] = Unit.new(line[1])


func _get_symbols() -> void:
	var symbols = _get_data("symbols")
	for symbol in symbols:
		Data.symbols[symbol] = [symbols[symbol][0], symbols[symbol][1].right(6)]


func _get_data(data_name: String) -> Dictionary:
	var file := File.new()
	file.open("res://Data/"+data_name, File.READ)
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
			res *= Data.constants[part][0]
	return res
