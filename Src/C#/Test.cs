using System;

using GrammarTree = ParserCombinator.GrammarTree;
using ParserRes = ParserCombinator.ParserRes;
using Parser = System.Func<System.String,ParserCombinator.ParserRes>;
using ParserGenerator = System.Func<System.Func<System.String,ParserCombinator.ParserRes>, System.Func<System.String,ParserCombinator.ParserRes>>;
using PS = ParserCombinator;

public class Test {

	public static Parser seq = PS.Sequence(PS.Str("Hej"), PS.Str(" "), PS.Choice(PS.Str("no"), PS.Str("yes")));
	public static Parser run = seq;

}
