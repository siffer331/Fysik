using Godot;
using System;

using Parser = System.Func<System.String,ParserCombinator.ParserRes>;
using ParserGenerator = System.Func<System.Func<System.String,ParserCombinator.ParserRes>, System.Func<System.String,ParserCombinator.ParserRes>>;
using PS = ParserCombinator;
using ParserRes = ParserCombinator.ParserRes;


public static class Parsers {
	public static Parser whitespace = PS.Any(PS.Symbols(" \t"));
    public static Parser num = PS.Concatenate(PS.Many(PS.Digit), "number");
    public static Parser integer = PS.Choice(num, PS.Concatenate(PS.SequenceEat(whitespace, PS.Str("-"), num), "number"));
    public static ParserGenerator parenthesis = PS.BetweenStr("(", ")", whitespace);
    public static ParserGenerator brackets = PS.BetweenStr("[", "]", whitespace);
	public static ParserGenerator colon = (Parser f) => PS.Choice(f, PS.GetChild(PS.SequenceEat(whitespace, f, PS.Str(":")), 0));

	public static Parser word = PS.Concatenate(PS.Many(PS.Letter, whitespace), "word");
	public static Parser unitPart = PS.Choice(PS.SetType(PS.SequenceEat(whitespace, word, PS.Str("^"), integer), "unit_exponent"), word);
	public static Parser unitMultiply = PS.SetType(PS.ManySeperated(PS.Str("*"), unitPart, whitespace), "unit_multiply");
	public static Parser unitDivide = PS.SetType(PS.SequenceEat(whitespace, unitMultiply, PS.Str("/"), unitMultiply), "unit_divide");
	public static Parser unit = PS.Choice(unitDivide, unitMultiply);
	public static Parser value = PS.SetType(PS.SequenceEat(whitespace, num, PS.GetChild(brackets(unit), 1)), "value_unit");

    public static ParserRes calculation(String s) {
        Parser basicElement = PS.Choice(word, value, num, PS.GetChild(parenthesis(calculation), 1));
        Parser element = PS.Choice(basicElement, PS.SetType(PS.SequenceEat(whitespace, PS.Str("-"), basicElement), "negate"));
        Parser multiply = PS.SetType(PS.ManySeperated(PS.Str("*"), element, whitespace), "multiply");
        Parser divide = PS.Choice(PS.SetType(PS.SequenceEat(whitespace, multiply, PS.Str("/"), multiply), "divide"), multiply);
        return PS.SetType(PS.ManySeperated(PS.Choice(PS.Str("+"), PS.Str("-")), divide, whitespace), "add")(s);
    }

	public static Parser setVariable = PS.SetType(PS.SequenceEat(whitespace, word, PS.Str("="), calculation), "set_variable");

	public static Parser run = colon(PS.Choice(setVariable, calculation));
}
