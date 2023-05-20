using Godot;
using System;
using System.Collections.Generic;
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

	public static Value To(Value value, Unit unit) {
		Unit si = unit.Clone();
		double factor = si.ConvertToSI();
		if(value.unit != si) throw new Exception("Units does not mach");
		value.unit = unit;
		value /= new Value(factor);
		return value;
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
		List<String> ends = new List<string>{"", "."+Data.delta1, "."+Data.delta2};
		if(variable.data.Contains(".")) ends.Add("."+variable.data.Split(".")[1]);
		while(progress) {
			progress = false;
			List<String> keys = new List<string>();
			foreach(String key in Data.find.Keys) keys.Add(key);
			foreach(String key in Data.variables.Keys) keys.Add(key);
			foreach(String v in keys) {
				String d1 = "";
				String d2 = "";
				String delta = "";
				if(v[0] == 'Δ') {
					delta = v;
					d1 = delta.Substring(1)+"."+Data.delta1;
					d2 = delta.Substring(1)+"."+Data.delta2;
				} else if(v.Contains(".") && v.Split(".")[1] == Data.delta1) {
					d1 = v;
					d2 = v.Split(".")[0]+"."+Data.delta2;
					delta = "Δ"+v.Split(".")[0];
				} else if(v.Contains(".") && v.Split(".")[1] == Data.delta2) {
					d1 = v.Split(".")[0]+"."+Data.delta1;
					d2 = v;
					delta = "Δ"+v.Split(".")[0];
				}
				int count = 0;
				if(HasVariable(d1)) count++;
				if(HasVariable(d2)) count++;
				if(HasVariable(delta)) count++;
				if(count != 2) continue;
				if(!HasVariable(d1)) Data.find[d1] = GetVariable(d2) - GetVariable(delta);
				if(!HasVariable(d2)) Data.find[d2] = GetVariable(d1) + GetVariable(delta);
				if(!HasVariable(delta)) Data.find[delta] = GetVariable(d2) - GetVariable(d1);
			}
			foreach(String formula in Data.formulas.Keys) {
				foreach(String end in ends) {
					int count = 0;
					String missing = "";
					bool used = false;
					foreach(String v in Data.formulas[formula].Item1.Keys) {
						if(HasVariable(v+end)) {
							count++;
							used = true;
						} else if(HasVariable(v)) count++;
						else missing = v;
					}
					if(count+1 == Data.formulas[formula].Item1.Count && used) {
						progress = true;
						foreach(String optional in Data.formulas[formula].Item2.Keys)
							if(!HasVariable(optional)) Data.find[optional] = GetValue(Data.formulas[formula].Item2[optional]);
						String form = Data.formulas[formula].Item1[missing];
						if(end != "") form = GetForm(form, end);
						Data.find[missing+end] = GetValue(form);
					}
				}
			}
		}
		if(!Data.find.ContainsKey(variable.data)) throw new Exception("Could not find variable");
		return Data.find[variable.data];
	}

	public static bool HasVariable(String variable) {
		return Data.variables.ContainsKey(variable) || Data.find.ContainsKey(variable);
	}

	public static Value GetVariable(String variable) {
		if(Data.variables.ContainsKey(variable)) return Data.variables[variable];
		if(Data.find.ContainsKey(variable)) return Data.find[variable];
		throw new Exception("Cant find variable");
	}

	public static Value GetValue(String calculation) {
		ParserRes result = Parsers.calculation(calculation);
		if(!result.succes) throw new Exception(result.error);
		if(result.rest != "") throw new Exception("Could not parse " + result.rest);
		return Evaluators.Calculation(result.tree);
	}

	public static String GetForm(String form, String end) {
		ParserRes result = Parsers.calculation(form);
		AddEnd(result.tree, end);
		return result.tree.Read();
	}

	public static void AddEnd(GrammarTree tree, String end) {
		if(tree.type == "word" && HasVariable(tree.data+end)) tree.data += end;
		foreach(GrammarTree child in tree.children) AddEnd(child, end);
	}


}
