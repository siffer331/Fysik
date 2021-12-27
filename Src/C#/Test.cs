using Godot;
using System;

using GrammarTree = ParserCombinator.GrammarTree;
using ParserRes = ParserCombinator.ParserRes;

public class Test : Control {

    public override void _Ready() {
        int Evaluate(GrammarTree tree) {
            int res = 0;
            switch(tree.type) {
                case "number":
                    return int.Parse(tree.data);
                case "calculation":
                    return Evaluate(tree.children.First.Next.Value);
                case "add":
                    res = 0;
                    bool add = true;
                    foreach(GrammarTree child in tree.children) {
                        if(child.data == "+") add = true;
                        else if(child.data == "-") add = false;
                        else if(add) res += Evaluate(child);
                        else res -= Evaluate(child);
                    }
                    return res;
                case "multiply":
                    res = 1;
                    foreach(GrammarTree child in tree.children) {
                        if(child.data != "*") res *= Evaluate(child);
                    }
                    return res;
                case "divide":
                    res = Evaluate(tree.children.First.Value);
                    int divide = Evaluate(tree.children.First.Next.Next.Value);
                    if(divide == 0) return 0;
                    return res/divide;
            }
            GD.Print("Error ", tree.type, " ", tree.data);
            return 0;
        }

        void Run(String s) {
            ParserRes result = Parsers.Calculation(s);
            if(!result.succes) GD.Print(result.ToString());
            else GD.Print(Evaluate(result.tree));
        }

        Run("1*24*3/6+3");
    }

}
