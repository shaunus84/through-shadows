using UnityEngine;
using System.Collections;

public class SpeechBubbleDebug : MonoBehaviour {
	public bool debug = false;
	public static bool _debug = false;
	
	void Awake(){
		_debug = debug;
	}
}
