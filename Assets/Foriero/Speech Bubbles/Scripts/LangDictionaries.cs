using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class LangDictionaries : MonoBehaviour {
	[Serializable]
	public class LangDictionary{
		public string aliasName;
		public string assetPath;
	}
	
	public Lang.LanguageCode langCode = Lang.LanguageCode.Unassigned;
	public List<LangDictionary> dictionaries = new List<LangDictionary>();
	
	void Awake(){
		foreach(LangDictionary ld in dictionaries){
			Lang.AddDictionary(ld.aliasName, ld.assetPath);
		}
		if(langCode != Lang.LanguageCode.Unassigned && Lang.selectedLanguage == Lang.LanguageCode.Unassigned){
			Lang.selectedLanguage = langCode;	
		}
	}
}
