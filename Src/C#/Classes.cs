using Godot;
using System;
using System.Collections.Generic;


public struct Unit {

	Dictionary<String, int> parts;

	public String ToString() {
		String top = "";
		String bottom = "";
		foreach(String part in parts) {
			if(parts[part] > 0) {
				top += "*" + part;
				if(parts[part] > 1) top += "^" + parts[part].ToString();
			}
			else {
				bottom += "*" + part;
				if(parts[part] < -1) top += "^" + (-parts[part]).ToString();
			}
			top = top.Substring(1);
			bottom = bottom.Substring(1);
			if(top == "") top = "1";
		}
	}

}

public class Value {

}
