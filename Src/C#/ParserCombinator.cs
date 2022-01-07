using Godot;
using System;
using System.Collections.Generic;
using Parser = System.Func<System.String,ParserCombinator.ParserRes>;

public static class ParserCombinator {

	public class GrammarTree {
		public GrammarTree(String data = "") {
			this.data = data;
			this.type = "";
			this.children = new LinkedList<GrammarTree>();
		}

		public String ToString() {
			String res = "("+type+" "+data+":";
			foreach(GrammarTree child in children) res += child.ToString();
			return res+")";
		}

		public String Read() {
			if(children.Count == 0) return data;
			String res = "";
			foreach(GrammarTree child in children) res += child.Read();
			return res;
		}

		public String data;
		public String type;
		public LinkedList<GrammarTree> children;
	}
	
	public class ParserRes {
		public ParserRes(bool succes, String data = "") {
			this.succes = succes;
			if(succes) rest = data;
			else error = data;
			tree = new GrammarTree();
		}

		public String ToString() {
			if(!succes) return "Error: " + error;
			return "Rest: '" + rest + "' tree: " + tree.ToString();
		}

		public bool succes;
		public String error = "";
		public String rest = "";
		public GrammarTree tree;
	}
	
	public static ParserRes Digit(String s) {
		if(s == "") return new ParserRes(false, "unexpected end of input");
		if(char.IsDigit(s[0])) {
			ParserRes res = new ParserRes(true, s.Substring(1));
			res.tree.data = s[0].ToString();
			res.tree.type = "digit";
			return res;
		}
		return new ParserRes(false, String.Format("Expected digit got {0}", s[0]));
	}

	public static Parser Str(String match) {
		ParserRes Res(String s) {
			if(s.StartsWith(match)) {
				ParserRes res = new ParserRes(true, s.Substring(match.Length));
				res.tree.data = match;
				res.tree.type = "word";
				return res;
			}
			return new ParserRes(false, String.Format("Expected {0} but got {1}", match, s));
		}
		return Res;
	}

	public static ParserRes Letter(String s) {
		if(s == "") return new ParserRes(false, "unexpected end of input");
		if(char.IsLetter(s[0])) {
			ParserRes res = new ParserRes(true, s.Substring(1));
			res.tree.data = s[0].ToString();
			res.tree.type = "letter";
			return res;
		}
		return new ParserRes(false, String.Format("Expected letter but got {0}", s[0]));
	}

	public static Parser Symbols(String symbols) {
		ParserRes Res(String s) {
			if(s == "") return new ParserRes(false, "unexpected end of input");
			foreach(char c in symbols) {
				if(s[0] == c) {
					ParserRes res = new ParserRes(true, s.Substring(1));
					res.tree.data = s[0].ToString();
					res.tree.type = "symbol";
					return res;
				}
			}
			return new ParserRes(false, String.Format("Expected {0} but got {1}", symbols, s[0]));
		}
		return Res;
	}
	
	public static Parser NoSymbols(String symbols) {
		ParserRes Res(String s) {
			if(s == "") return new ParserRes(false, "unexpected end of input");
			foreach(char c in symbols) {
				if(s[0] == c) return new ParserRes(false, String.Format("Expected no {0} but got {1}", symbols, s[0]));
			}
			ParserRes res = new ParserRes(true, s.Substring(1));
			res.tree.data = s[0].ToString();
			res.tree.type = "symbol";
			return res;
		}
		return Res;
	}

	public static Parser SetType(Parser f, String type) {
		ParserRes Res(String s) {
			ParserRes res = f(s);
			res.tree.type = type;
			return res;
		}
		return Res;
	}

	public static Parser Concatenate(Parser parser, String type = "") {
		ParserRes Res(String s) {
			ParserRes res = parser(s);
			res.tree.data = res.tree.Read();
			res.tree.children = new LinkedList<GrammarTree>();
			if(type != "") res.tree.type = type;
			return res;
		}
		return Res;
	}

	public static Parser CompressLayer(Parser f) {
		ParserRes Res(String s) {
			ParserRes res = f(s);
			LinkedList<GrammarTree> list = new LinkedList<GrammarTree>();
			foreach(GrammarTree child in res.tree.children) {
				foreach(GrammarTree node in child.children) list.AddLast(node);
			}
			res.tree.children = list;
			return res;
		}
		return Res;
	}

	public static Parser GetChild(Parser f, int child) {
		ParserRes Res(String s) {
			ParserRes res = f(s);
			if(child >= res.tree.children.Count) {
				res.tree.children = new LinkedList<GrammarTree>();
				return res;
			}

			LinkedListNode<GrammarTree> tree = res.tree.children.First;
			for(int i = 0; i < child; i++) tree = tree.Next;
			res.tree = tree.Value;
			return res;
		}
		return Res;
	}
	
	public static Parser Any(Parser f, Parser eat = null) {
		ParserRes Res(String s) {
			ParserRes res = new ParserRes(true);
			res.tree.type = "sequence";
			while(true) {
				if(eat != null) s = eat(s).rest;
				ParserRes test = f(s);
				if(!test.succes) break;
				res.tree.children.AddLast(test.tree);
				s = test.rest;
			}
			if(eat != null) s = eat(s).rest;
			res.rest = s;
			return res;
		}
		return Res;
	}

	public static Parser Many(Parser f, Parser eat = null) {
		ParserRes Res(String s) {
			ParserRes res = Any(f, eat)(s);
			if(res.tree.children.Count == 0) return new ParserRes(false, "Many: Found no matches");
			return res;
		}
		return Res;
	}


	public static Parser Choice(params Parser[] parsers) {
		ParserRes Res(String s) {
			ParserRes res = new ParserRes(false, "Choice: Had no parsers");
			foreach(Parser parser in parsers) {
				res = parser(s);
				if(res.succes) return res;
			}
			return new ParserRes(false, res.error);
		}
		return Res;
	}

	public static Parser Sequence(params Parser[] parsers) {
		return SequenceEat(null, parsers);
	}

	public static Parser SequenceEat(Parser eat, params Parser[] parsers) {
		ParserRes Res(String s) {
			ParserRes res = new ParserRes(true);
			res.tree.type = "sequence";
			foreach(Parser parser in parsers) {
				if(eat != null) s = eat(s).rest;
				ParserRes test = parser(s);
				if(!test.succes) return new ParserRes(false, test.error);
				res.tree.children.AddLast(test.tree);
				s = test.rest;
			}
			if(eat != null) s = eat(s).rest;
			res.rest = s;
			return res;
		}
		return Res;
	}

	public static Func<Parser, Parser> Between(Parser start, Parser end, Parser eat = null) {
		Parser Res(Parser parser) {
			return Sequence(start, parser, end, eat);
		}
		return Res;
	}

	public static Func<Parser, Parser> BetweenStr(String start, String end, Parser eat = null) {
		return Between(Str(start), Str(end), eat);
	}

	public static Parser AnySeperated(Parser sepperator, Parser content, Parser eat = null) {
		ParserRes Res(String s) {
			ParserRes res = new ParserRes(true);
			res.tree.type = "sequence";
			if(eat != null) s = eat(s).rest;
			ParserRes test = content(s);
			if(!test.succes) return res;
			s = test.rest;
			res.tree.children.AddLast(test.tree);
			if(test.succes) {
				while(true) {
					if(eat != null) s = eat(s).rest;
					test = sepperator(s);
					if(!test.succes) {
						break;
					}
					GrammarTree tree = test.tree;
					String rest = test.rest;
					if(eat != null) rest = eat(rest).rest;
					test = content(rest);
					if(!test.succes) {
						break;
					}
					res.tree.children.AddLast(tree);
					res.tree.children.AddLast(test.tree);
					s = test.rest;
				}
			}
			if(eat != null) s = eat(s).rest;
			res.rest = s;
			return res;
		}
		return Res;
	}

	public static Parser ManySeperated(Parser sepperator, Parser content, Parser eat = null) {
		ParserRes Res(String s) {
			ParserRes res = AnySeperated(sepperator, content, eat)(s);
			if(res.tree.children.Count == 0) return new ParserRes(false, "ManySeperated: Found no matches in "+s);
			return res;
		}
		return Res;
	}
}
