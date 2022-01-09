using Godot;
using System;
using System.Collections.Generic;


public class Unit {
	Dictionary<String, int> parts = new Dictionary<string, int>();

	public Unit(String k = "", int p = 1) {
		if(k != "" && k != "1")  parts[k] = p;
	}

	public Unit(Dictionary<String, int> parts) {
		foreach(String key in parts.Keys) this.parts[key] = parts[key];
	}

	public Unit Clone() {
		return new Unit(parts);
	}

	public int Count() {
		int res = 0;
		foreach(int power in parts.Values) res += Math.Abs(power);
		return res;
	}

	public List<String> GetKeys() {
		List<String> res = new List<string>();
		foreach(String key in parts.Keys) res.Add(key);
		return res;
	}

	public String ToString() {
		String top = "";
		String bottom = "";
		foreach(String key in parts.Keys) {
			if(parts[key] > 0) {
				top += "*" + key;
				if(parts[key] > 1) top += "^" + parts[key].ToString();
			}
			else {
				bottom += "*" + key;
				if(parts[key] < -1) bottom += "^" + (-parts[key]).ToString();
			}
		}
		if(top != "") top = top.Substring(1);
		if(bottom != "") bottom = bottom.Substring(1);
		if(top == "") top = "1";
		if(bottom == "") return top;
		return top + "/" + bottom;
	}

	public String GetReadeable() {
		Unit res = Clone();
		foreach(String alias in Data.aliases) {
			while(res >= Data.conversions[alias].Item2) {
				res -= Data.conversions[alias].Item2;
				res.ChangeUnit(alias, 1);
			}
			while(res >= -Data.conversions[alias].Item2) {
				res += Data.conversions[alias].Item2;
				res.ChangeUnit(alias, -1);
			}
		}
		return res.ToString();
	}

	public void SetUnit(string unit, int power) {
		parts[unit] = power;
	}

	public int GetUnit(String unit) {
		if(!parts.ContainsKey(unit)) return 0;
		return parts[unit];
	}

	public void ChangeUnit(String unit, int power) {
		parts[unit] = power + GetUnit(unit);
	}

	public void RemoveUnit(string unit) {
		if(!parts.ContainsKey(unit)) return;
		parts.Remove(unit);
	}
		
	public void Simplify() {
		List<String> remove = new List<string>();
		foreach(String key in parts.Keys) {
			if(parts[key] == 0) remove.Add(key);
		}
		foreach(String key in remove) parts.Remove(key);
	}

	public double ConvertToSI() {
		double resFactor = 1;
		Unit unit = new Unit();
		foreach(String key in GetKeys()) {
			String label = key;
			int power = parts[key];
			if(!Data.conversions.ContainsKey(label) && label.Length > 1 && Data.prefixes.ContainsKey(label[0])) {
				resFactor *= Math.Pow(10, Data.prefixes[label[0]]*power);
				label = label.Substring(1);
				if(!Data.conversions.ContainsKey(label)) unit.ChangeUnit(label, power);
				parts[key] = 0;
			}
			if(Data.conversions.ContainsKey(label)) {
				Tuple<double, Unit> conversion = Data.conversions[label];
				//GD.Print(label, " ", unit.ToString(), " ", power, " ", conversion.Item3.ToString());
				//GD.Print((conversion.Item3*power).ToString(), " ", (unit + conversion.Item3*power).ToString());
				unit += conversion.Item2*power;
				resFactor *= conversion.Item1;
				parts[key] = 0;
			}

		}
		parts = (this+unit).parts;
		Simplify();
		return resFactor;
	}

	public void Debug() {
		GD.Print("Debug begin");
		foreach(String key in parts.Keys) GD.Print("   ", key, " ", parts[key]);
		GD.Print("Debug end");
	}

	public bool IsOne() {
		return parts.Count == 0;
	}

	public static Unit operator +(Unit a, Unit b) {
		Unit res = a.Clone();
		foreach(String key in b.parts.Keys) res.ChangeUnit(key, b.parts[key]);
		res.Simplify();
		return res;
	}

	public static Unit operator -(Unit a, Unit b) {
		return a + (-b);
	}

	public static Unit operator -(Unit a) {
		return (-1)*a;
	}

	public static Unit operator *(Unit a, int b) {
		Unit res = a.Clone();
		foreach(String key in res.GetKeys()) res.parts[key] *= b;
		return res;
	}

	public static Unit operator *(int a, Unit b) {
		return b*a;
	}

	public static Unit operator /(Unit a, int b) {
		Unit res = a.Clone();
		foreach(String key in res.GetKeys()) res.parts[key] /= b;
		return res;
	}

	public static bool operator ==(Unit a, Unit b) {
		foreach(String key in a.parts.Keys) {
			if(a.GetUnit(key) != b.GetUnit(key)) return false;
		}
		foreach(String key in b.parts.Keys) {
			if(a.GetUnit(key) != b.GetUnit(key)) return false;
		}
		return true;
	}

	public static bool operator !=(Unit a, Unit b) {
		return !(a == b);
	}

	public static bool operator <(Unit a, Unit b) {
		foreach(String key in a.parts.Keys) {
			int aPower = a.GetUnit(key); int bPower = b.GetUnit(key);
			if(Math.Sign(aPower) != Math.Sign(bPower) || Math.Abs(aPower) >= Math.Abs(bPower)) return false;
		}
		return true;
	}

	public static bool operator >(Unit a, Unit b) {
		return b < a;
	}

	public static bool operator <=(Unit a, Unit b) {
		foreach(String key in a.parts.Keys) {
			int aPower = a.GetUnit(key); int bPower = b.GetUnit(key);
			if(Math.Sign(aPower) != Math.Sign(bPower) || Math.Abs(aPower) > Math.Abs(bPower)) return false;
		}
		return true;
	}

	public static bool operator >=(Unit a, Unit b) {
		return b <= a;
	}
}

public struct Value {

	public double value;
	public Unit unit;

	public Value( double value, Unit unit) {
		this.value = value;
		this.unit = unit.Clone();
		ConvertToSI();
	}

	public void ConvertToSI() {
		value *= unit.ConvertToSI();
	}

	public Value(double value) {
		this.value = value;
		this.unit = new Unit();
	}

	public void SetUnit(Unit value) {
		unit = value.Clone();
		ConvertToSI();
	}

	public String ToString() {
		String unitString = " " + unit.GetReadeable();
		int power = (int)Math.Floor(Math.Log10(value));
		if(value == 0) power = 0;
		power -= power%3;
		String powerString = "*10^" + power.ToString();
		double factor = value/Math.Pow(10, power);
		if(power == 0) powerString = "";
		if(power == -3) {
			factor *= 0.001d;
			powerString = "";
		}
		if(power == 3) {
			factor *= 1000d;
			powerString = "";
		}
		if(unit.IsOne()) unitString = "";
		return (factor).ToString() + powerString + unitString;
	}

	public static Value operator +(Value a, Value b) {
		if(a.unit != b.unit) throw new Exception("Units do not match");
		return new Value(a.value+b.value, a.unit);
	}

	public static Value operator *(Value a, double b) {
		a.value *= b;
		return a;
	}

	public static Value operator *(double a, Value b) {
		return b * a;
	}

	public static Value operator -(Value a) {
		return (-1d)*a;
	}

	public static Value operator -(Value a, Value b) {
		return a +(-b);
	}

	public static Value operator *(Value a, Value b) {
		a.unit += b.unit;
		a.value *= b.value;
		return a;
	}

	public static Value operator /(Value a, Value b) {
		a.unit -= b.unit;
		a.value /= b.value;
		return a;
	}

	public static Value operator ^(Value a, Value b) {
		if(!b.unit.IsOne()) throw new Exception("Cant exponentiate with non one unit");
		if(a.unit.IsOne() && Math.Abs(b.value - Math.Floor(b.value)) > 1e-200)
			throw new Exception("Cant exponentiate non one unit in non integers");
		a.value = Math.Pow(a.value, Math.Floor(b.value));
		a.unit *= (int)b.value;
		return a;
	}

}
