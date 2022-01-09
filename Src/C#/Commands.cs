using Godot;
using System;

public static class Commands {

	public static String Dealloc(String variable) {
		if(!Data.variables.ContainsKey(variable)) throw new Exception("variabl " + variable + " does not exist");
		Data.variables.Remove(variable);
		return "Succes";
	}

	public static String Reset() {
		Data.variables.Clear();
		return "Succes";
	}

	public static String To(Value value, Unit unit) {
		Unit si = unit.Clone();
		double factor = si.ConvertToSI();
		if(value.unit != si) throw new Exception("Units does not mach");
		value.unit = unit;
		value /= new Value(factor);
		return value.ToString();
	}

	public static Value Sin(Value value) {
		if(!value.unit.IsOne() && value.unit != new Unit("r")) throw new Exception("Unit " + value.unit.ToString() + " is not an angle");
		if(value.value == Data.factors["PI"]) value.value = 0;
		else value.value = Math.Sin(value.value);
		value.unit = new Unit();
		return value;
	}

	public static Value Cos(Value value) {
		if(!value.unit.IsOne() && value.unit != new Unit("r")) throw new Exception("Unit " + value.unit.ToString() + " is not an angle");
		if(value.value == Data.factors["PI"]) value.value = 1;
		else value.value = Math.Cos(value.value);
		value.unit = new Unit();
		return value;
	}

	public static Value Tan(Value value) {
		if(!value.unit.IsOne() && value.unit != new Unit("r")) throw new Exception("Unit " + value.unit.ToString() + " is not an angle");
		if(value.value == Data.factors["PI"]) value.value = 0;
		else value.value = Math.Tan(value.value);
		value.unit = new Unit();
		return value;
	}

}