using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameLogic)]
	public class DisplayDialogueAction : FsmStateAction 
	{
		[RequiredField]
		[Tooltip("The required speaker.")]
		public GameObject who;

		[RequiredField]
		[Tooltip("Text to display")]
		public string script;


	}
}
