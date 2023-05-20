using Godot;
using System;
using System.Collections.Generic;

public class Import : WindowDialog {
	static Dictionary<String,String> defaults = new Dictionary<String, String>() {
		{"Constants","g = 9.82 m/s^2\ne_ = 1.6e-19 C"},
		{"Factors","π = 3.14159\nPI = 3.14159\ne = 2.7182818284"},
		{"Formulas","[Newton]\n#kinematic\nF = m*a\na = F/m\nm = F/a"},
		{"Units","pure 1000 g = 1 kg\n1000 L = 1 m^3\nSI N = kg*m/s^2\n180 deg = π r"}
	};
	
	static String currentType;
	
	private void _on_Type_pressed(String type){
		GetNode<TextEdit>("Margin/Split/Text").Text = defaults[type];
		currentType = type;
	}
	
	private void _on_Import_pressed(){
		TextEdit text = GetNode<TextEdit>("Margin/Split/Text");
		if(currentType == "") text.Text += "\nNo type selected";
		else {
			try {
				if(currentType == "Constants") Data.ReadConstantText(text.Text);
				if(currentType == "Factors") Data.ReadFactorText(text.Text);
				if(currentType == "Formulas") Data.ReadFormulaText(text.Text);
				if(currentType == "Units") Data.ReadUnitText(text.Text);
				text.Text += "\nImport succeded";
			}
			catch (Exception e) {
				text.Text += "\n" + Data.proccesingLine;
			}
		}
	}
}
