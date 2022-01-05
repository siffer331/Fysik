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
				res.SetUnit(tree.data, 1);
				return res;
		}
		throw new Exception("Could not evaluate type " + tree.type);
	}

	public static int Calculation(GrammarTree tree) {
		int res = 0;
		switch(tree.type) {
			case "number":
				return int.Parse(tree.data);
			case "calculation":
				return Calculation(tree.children.First.Next.Value);
			case "add":
				res = 0;
				bool add = true;
				foreach(GrammarTree child in tree.children) {
				if(child.data == "+") add = true;
				else if(child.data == "-") add = false;
				else if(add) res += Calculation(child);
				else res -= Calculation(child);
				}
				return res;
			case "multiply":
				res = 1;
				foreach(GrammarTree child in tree.children) {
				if(child.data != "*") res *= Calculation(child);
				}
				return res;
			case "divide":
				res = Calculation(tree.children.First.Value);
				int divide = Calculation(tree.children.First.Next.Next.Value);
				if(divide == 0) return 0;
				return res/divide;
			case "negate":
				return -Calculation(tree.children.First.Next.Value);
		}
		throw new Exception("Could not evaluate type " + tree.type);
	}
}

