using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections;
using System.IO;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Localization")]
	[Tooltip("Init a dictionary. Call this only once, when the scene starts or if you need to load a new dictionary on the fly.")]
	public class DictionaryInit : FsmStateAction
	{
		[RequiredField]
		[Tooltip("A name that you will use in Get Text Action. If IsNone a default file name is used instead.")]
		public FsmString aliasName;
		[RequiredField]
		[Tooltip("Resource path for example : Dictionaries/mydictionary")]
		public FsmString assetPath;		
		public bool appendLanguage = false;
			
		public override void Reset()
		{
			aliasName = new FsmString{UseVariable = true};
			assetPath = new FsmString{UseVariable = true};
			appendLanguage = false;
		}

		public override void OnEnter()
		{
			string path = assetPath.Value +  (appendLanguage ? "_" + Lang.selectedLanguage.ToString() : "" );
			Lang.AddDictionary(aliasName.Value, path);
			Finish();
		}
	}
}