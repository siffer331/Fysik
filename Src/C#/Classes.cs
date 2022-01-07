using Godot;
using System;
using System.Collections.Generic;


public struct Unit {
	String[] units;
	int[] powers;

	public Unit(String k = "") {
		units = new String[0];
		powers = new int[0];
		if(k != "") SetUnit(k,1);
	}

	public Unit(Dictionary<String, int> parts) {
		int l = parts.Count;
		units = new String[l];
		powers = new int[l];
		int index = 0;
		foreach(String key in parts.Keys) {
			units[index] = key;
			powers[index] = parts[key];
			index++;
		}
	}

	public Dictionary<String, int> GetParts() {
		Dictionary<String, int> parts = new Dictionary<string, int>();
		for(int i = 0; i < units.Length; i++) parts[units[i]] = powers[i];
		return parts;
	}

	public String ToString() {
		String top = "";
		String bottom = "";
		for(int i = 0; i < units.Length; i++) {
			if(powers[i] > 0) {
				top += "*" + units[i];
				if(powers[i] > 1) top += "^" + powers[i].ToString();
			}
			else {
				bottom += "*" + units[i];
				if(powers[i] < -1) bottom += "^" + (-powers[i]).ToString();
			}
		}
		if(top != "") top = top.Substring(1);
		if(bottom != "") bottom = bottom.Substring(1);
		if(top == "") top = "1";
		if(bottom == "") return top;
		return top + "/" + bottom;
	}

	public void SetUnit(string unit, int power) {
		int l = units.Length;
		String[] newUnits = new String[l+1];
		int[] newPowers = new int[l+1];
		for(int i = 0; i < l; i++) {
			newUnits[i] = units[i];
			newPowers[i] = powers[i];
		}
		newUnits[l] = unit;
		newPowers[l] = power;
		units = newUnits;
		powers = newPowers;
	}

	public int GetUnit(String unit) {
		int index = Find(unit);
		if(index == -1) return 0;
		return powers[index];
	}

	public void ChangeUnit(String unit, int power) {
		SetUnit(unit, power + GetUnit(unit));
	}

	public void RemoveUnit(string unit) {
		int index = Find(unit);
		if(index == -1) return;
		int l = units.Length;
		String[] newUnits = new String[l-1];
		int[] newPowers = new int[l-1];
		for(int i = 0; i < l; i++) {
			if(i < index) {
				newUnits[i] = units[i];
				newPowers[i] = powers[i];
			}
			else {
				newUnits[i] = units[i+1];
				newPowers[i] = powers[i+1];
			}
		}
		units = newUnits;
		powers = newPowers;
	}
		

	public int Find(string unit) {
		for(int i = 0; i < units.Length; i++) {
			if(units[i] == unit) return i;
		}
		return -1;
	}

	public void Simplify() {
		int l = 0;
		for(int i = 0; i < powers.Length; i++) if(powers[i] != 0) l++;
		String[] newUnits = new String[l];
		int[] newPowers = new int[l];
		int index = 0;
		for(int i = 0; i < units.Length; i++) {
			if(powers[i] != 0) {
				newUnits[index] = units[i];
				newPowers[index] = powers[i];
				index++;
			}
		}
		units = newUnits;
		powers = newPowers;
	}

	public Tuple<double, int> ConvertToSI() {
		int resPower = 0;
		double resFactor = 1;
		Unit unit = new Unit();
		for(int i = 0; i < units.Length; i++) {
			String label = units[i];
			int power = powers[0];
			if(label.Length > 1 && Data.prefixes.ContainsKey(label[0])) {
				resPower += Data.prefixes[label[0]]*power;
				label = label.Substring(1);
				if(!Data.conversions.ContainsKey(label)) unit.ChangeUnit(label, power);
				powers[i] = 0;
			}
			if(Data.conversions.ContainsKey(label)) {
				Tuple<double, Unit> conversion = Data.conversions[label];
				unit += conversion.Item2;
				resFactor *= conversion.Item1;
				powers[i] = 0;
			}

		}
		this += unit;
		Simplify();
		return Tuple<double, int>(resPower, resFactor);
	}

	public void Debug() {
		GD.Print("Debug begin");
		for(int i = 0; i < units.Length; i++) GD.Print("  ", units[i], " ", powers[i]);
		GD.Print("Debug end");
	}

	public bool IsOne() {
		return units.Length == 0;
	}

	public static Unit operator +(Unit a, Unit b) {
		Dictionary<String, int> parts = new Dictionary<string, int>();
		for(int i = 0; i < a.units.Length; i++) parts[a.units[i]] = a.powers[i];
		for(int i = 0; i < b.units.Length; i++) {
			if(!parts.ContainsKey(b.units[i])) parts[b.units[i]] = 0;
			parts[b.units[i]] += b.powers[i];
		}
		Unit res = new Unit(parts);
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
		if(b == 0) return new Unit();
		for(int i = 0; i < a.units.Length; i++) a.powers[i] *= b;
		return a;
	}

	public static Unit operator *(int a, Unit b) {
		return b*a;
	}

	public static Unit operator /(Unit a, int b) {
		if(b == 0) throw new DivideByZeroException();
		for(int i = 0; i < a.units.Length; i++) a.powers[i] /= b;
		a.Simplify();
		return a;
	}

	public static bool operator ==(Unit a, Unit b) {
		Dictionary<String, int> aParts = a.GetParts();
		Dictionary<String, int> bParts = b.GetParts();
		if(aParts.Count != bParts.Count) return false;
		foreach(String key in aParts.Keys) {
			if(!bParts.ContainsKey(key) || bParts[key] != aParts[key]) return false;
		}
		return true;
	}

	public static bool operator !=(Unit a, Unit b) {
		return !(a == b);
	}

	public static bool operator <(Unit a, Unit b) {
		foreach(String key in a.units) if(b.GetUnit(key) != 0) return false;
		foreach(String key in b.units) {
			int aPower = a.GetUnit(key);
			int bPower = b.GetUnit(key);
			if(Math.Sign(aPower) != Math.Sign(bPower) || Math.Abs(aPower) >= Math.Abs(bPower)) return false;
		}
		return true;
	}

	public static bool operator >(Unit a, Unit b) {
		return b < a;
	}

	public static bool operator <=(Unit a, Unit b) {
		foreach(String key in a.units) if(b.GetUnit(key) != 0) return false;
		foreach(String key in b.units) {
			int aPower = a.GetUnit(key);
			int bPower = b.GetUnit(key);
			if(Math.Sign(aPower) != Math.Sign(bPower) || Math.Abs(aPower) > Math.Abs(bPower)) return false;
		}
		return true;
	}

	public static bool operator >=(Unit a, Unit b) {
		return b <= a;
	}
}

public struct Value {

	public int power;
	public double value;
	public Unit unit;

	public Value(int power, double value, Unit unit) {
		this.power = power;
		this.value = value;
		this.unit = unit;
		ConvertToSI();
	}

	public void ConvertToSI() {
		Tuple<double, int> factor = unit.ConvertToSI();
		value *= factor.Item1;
		power += factor.Item2;
		Simplify();
	}

	public Value(int power, double value) {
		this.power = power;
		this.value = value;
		this.unit = new Unit("");
		Simplify();
	}

	public void Simplify() {
		int change = power%3;
		power -= change;
		value *= Math.Pow(10d, change);

		while(Math.Abs(value) >= 1000d) {
			value /= 1000d;
			power += 3;
		}
		while(Math.Abs(value) < 1d && Math.Abs(value) > 0d) {
			value *= 1000;
			power -= 3;
		}
	}

	public double GetValue() {
		return value*Math.Pow(10d, power);
	}

	public String ToString() {
		String unitString = " " + unit.ToString();
		String powerString = "*10^" + power.ToString();
		double factor = 1d;
		if(power == 0) powerString = "";
		if(power == -3) {
			factor = 0.001d;
			powerString = "";
		}
		if(unit.IsOne()) unitString = "";
		return (value*factor).ToString() + powerString + unitString;
	}

	public static Value operator +(Value a, Value b) {
		if(a.unit != b.unit) throw new Exception("Units do not match");
		double value = 0;
		if(a.power > b.power) value = a.value + b.value*Math.Pow(10d,b.power-a.power);
		else value = b.value + a.value*Math.Pow(10d,a.power-b.power);
		return new Value(Math.Max(a.power, b.power), value, a.unit);
	}

	public static Value operator *(Value a, double b) {
		a.value *= b;
		a.Simplify();
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
		a.power += b.power;
		a.value *= b.value;
		a.Simplify();
		return a;
	}

	public static Value operator /(Value a, Value b) {
		if(b.value == 0) throw new DivideByZeroException();
		a.unit -= b.unit;
		a.power -= b.power;
		a.value /= b.value;
		a.Simplify();
		return a;
	}

	public static Value operator ^(Value a, Value b) {
		double value = b.GetValue();
		if(!b.unit.IsOne()) throw new Exception("Cant exponentiate with non one unit");
		if(a.unit.IsOne() && (b.value < 0d || (value - Math.Floor(value) > 0.0001) || b.power < 0))
			throw new Exception("Cant exponentiate non one unit in non positive integers");
		a.value = Math.Pow(a.value, Math.Floor(value));
		a.power *= (int)value;
		a.unit *= (int)value;
		return a;
	}

}
