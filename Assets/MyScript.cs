using UnityEngine;
using System.Collections;

public class MyScript : MonoBehaviour{
	public Texture2D New_Element_0;




	void OnGUI(){
		GUI.Box( new Rect(546f, 33f, 10f, 10f), new GUIContent("", New_Element_0, ""));
	}
}