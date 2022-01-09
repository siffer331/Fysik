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
    public static Parser rational = PS.Choice(PS.Concatenate(PS.Sequence(num, PS.Str("."), num), "number"), num);
	public static Parser rationalComplete = PS.Choice(PS.Concatenate(PS.SequenceEat(whitespace, rational, PS.Str("e"), integer), "number"), rational);
    public static ParserGenerator parenthesis = PS.BetweenStr("(", ")", whitespace);
    public static ParserGenerator brackets = PS.BetweenStr("[", "]", whitespace);
	public static ParserGenerator colon = (Parser f) => PS.Choice(f, PS.GetChild(PS.SequenceEat(whitespace, f, PS.Str(":")), 0));
	public static Parser word = PS.Concatenate(PS.Many(PS.Letter), "word");
	public static Parser name = PS.Concatenate(PS.Many(PS.Choice(PS.Letter, PS.Digit, PS.Symbols("_"))), "word");

	public static Parser unitPart = PS.Choice(PS.SetType(PS.SequenceEat(whitespace, word, PS.Str("^"), integer), "unit_exponent"), word);
	public static Parser unitMultiply = PS.SetType(PS.ManySeperated(PS.Str("*"), unitPart, whitespace), "unit_multiply");
	public static Parser unitDivide = PS.SetType(PS.SequenceEat(whitespace, PS.Choice(PS.Str("1"), unitMultiply), PS.Str("/"), unitMultiply), "unit_divide");
	public static Parser unit = PS.Choice(unitDivide, unitMultiply);
	public static Parser value = PS.SetType(PS.SequenceEat(whitespace, rationalComplete, PS.GetChild(brackets(unit), 1)), "value_unit");

	public static Func<Parser, String, Parser> function = (Parser f, String s) => PS.SetType(PS.Sequence(PS.Str(s), PS.GetChild(parenthesis(f),1)), s);

    public static ParserRes calculation(String s) {
		Parser sin = function(calculation, "sin");
		Parser cos = function(calculation, "cos");
		Parser tan = function(calculation, "tan");
        Parser basicElement = PS.Choice(sin, cos, tan, value, rationalComplete, name, PS.GetChild(parenthesis(calculation), 1));
        Parser element = PS.Choice(basicElement, PS.SetType(PS.SequenceEat(whitespace, PS.Str("-"), basicElement), "negate"));
        Parser multiply = PS.SetType(PS.ManySeperated(PS.Str("*"), element, whitespace), "multiply");
        Parser divide = PS.Choice(PS.SetType(PS.SequenceEat(whitespace, multiply, PS.Str("/"), multiply), "divide"), multiply);
        return PS.SetType(PS.ManySeperated(PS.Choice(PS.Str("+"), PS.Str("-")), divide, whitespace), "add")(s);
    }

	public static Parser setVariable = PS.SetType(PS.SequenceEat(whitespace, word, PS.Str("="), calculation), "set_variable");
	public static Parser dealloc = function(word, "dealloc");
	public static Parser to = function(PS.SequenceEat(whitespace,calculation, PS.Str(","), unit), "to");
	public static Parser reset = PS.SetType(PS.Sequence(PS.Str("reset("), whitespace, PS.Str(")")), "reset");

	public static Parser run = colon(PS.Choice(setVariable, dealloc, reset, to, calculation));
}
