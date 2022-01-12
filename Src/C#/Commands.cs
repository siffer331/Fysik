using Godot;
using System;
using ParserRes = ParserCombinator.ParserRes;
using GrammarTree = ParserCombinator.GrammarTree;

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
		else if(value.value == Data.factors["PI"]/2) value.value = 1;
		else value.value = Math.Sin(value.value);
		value.unit = new Unit();
		return value;
	}

	public static Value Cos(Value value) {
		if(!value.unit.IsOne() && value.unit != new Unit("r")) throw new Exception("Unit " + value.unit.ToString() + " is not an angle");
		if(value.value == Data.factors["PI"]) value.value = 1;
		else if(value.value == Data.factors["PI"]/2) value.value = 0;
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

	public static Value Sqrt(Value value) {
		value.value = Math.Sqrt(value.value);
		value.unit /= 2;
		return value;
	}

	public static Value Log(Value value) {
		if(!value.unit.IsOne()) throw new Exception("Cant take log of value with unit");
		value.value = Math.Log10(value.value);
		return value;
	}

	public static Value Find(GrammarTree variable) {
		if(Data.variables.ContainsKey(variable.data)) return Data.variables[variable.data];
		bool progress = true;
		while(progress) {
			progress = false;
			foreach(String formula in Data.formulas.Keys) {
				int count = 0;
				String missing = "";
				foreach(String v in Data.formulas[formula].Item1.Keys) {
					if(HasVariable(v)) count++;
					else missing = v;
				}
				if(count+1 == Data.formulas[formula].Item1.Count) {
					progress = true;
					foreach(String optional in Data.formulas[formula].Item2.Keys)
						if(!HasVariable(optional)) Data.find[optional] = GetValue(Data.formulas[formula].Item2[optional]);
					Data.find[missing] = GetValue(Data.formulas[formula].Item1[missing]);
					if(missing == variable.data) break;
				}
			}
		}
		if(!Data.find.ContainsKey(variable.data)) throw new Exception("Could not find variable");
		return Data.find[variable.data];
	}

	public static bool HasVariable(String variable) {
		return Data.variables.ContainsKey(variable) || Data.find.ContainsKey(variable);
	}

	public static Value GetValue(String calculation) {
		ParserRes result = Parsers.calculation(calculation);
		if(!result.succes) throw new Exception(result.error);
		if(result.rest != "") throw new Exception("Could not parse " + result.rest);
		return Evaluators.Calculation(result.tree);
	}


}