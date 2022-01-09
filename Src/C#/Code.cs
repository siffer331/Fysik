using Godot;
using System;
using System.Collections.Generic;
using ParserRes = ParserCombinator.ParserRes;
using GrammarTree = ParserCombinator.GrammarTree;

public class Code : Control {

	[Export]
	NodePath codePath;
	TextEdit code;
	bool undo = false;


	public override void _Ready() {
		Data.LoadLibraries();
		code = GetNode(codePath) as TextEdit;

		code.AddColorRegion("///", "", new Color(.6f,.3f,.3f));
		code.AddColorRegion("//", "", new Color(.4f,.4f,.8f));
		code.AddColorRegion("#", "", new Color(.4f,.4f,.4f));
		code.AddColorRegion("%", "", new Color(.4f,.7f,.4f));
		code.AddColorRegion("[", "]", new Color(.4f,.5f,.8f));
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
			ParserRes parsingResult = Parsers.run(line);
			if(!parsingResult.succes) return "/" + parsingResult.ToString();
			if(parsingResult.rest != "") return "/missplassed text: " + parsingResult.rest;

			try {
				switch(parsingResult.tree.type) {
					case "add":
						Value result = Evaluators.Calculation(parsingResult.tree);
						return result.ToString();
					case "set_variable":
						String variableName = parsingResult.tree.children.First.Value.data;
						Value value = Evaluators.Calculation(parsingResult.tree.children.First.Next.Next.Value);
						Data.variables[variableName] = value;
						return variableName + " has been set to " + value.ToString();
					case "dealloc":
						variableName = parsingResult.tree.children.First.Next.Value.data;
						return Commands.Dealloc(variableName);
					case "reset":
						return Commands.Reset();
					case "to":
						value = Evaluators.Calculation(parsingResult.tree.children.First.Next.Value.children.First.Value);
						Unit unit = Evaluators.Unit(parsingResult.tree.children.First.Next.Value.children.First.Next.Next.Value);
						return Commands.To(value, unit);
				}
				return "/Did not recognise tree type " + parsingResult.tree.type;
			}
			catch(Exception e) {
				//GD.Print(e.ToString());
				return "/" + e.Message;
			}
		}
		return "";
	}

	public void OnTextEditTextChanged() {
		if(undo) code.Undo();
		undo = false;
	}

}
