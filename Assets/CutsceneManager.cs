using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;

public class CutsceneManager : MonoBehaviour 
{

	public GUIStyle dialogueStyle;
	public GUIStyle locationStyle;
	public GUIStyle nameStyle;
	public PlayMakerFSM fsm;

	// current string for displaying
	private string currentTextChunk = "";
	private string currentString = "";
	private string currentName = "";

	// current character index of string
	private int currentCharIndex = 0;
	private bool next = false;
	private bool displayingDialogue = false;
	public float textSpeed = 0.3f;

	private string currentLocationText = "";
	private string currentLocationString = "";
	private int currentLocationCharIndex = 0;
	private bool nextLocation = false;
	private bool displayingLocation = false;
	private bool doingDialogue = false;
	public float locationtextSpeed = 0.3f;
	private bool fadeLocation = false;
	private float locationAlpha = 1;
	public float locationTextFadeOutTime = 3f;

	private float _t = 0;
	private FsmBool canSkip = false;


	// Use this for initialization
	void Start () 
	{
		canSkip = FsmVariables.GlobalVariables.FindFsmBool("CanSkip");
	}

	public void StartLocationText(string locText)
	{
		currentLocationString = locText;
		displayingLocation = true;
	}

	public void StartDialogue(string dialogue, int fStyle, string cName)
	{
		currentName = cName;
		dialogueStyle.fontStyle = (FontStyle)fStyle;
		ClearDialogue();
		currentString = dialogue;
		displayingDialogue = true;
		doingDialogue = true;
	}

	public void ClearDialogue()
	{
		currentTextChunk = "";
		currentString = "";
		doingDialogue = false;
	}

	public void ClearLocation()
	{
		currentLocationString = "";
		currentLocationText = "";
		fadeLocation = false;
	}

	public IEnumerator WaitToFadeLocation()
	{
		yield return new WaitForSeconds(locationTextFadeOutTime);

		fadeLocation = true;

		Debug.Log ("Done fade location");
	}

	public IEnumerator DisplayDialogue()
	{
		next = true;
		yield return new WaitForSeconds(textSpeed);

		if(currentCharIndex < currentString.Length)
		{
			currentTextChunk += currentString[currentCharIndex];
			currentCharIndex++;
			next = false;
		}
		else
		{
			displayingDialogue = false;
			next = false;
			currentCharIndex = 0;
			canSkip.Value = true;
		}
	}

	public IEnumerator DisplayLocation()
	{
		nextLocation = true;
		yield return new WaitForSeconds(locationtextSpeed);
		
		if(currentLocationCharIndex < currentLocationString.Length)
		{
			currentLocationText += currentLocationString[currentLocationCharIndex];
			currentLocationCharIndex++;
			nextLocation = false;
		}
		else
		{
			displayingLocation = false;
			nextLocation = false;
			currentLocationCharIndex = 0;
			StartCoroutine(WaitToFadeLocation());
		}
	}
	// Update is called once per frame
	void Update () 
	{
		if(!next && displayingDialogue)
		{
			StartCoroutine(DisplayDialogue());
		}

		if(!nextLocation && displayingLocation)
		{
			StartCoroutine(DisplayLocation());
		}

		if(fadeLocation)
		{
			locationAlpha = Mathf.Lerp(1f, 0f, _t);
			_t += Time.deltaTime/2; //Take 2 seconds
		}

		if(locationAlpha == 0)
		{
			ClearLocation();
			locationAlpha = 1;
		}
	}
	
	void OnGUI()
	{
		/*
		GUI.color = new Color(GUI.color.r, GUI.color.r, GUI.color.r, 1);
		GUI.TextArea (new Rect(Screen.width * 0.1f, Screen.height * 0.8f, Screen.width * 0.8f, Screen.height * 0.15f), currentTextChunk, dialogueStyle);
		GUI.color = new Color(GUI.color.r, GUI.color.r, GUI.color.r, locationAlpha);
		GUI.TextArea (new Rect(Screen.width * 0.1f, Screen.height * 0.8f, Screen.width * 0.8f, Screen.height * 0.15f), currentLocationText, locationStyle);
*/

		GUI.color = new Color(GUI.color.r, GUI.color.r, GUI.color.r, 1);
		if(doingDialogue)
		{
			GUI.Label (new Rect(0,Screen.width * 0.4f,Screen.width, Screen.height), currentTextChunk, dialogueStyle);
			GUI.Label(new Rect(0,Screen.width * 0.4f,Screen.width, Screen.height), currentName, nameStyle);
		}
		GUI.color = new Color(GUI.color.r, GUI.color.r, GUI.color.r, locationAlpha);
		GUI.Label (new Rect(0,0,Screen.width, Screen.height), currentLocationText, locationStyle);
	}
}
