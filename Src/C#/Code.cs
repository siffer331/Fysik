using Godot;
using System;
using ParserRes = ParserCombinator.ParserRes;

public class Code : Control {

	[Export]
	NodePath codePath;
	TextEdit code;
	bool undo = false;

	public override void _Ready() {
		code = GetNode(codePath) as TextEdit;
	}

	public override void _Input(InputEvent inputEvent) {
		if(inputEvent.IsActionPressed("run_line")) {
			RunLine();
			undo = true;
		}
	}

	public void RunLine() {
		int lineNumber = code.CursorGetLine();
		String line = code.GetLine(lineNumber);
		String result = EvaluateLine(line);
		if(result != "") {
			code.CursorSetColumn(line.Length);
			if(
				lineNumber < code.GetLineCount()-1 &&
				code.GetLine(lineNumber+1).Length > 1 && code.GetLine(lineNumber+1).Substring(0,2) == "//"
			) {
				String text = "";
				String[] lines = code.Text.Split("\n");
				for(int i = 0; i < lines.Length; i++) {
					if(i != lineNumber+1) text += lines[i] + "\n";
				}
				text = text.Substring(0, text.Length-1);
				code.Text = text;
				code.CursorSetLine(lineNumber);
				code.CursorSetColumn(line.Length);
			}
			if(line == "" || line[line.Length-1] != ':') code.InsertTextAtCursor("\n//" + result);
			code.CursorSetLine(lineNumber);
			code.CursorSetColumn(line.Length);
		} 
	}

	public String EvaluateLine(String line) {
		if(line != "" && line[0] != '#' && line[0] != '%' && (line.Length < 2 || line.Substring(0,2) != "//")) {
			ParserRes parsingResult = Parsers.unit(line);
			if(!parsingResult.succes) return "/" + parsingResult.ToString();
			if(parsingResult.rest != "") return "/missplassed text: " + parsingResult.rest;

			try {
				Unit result = Evaluators.Unit(parsingResult.tree);
				return result.ToString();
			}
			catch(Exception e) {
				return "/" + e.ToString();
			}
		}
		return "";
	}

	public void OnTextEditTextChanged() {
		if(undo) code.Undo();
		undo = false;
	}

}
