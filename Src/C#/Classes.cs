using Godot;
using System;
using System.Collections.Generic;


public struct Unit {

	String[] units;
	int[] powers;

	public Unit(String k) {
		units = new String[0];
		powers = new int[0];
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
				if(powers[i] < -1) top += "^" + (-powers[i]).ToString();
			}
			top = top.Substring(1);
			bottom = bottom.Substring(1);
			if(top == "") top = "1";
		}
		if(bottom == "") return top;
		return top + "/" + bottom;
	}

	public void AddUnit(string unit, int power) {
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

	public void AddUnit(string unit) {
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

}

public class Value {

}
