using Godot;
using System;
using GrammarTree = ParserCombinator.GrammarTree;

public class Evaluators {

	public static Unit Unit(GrammarTree tree) {
		Unit res = new Unit("");
		switch(tree.type) {
			case "unit_divide":
				return Unit(tree.children.First.Value) -
				Unit(tree.children.First.Next.Next.Value);
			case "unit_multiply":
				foreach(GrammarTree child in tree.children) {
					if(child.data != "*") res += Unit(child);
				}
				return res;
			case "unit_exponent":
				return Unit(tree.children.First.Value) *
				int.Parse(tree.children.First.Next.Next.Value.data);
			case "word":
				return new Unit(tree.data);
		}
		throw new Exception("Could not evaluate type " + tree.type);
	}

	public static Value Calculation(GrammarTree tree) {
		Value res = new Value(0,1);
		switch(tree.type) {
			case "number":
				return new Value(0, int.Parse(tree.data));
			case "value_unit":
				res = Calculation(tree.children.First.Value);
				res.unit = Unit(tree.children.First.Next.Value);
				return res;
			case "add":
				bool add = true;
				bool first = true;
				foreach(GrammarTree child in tree.children) {
					if(first) {
						res = Calculation(child);
						first = false;
					}
					else if(child.data == "+") add = true;
					else if(child.data == "-") add = false;
					else if(add) res += Calculation(child);
					else res -= Calculation(child);
				}
				return res;
			case "multiply":
				res.value = 1;
				foreach(GrammarTree child in tree.children) {
					if(child.data != "*") res *= Calculation(child);
				}
				return res;
			case "divide":
				res = Calculation(tree.children.First.Value);
				Value divide = Calculation(tree.children.First.Next.Next.Value);
				return res/divide;
			case "negate":
				return -Calculation(tree.children.First.Next.Value);
			case "word":
				if(Data.variables.ContainsKey(tree.data)) return Data.variables[tree.data];
				if(Data.factors.ContainsKey(tree.data)) return new Value(0, Data.factors[tree.data]);
				throw new Exception("Could not find variable " + tree.data);
		}
		throw new Exception("Could not evaluate type " + tree.type);
	}
}

