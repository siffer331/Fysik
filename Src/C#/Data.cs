using Godot;
using System;
using System.Collections.Generic;

public static class Data {

	public static Dictionary<String, Value> variables = new Dictionary<string, Value>();
	public static Dictionary<String, double> factors = new Dictionary<string, double>();
	public static Dictionary<String, Tuple<double, Unit>> conversions = new Dictionary<string, Tuple<double, Unit>>();
	public static Dictionary<char, int> prefixes = new Dictionary<char, int>() {
		{'Y', 24}, {'Z', 21}, {'E', 18}, {'P', 15}, {'T', 12}, {'G',  9}, {'M',  6}, {'k',  3}, {'h',  2}, {'D',  1},
		{'d', -1}, {'c', -2}, {'m', -3}, {'μ', -6}, {'n', -9}, {'p',-12}, {'f',-15}, {'a',-18}, {'z',-21}, {'y',-24}
	};

	public static void LoadLibraries() {
		LoadFactors();
		LoadUnits();
	}

	private static void LoadFactors() {
		Directory directory = new Directory();
		String dir = "res://Data/Factors/";
		directory.Open(dir);
		directory.ListDirBegin(true, true);
		String fileName = directory.GetNext();
		while(fileName != "") {
			File file = new File();
			file.Open(dir+fileName, File.ModeFlags.Read);
			String[] lines = file.GetAsText().Split("\n");
			foreach(String line in lines) {
				String[] parts = line.Split(" ");
				factors[parts[0]] = double.Parse(parts[2]);
			}
			fileName = directory.GetNext();
		}
		directory.ListDirEnd();
	}

	private static void LoadUnits() {
		Directory directory = new Directory();
		String dir = "res://Data/Units/";
		directory.Open(dir);
		directory.ListDirBegin(true, true);
		String fileName = directory.GetNext();
		while(fileName != "") {
			File file = new File();
			file.Open(dir+fileName, File.ModeFlags.Read);
			String[] lines = file.GetAsText().Split("\n");
			int num = 0;
			foreach(String line in lines) {
				if(line == "") return;
				if(line[0] == '[' ) num = 0;
				else if(num == 1) {
					if(line.Contains(" ")) {
						String[] parts = line.Split(" ");
						conversions[parts[0]] = new Tuple<double, Unit>(Math.Pow(10, (double)prefixes[parts[1][0]]), new Unit(parts[1]+parts[0]));
					}
				}
				else {
					String[] parts = line.Split(" ");
					double factor = GetFactor(parts[3])/GetFactor(parts[0]);
					Unit unit = new Unit(parts[4]);
					if(conversions.ContainsKey(parts[4])) {
						factor *= conversions[parts[4]].Item1;
						unit = conversions[parts[4]].Item2;
					}
					conversions[parts[1]] = new Tuple<double, Unit>(factor, unit);
				}
				num++;
			}
			fileName = directory.GetNext();
		}
		directory.ListDirEnd();
	}

	private static double GetFactor(String s) {
		if(factors.ContainsKey(s)) return factors[s];
		return double.Parse(s);
	}

}
