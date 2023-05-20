using static Godot.GD;
using Godot;
using System;
using System.Collections.Generic;
using ParserRes = ParserCombinator.ParserRes;
using GrammarTree = ParserCombinator.GrammarTree;

public class Code : Control {
	[Export]
	public NodePath codePath;
	[Export]
	public NodePath panelPath;
	[Export]
	public NodePath exportPath;
	
	LeftPanel panel;
	TextEdit code;
	Popup export;
	bool undo = false;
	
	public override void _Ready() {
		Data.LoadLibraries();
		code = GetNode<TextEdit>(codePath);
		panel = GetNode<LeftPanel>(panelPath);
		export = GetNode<Popup>(exportPath);
	
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
		} if(inputEvent.IsActionPressed("run")) RunAll();
		if(inputEvent.IsActionPressed("export")) Export();
	}
	
	public void RunAll() {
		int line = code.CursorGetLine();
		int column = code.CursorGetColumn();
		for(int i = 0; i < code.GetLineCount(); i++) {
			int r = RunLine(i);
			if(i < line) line += r;
		}
		code.CursorSetLine(line);
		code.CursorSetColumn(column);
	}
	
	public int RunLine(int lineNumber = -1) {
		if(lineNumber == -1) lineNumber = code.CursorGetLine();
		int curserLine = code.CursorGetLine();
		int cursorColumn = code.CursorGetColumn();
		String line = code.GetLine(lineNumber);
		String result = EvaluateLine(line);
		int change = 0;
		if(result != "") {
			code.CursorSetColumn(line.Length);
			String text = "";
			for(int i = 0; i < code.GetLineCount(); i++) {
				String l = code.GetLine(i);
				if(i == lineNumber+1 && l.Length > 1 && l.Substring(0,2) == "//") change = -1;
				else text += l + "\n";
				if(i == lineNumber) {
					String trim = line.Trim();
					if(trim[trim.Length-1] != ':') {
						text += "//" + result + "\n";
						change += 1;
					}
				}
			}
			text = text.Substring(0, text.Length-1);
			code.Text = text;
		} 
		code.CursorSetLine(curserLine);
		code.CursorSetColumn(cursorColumn);
		return change;
	}
	
	public String DemonstrateLine(String line) {
		ParserRes parsingResult = Test.run(line);
		if(!parsingResult.succes) return parsingResult.error;
		return parsingResult.rest + "\n" + parsingResult.tree.ToIndentedString();
	}
	
	public void Export() {
		String res = "";
		foreach(String line in code.Text.Split("\n")) {
			if(line == "" || line[0] == '#' || (line.Length > 1 && line.Substring(0,2) == "//")) continue;
			if(line[0] == '%') {
				res += line.Substring(1) + "\n";
				continue;
			}
			ParserRes parsingResult = Parsers.run(line);
			if(!parsingResult.succes || parsingResult.rest != "") continue;
			try {
				switch(parsingResult.tree.type) {
					case "add":
						Value result = Evaluators.Calculation(parsingResult.tree);
						res += "$" + result.ToLatex() + "$\n";
						continue;
					case "set_variable":
						String variableName = parsingResult.tree.children.First.Value.data;
						Value value = Evaluators.Calculation(parsingResult.tree.children.First.Next.Next.Value);
						res += "$"+variableName + " = " + value.ToLatex() + "$\n";
						continue;
					case "to":
						value = Evaluators.Calculation(parsingResult.tree.children.First.Next.Value.children.First.Value);
						Unit unit = Evaluators.Unit(parsingResult.tree.children.First.Next.Value.children.First.Next.Next.Value);
						res += "$" + Commands.To(value, unit).ToLatex() + "$\n";
						continue;
				}
			} catch(Exception e) {}
		}
		foreach(String character in Data.latexCharacters.Keys) res.Replace(character, Data.latexCharacters[character] + " ");
		export.GetNode<TextEdit>("Margin/Text").Text = res;
		export.Popup_();
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
						panel.DisaplyVariables();
						return variableName + " has been set to " + value.ToString();
					case "dealloc":
						variableName = parsingResult.tree.children.First.Next.Value.data;
						String res = Commands.Dealloc(variableName);
						panel.DisaplyVariables();
						return res;
					case "reset":
						String resu = Commands.Reset();
						panel.DisaplyVariables();
						return resu;
					case "to":
						value = Evaluators.Calculation(parsingResult.tree.children.First.Next.Value.children.First.Value);
						Unit unit = Evaluators.Unit(parsingResult.tree.children.First.Next.Value.children.First.Next.Next.Value);
						return Commands.To(value, unit).ToString();
					case "delta":
						GrammarTree tree = parsingResult.tree.children.First.Next.Value;
						Data.delta1 = tree.children.First.Value.data;
						Data.delta2 = tree.children.First.Next.Next.Value.data;
						return "succes";
				}
				return "/Did not recognise tree type " + parsingResult.tree.type;
			}
			catch(Exception e) {
				//Print(e.ToString());
				return "/" + e.Message;
			}
		}
		return "";
	}
	
	public void OnTextEditTextChanged() {
		if(undo) code.Undo();
		undo = false;
	}
	
	public void Write(String text) {
		code.InsertTextAtCursor(text);
	}
	
	private void _on_Run_pressed() {
		RunAll();
	}
	
	private void _on_Export_pressed() {
		Export();
	}
	
	private void _on_Import_pressed() {
		GetNode<WindowDialog>("../../../../ImportPopup").Show();
	}
}

