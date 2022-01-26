using Godot;
using System;

public class LeftPanel : Control {

	[Export]
	public NodePath codePath;

	private Code code;

	public override void _Ready() {
		code = GetNode<Code>(codePath);
		foreach(char c in "πΩμτε") {
			Button button = new Button();
			button.Text = ""+c;
			button.RectMinSize = new Vector2(22, 22);
			GetNode("Margin/Sepperator/Characters/HeaderSplit/Margin/Scroll/Split").AddChild(button);
			button.Connect("pressed", this, "OnButtonPressed", new Godot.Collections.Array {""+c});
		}
	}

	private void OnButtonPressed(String text) {
		code.Write(text);
	}

	public void DisaplyVariables() {
		VBoxContainer split = GetNode<VBoxContainer>("Margin/Sepperator/Variables/HeaderSplit/Margin/Scroll/Split");
		foreach(Control child in split.GetChildren()) child.QueueFree();
		foreach(String variable in Data.variables.Keys) {
			Label label = new Label();
			label.Text = variable + " = " + Data.variables[variable].ToString();
			split.AddChild(label);
		}
	}
}