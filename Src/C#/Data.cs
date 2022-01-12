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
	public static List<String> aliases = new List<string>();
	public static Dictionary<String, Value> constants = new Dictionary<string, Value>();
	public static Dictionary<String, Tuple<Dictionary<String, String>, Dictionary<String, String>>> formulas =
		new Dictionary<string, Tuple<Dictionary<string, string>, Dictionary<string, string>>>(); 
	public static Dictionary<String, Value> find = new Dictionary<string, Value>();

	public static void LoadLibraries() {
		LoadFactors();
		LoadUnits();
		LoadConstants();
		LoadFormulas();
		//foreach(String key in conversions.Keys) GD.Print(key, " ", conversions[key].Item1, " ", conversions[key].Item2, " ", conversions[key].Item3.ToString());
		SortAliases();
		//foreach(String a in aliases) GD.Print(a);
	}

	public static void SortAliases() {
		aliases.Sort((x, y) => conversions[y].Item2.Count() - conversions[x].Item2.Count());
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
				if(line == "") continue;
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
			foreach(String line in lines) {
				if(line == "") continue;
				String[] parts = line.Split(" ");
				String[] components = new String[] {"1", "", "1", ""};
				if(parts.Length == 3) {
					components[1] = parts[0]; components[3] = parts[2];
				} else if(parts.Length == 4) {
					if(parts[0] == "SI") {
						components[1] = parts[1]; components[3] = parts[3];
						aliases.Add(parts[1]);
					}
					else if(parts[0] == "pure") {
						double fac = GetFactor(parts[3]);
						conversions[parts[1]] = new Tuple<double, Unit>(fac, new Unit(parts[4]));
						continue;
					}
					else {components[1] = parts[0]; components[2] = parts[2]; components[3] = parts[3];}
				} else if(parts.Length == 5) {
					components[0] = parts[0]; components[1] = parts[1]; components[2] = parts[3]; components[3] = parts[4];
				} else if(parts.Length == 6) {
					double fac = GetFactor(parts[4])/GetFactor(parts[1]);
					conversions[parts[2]] = new Tuple<double, Unit>(fac, new Unit(parts[5]));
					continue;
				}

				//GD.Print(components[0], " ", components[1], " ", components[2], " ", components[3]);
				ParserCombinator.ParserRes res = Parsers.unit(components[3]);
				Unit unit = Evaluators.Unit(res.tree);
				double conversion = unit.ConvertToSI();
				double factor = GetFactor(components[2])*conversion/GetFactor(components[0]);
				//GD.Print(conversion.ToString(), " ", factor.ToString());
				conversions[components[1]] = new Tuple<double, Unit>(factor, unit);
			}
			fileName = directory.GetNext();
		}
		directory.ListDirEnd();
	}

	private static void LoadConstants() {
		Directory directory = new Directory();
		String dir = "res://Data/Constants/";
		directory.Open(dir);
		directory.ListDirBegin(true, true);
		String fileName = directory.GetNext();
		while(fileName != "") {
			File file = new File();
			file.Open(dir+fileName, File.ModeFlags.Read);
			String[] lines = file.GetAsText().Split("\n");
			foreach(String line in lines) {
				if(line == "") continue;
				String[] parts = line.Split(" ");
				constants[parts[0]] = new Value(double.Parse(parts[2]), Evaluators.Unit(Parsers.unit(parts[3]).tree));
			}
			fileName = directory.GetNext();
		}
		directory.ListDirEnd();
	}

	private static void LoadFormulas() {
		Directory directory = new Directory();
		String dir = "res://Data/Formulas/";
		directory.Open(dir);
		directory.ListDirBegin(true, true);
		String fileName = directory.GetNext();
		while(fileName != "") {
			File file = new File();
			file.Open(dir+fileName, File.ModeFlags.Read);
			String[] lines = file.GetAsText().Split("\n");
			String formula = "";
			foreach(String line in lines) {
				if(line == "") continue;
				if(line[0] == '[') {
					formula = line.Split("]")[0].Substring(1);
					formulas[formula] = new Tuple<Dictionary<string, string>, Dictionary<string, string>>(
						new Dictionary<String, String>(), new Dictionary<String, String>());
				} else if(line[0] == '#') continue;
				else if(line[0] == '$'){
					String[] parts = line.Substring(1).Split("=");
					parts[0].Trim();
					formulas[formula].Item2[parts[0]] = parts[1];
				} else {
					String[] parts = line.Split("=");
					parts[0].Trim();
					formulas[formula].Item1[parts[0]] = parts[1];
				}
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
