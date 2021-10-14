class_name Code
extends Control

const types = ['*+-/^', '1234567890.', '=', '()', '[]']
const COMMANDS = {
	"dealloc" : {
		"return": "null",
		"args": 1,
	},
	"to" : {
		"return": "Value",
		"args": 2,
	},
	"sin" : {
		"return": "Value",
		"args": 1,
	},
	"tan" : {
		"return": "Value",
		"args": 1,
	},
	"cos" : {
		"return": "Value",
		"args": 1,
	},
	"sqrt" : {
		"return": "Value",
		"args": 1,
	},
	"sq" : {
		"return": "Value",
		"args": 1,
	},
	"find" : {
		"return": "Value",
		"args": 1,
	},
}

enum TYPES{EMPTY = -2, WORD = -1, OPERATION, VALUE, EQUALS, PARENTHETHIES, UNIT, SUCCESS}

export var code_path: NodePath

var undo := false

onready var code: TextEdit = get_node(code_path)


func _ready() -> void:
	Data.connect("write", self, "_on_Data_write")
	code = code as TextEdit
	code.add_color_region("///", "", Color(.6,.3,.3))
	code.add_color_region("//", "", Color(.4,.4,.8))
	code.add_color_region("#", "", Color(.4,.4,.4))
	code.add_color_region("%", "", Color(.4,.7,.4))
	code.add_color_region("[", "]", Color(.4,.5,.8))
	for constant in Data.constants:
		code.add_keyword_color(constant, Color(.7,.7,.9))
	for formula in Data.formulas:
		code.add_keyword_color(formula, Color(.7,.5,.9))
	for command in COMMANDS:
		code.add_keyword_color(command, Color(.3,.7,.7))


func _input(event: InputEvent) -> void:
	code = code as TextEdit
	if event.is_action_pressed("run line"):
		undo = true
		var line_num = code.cursor_get_line()
		var line = code.get_line(line_num)
		if line != "" and (not line[0] in "#%") and line.substr(0,2) != "//":
			var res = eval(line)
			code.cursor_set_column(len(line))
			if (
				line_num < code.get_line_count()-1 and
				code.get_line(line_num+1).substr(0,2) == "//"
			):
				var index = 0
				var lines = -2
				for line_a in code.text.split("\n"):
					lines += 1
					if lines == line_num:
						break
					index += len(line_a) + 1
				var temp := code.text
				temp.erase(index, len(code.get_line(line_num+1))+1)
				code.text = temp
				code.cursor_set_line(line_num)
				code.cursor_set_column(len(line))
			if line[len(line)-1] != ":":
				code.insert_text_at_cursor("\n//" + res[0])


func _get_parts(line: String) -> Array:
	var parts := []
	var part = ""
	var type = TYPES.EMPTY
	var p1 = 0
	var p2 = 0
	line = line.split("#")[0]
	for c in line:
		if c in " 	:":
			continue
		var c_type = TYPES.WORD
		if p1 != 0 or p2 != 0:
			c_type = type
		if c in '()':
			p1 += '( )'.find(c) - 1
		if c in '[]':
			p2 += '[ ]'.find(c) - 1
		if c_type == TYPES.WORD:
			for i in range(len(types)):
				if c in types[i]:
					c_type = i
					break
		if len(part) > 0:
			if type == TYPES.VALUE and (
				(part[len(part)-1] == "1" and c == "e") or
				(part[len(part)-1] == "e" and c == "-")
			):
				c_type = TYPES.VALUE
		if type == TYPES.WORD and c_type == TYPES.VALUE:
			c_type = TYPES.WORD
		if c_type == TYPES.WORD and type == TYPES.VALUE:
			type = TYPES.WORD
		if type == c_type:
			part += c
		else:
			if type != TYPES.EMPTY:
				parts.append([part, type])
			part = c
			type = c_type
	parts.append([part, type])
	if p1 != 0 or p2 != 0:
		return ["error", "/unbalanced parenthetthies"]
	return [parts]


func eval(line: String) -> Array:
	var parts = _get_parts(line)
	if parts[0] is String and parts[0] == "error":
		return [parts[1]]
	parts = parts[0]
	if len(parts) > 2:
		for i in range(len(parts)-1):
			if parts[i][1] == TYPES.WORD and parts[i+1][1] == TYPES.PARENTHETHIES:
				if (
					parts[i][0] in COMMANDS and
					COMMANDS[parts[i][0]]["return"] == "null"
				):
					return ["/Misplaced command " + parts[i][0]]
	var index = 0
	while index < len(parts):
		if parts[index][1] == TYPES.VALUE:
			var value = parts[index][0]
			var unit := "[]"
			if index < len(parts) - 1 and parts[index+1][1] == TYPES.UNIT:
				unit = parts[index+1][0]
				parts.remove(index+1)
			parts[index][0] = Value.new(value, unit)
		index += 1
	index = 0
	while index < len(parts):
		if index == 0 and len(parts) > 1 and parts[1][1] == TYPES.EQUALS:
			index += 1
		if parts[index][1] == TYPES.WORD:
			if index != len(parts) - 1 and parts[index+1][1] == TYPES.PARENTHETHIES:
				if parts[index][0] in COMMANDS or parts[index][0] in Data.formulas:
					var args = parts[index+1][0].substr(1,len(parts[index+1][0])-2)
					var res = command(parts[index][0], args)
					if res[0] is String and res[0] == "error":
						return [res[1]]
					else:
						parts.remove(index+1)
						if res[0][1] == TYPES.EMPTY:
							parts.remove(index)
							continue
						if res[0][1] == TYPES.SUCCESS:
							return res[0][0]
						parts[index] = res[0]
				else:
					return ["/Cant find command " + parts[index][0]]
			else:
				if parts[index][0] in Data.variables:
					parts[index] = [Data.variables[parts[index][0]], TYPES.VALUE]
				elif parts[index][0] in Data.constants:
					parts[index] = [Data.constants[parts[index][0]], TYPES.VALUE]
				else:
					return ["/Cant find variable " + parts[index][0]]
		index += 1
	for i in range(len(parts)):
		if parts[i][1] == TYPES.PARENTHETHIES:
			var res = eval(parts[i][0].substr(1,len(parts[i][0])-2))
			if len(res) == 1:
				return res
			parts[i] = res[1]
	for operation in "^*/+-":
		var error = operate(parts, operation)
		if error != "":
			return [error]
	if len(parts) > 1 and parts[1][1] == TYPES.EQUALS:
		if parts[0][1] != TYPES.WORD:
			return ["/Cant set " + str(parts[0][0])]
		if len(parts) == 2:
			return ["/Cant set variable to empty"]
		if parts[2][1] != TYPES.VALUE:
			return ["/Cant set variable to " + parts[2][0]]
		Data.set_variable(parts[0][0], parts[2][0])
		code.add_keyword_color(parts[0][0], Color(.7,.9,.7))
		return [parts[0][0] + " has been set to " + str(parts[2][0]), parts[2][0]]
	if len(parts) == 0:
		return ["Succesfull", ["", TYPES.EMPTY]]
	return ["result: " + str(parts[0][0]), parts[0]]


func operate(parts: Array, operation: String) -> String:
	var i = 0
	while i < len(parts):
		if parts[i][1] == 0 and parts[i][0] == operation:
			if parts[i+1][1] != 1:
				return "/Cant convert '{part}' to a number".format({"part":parts[i+1][0]})
			if i == 0:
				if operation == "-":
					parts[1][0] = A.multiply_v(Value.new("-1"),parts[1][0])
					parts.remove(0)
					continue
				return "/cant operate on nothing"
			if parts[i-1][1] != 1:
				return "/Cant convert '{part}' to a number".format({"part":parts[i-1][0]})
			match(operation):
				'^':
					parts[i][0] = A.power(parts[i-1][0],parts[i+1][0])
				'*':
					parts[i][0] = A.multiply_v(parts[i-1][0],parts[i+1][0])
				'/':
					parts[i][0] = A.divide_v(parts[i-1][0],parts[i+1][0])
				'+':
					parts[i][0] = A.add(parts[i-1][0],parts[i+1][0])
				'-':
					print(parts)
					parts[i][0] = A.subtract(parts[i-1][0],parts[i+1][0])
			if parts[i][0] is String:
				return parts[i][0]
			parts[i][1] = 1
			parts.remove(i-1)
			parts.remove(i)
			i -= 1
		else:
			i += 1
	return ""


func formula(command: String, raw_args: String) -> Array:
	var args := raw_args.split(",")
	if raw_args == "":
		return ["error", "/Missing arguments in " + command]
	var replace := {}
	var arg_1_parts = _get_parts(args[0])
	if len(arg_1_parts) > 1:
		return ["error", "/Invalid keyword {keyword} in {formula}".format(
			{"keyword": args[0], "formula": command}
		)]
	var type = arg_1_parts[0][0][0]
	args.remove(0)
	for arg in args:
		if len(arg.split("=")) != 2:
			return ["error", "/Invalid arg {arg} in {formula}".format(
				{"arg": arg, "formula": command}
			)]
		arg = arg.split("=")
		var parts = _get_parts(arg[0])
		if len(parts) > 1:
			return ["error", "/Invalid keyword {keyword} in {formula}".format(
				{"keyword": arg[0], "formula": command}
			)]
		var keyword = parts[0][0][0]
		replace[keyword] = arg[1]
	if not type in Data.formulas[command]["versions"]:
		return ["error", "/Cant get " + type]
	var formula: String = Data.formulas[command]["versions"][type]
	for arg in replace:
		formula = formula.replace(arg, "("+replace[arg]+")")
	var res = eval(formula)
	if len(res) == 1:
		return ["error", res[0]]
	return [res[1]]


func command(command: String, raw_args: String) -> Array:
	if command in Data.formulas:
		return formula(command, raw_args)
	if COMMANDS[command].args > 0 and raw_args == "":
		return ["error", "no input recieved"]
	var args := raw_args.split(",")
	if len(args) != COMMANDS[command].args:
		return ["error", "wrong number of args"]
	return Commands.call(command, args, self)


func _on_TextEdit_text_changed() -> void:
	if undo:
		code.undo()
		undo = false


func _on_Data_write(text: String) -> void:
	code.insert_text_at_cursor(text)
