using Godot;
using System;

using Parser = System.Func<System.String,ParserCombinator.ParserRes>;
using ParserGenerator = System.Func<System.Func<System.String,ParserCombinator.ParserRes>, System.Func<System.String,ParserCombinator.ParserRes>>;
using PS = ParserCombinator;


public static class Parsers {

    public static Parser num = PS.Concatenate(PS.Many(PS.Digit), "number");
    public static Parser integer = PS.Choice(num, PS.Concatenate(PS.Sequence(PS.Str("-"), num), "number"));
    public static ParserGenerator parenthesis = PS.BetweenStr("(", ")");
    public static PS.ParserRes Calculation(String s) {
        Parser element = PS.Choice(integer, PS.SetType(parenthesis(Calculation), "calculation"));
        Parser multiply = PS.SetType(PS.ManySeperated(PS.Str("*"), element), "multiply");
        Parser divide = PS.Choice(PS.SetType(PS.Sequence(multiply, PS.Str("/"), multiply), "divide"), multiply);
        return PS.SetType(PS.ManySeperated(PS.Choice(PS.Str("+"), PS.Str("-")), divide), "add")(s);
    }

}
