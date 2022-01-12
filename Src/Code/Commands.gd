class_name Command
extends Node

#return ["error", "/oh no"]

func dealloc(args: Array, code: Code, exporting := false) -> Array:
	if not args[0] in Data.variables:
		return ["error", "variable does not excist"]
	Data.variables.erase(args[0])
	Data.emit_signal("variable_changed")
	return [[["Succesfull", ["", Code.TYPES.EMPTY]], Code.TYPES.SUCCESS]]


func reset(args: Array, code: Code, exporting := false) -> Array:
	Data.variables.clear()
	Data.emit_signal("variable_changed")
	return [[["Succesfull", ["", Code.TYPES.EMPTY]], Code.TYPES.SUCCESS]]


func to(args: Array, code: Code, exporting := false) -> Array:
	var unit := Unit.new(args[1])
	var si := A.copy(unit)
	var factor := si.convert_to_si() 
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value := A.copy_value(res[1][0])
	if not UA.equals(value.unit, si):
		return ["error", "the units are no the same"]
	value.value /= factor[0]
	value.p -= factor[1]
	value.unit = unit
	value.simplify()
	if exporting:
		Data.exported.append("$"+res[1][0].to_latex()+"="+value.to_latex()+"$")
	return [[value, Code.TYPES.VALUE]]


func sin(args: Array, code: Code, exporting := false) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = sin(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func tan(args: Array, code: Code) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = tan(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func cos(args: Array, code: Code, exporting := false) -> Array:
	var res := code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = cos(value.value)
	if UA.equals(value.unit, Unit.new("r")):
		value.unit = Unit.new("")
	return [[value, Code.TYPES.VALUE]]


func sqrt(args: Array, code: Code, exporting := false) -> Array:
	var res = code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	if value.p%2 == 1:
		value.value *= 10
	value.value = sqrt(value.value)
	value.p = sign(value.p)*int(abs(value.p/2))
	value.simplify()
	for unit in value.unit.units:
		value.unit.units[unit] /= 2
	return [[value, Code.TYPES.VALUE]]


func sq(args: Array, code: Code, exporting := false) -> Array:
	var res = code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = value.value*value.value
	value.p *= 2
	value.simplify()
	for unit in value.unit.units:
		value.unit.units[unit] *= 2
	return [[value, Code.TYPES.VALUE]]


func ln(args: Array, code: Code, exporting := false) -> Array:
	var res = code.eval(args[0])
	if len(res) == 1:
		return ["error", res[0]]
	var value = res[1][0]
	value.value = log(value.value) + log(10)*value.p
	value.p = 0
	value.simplify()
	return [[value, Code.TYPES.VALUE]]


func find(args: Array, code: Code, exporting := false) -> Array:
	var found := {}
	var order := []
	var target: String = args[0]
	for v in Data.variables:
		found[v] = v
	var changed := true
	while changed:
		changed = false
		for formula in Data.formulas:
			var count := 0
			for v in Data.formulas[formula].versions:
				if v in found or v in Data.formulas[formula].defaults:
					count += 1
			if count == len(Data.formulas[formula].versions)-1:
				changed = true
				for v in Data.formulas[formula].versions:
					if v in Data.formulas[formula].defaults:
						Data.defaults[v] = Data.formulas[formula].defaults[v]
					if not v in found:
						found[v] = Data.formulas[formula].versions[v]
						order.append(v)
		if target in found:
			break
	var res := ""
	if target in found:
		var calc := " "+target+" "
		order.invert()
		var used := []
		for v in order:
			if v in calc:
				used.append(v)
			calc = calc.replace(" "+v+" ", "("+found[v]+" )")
		used.invert()
		for v in used:
			Data.exported.append("$"+v+"="+Code.equation_to_latex(found[v])+"$")
			for u in used:
				if u == v:
					break
				found[v] = found[v].replace(" "+u+" ", "("+found[u]+" )")
			var evaled := code.eval(found[v])
			if len(evaled) > 1:
				Data.exported.append("$"+v+"="+evaled[1][0].to_latex()+"$")
		var evaled := code.eval(calc)
		if len(evaled) < 1:
			return ["error", "Can not find " + target]
		res = target + " has been found to be " + str(evaled[1][0])
		Data.defaults.clear()
		return [[evaled[1][0], Code.TYPES.VALUE]]
	else:
		return ["error", "Can not find " + target]
