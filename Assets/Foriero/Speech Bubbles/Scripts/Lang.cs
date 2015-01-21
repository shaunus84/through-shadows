#if UNITY_EDITOR
using UnityEditor;
using System.Net;
#endif
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

public static class Lang {
	
	private static Lang.LanguageCode _selectedLanguage = Lang.LanguageCode.Unassigned;
		
	public static Lang.LanguageCode selectedLanguage{
		set{
			_selectedLanguage = value;
			if(OnLanguageChange != null) OnLanguageChange();
		}
		get{
			return _selectedLanguage;
		}
	}
	
	public delegate void LanguageChangeEvent(); 
	
	public static event LanguageChangeEvent OnLanguageChange;
	
	private static List<LangDictionary> dictionaries = new List<LangDictionary>();
	
	public static bool DictionaryExists(string anAliasName){
		foreach(LangDictionary d in dictionaries){
			if(d.aliasName.Equals(anAliasName)) {
				return true;	
			}
		}
		return false;
	}
		
	public static LangDictionary GetDictionary(string anAliasName){
		foreach(LangDictionary d in dictionaries){
			if(d.aliasName.Equals(anAliasName)) {
				return d;	
			}
		}
		return null;
	}
	
	public static LangDictionary AddDictionary(string anAliasName, string anAssetPath){
		if(DictionaryExists(anAliasName)) dictionaries.Remove(GetDictionary(anAliasName));
		LangDictionary result = new LangDictionary(anAliasName, anAssetPath);
		dictionaries.Add(result);
		Debug.Log("DICTIONARY ADD : " + anAliasName + " RESOURCE PATH : " + anAssetPath);
		return result;
	}
								
	public static string GetText(string aDictionaryAliasName, string anId, string aDefaultValue = ""){
		if(DictionaryExists(aDictionaryAliasName)){	
			LangDictionary dictionary = GetDictionary(aDictionaryAliasName);
			return dictionary.GetText(anId, aDefaultValue);
		} else {
			Debug.LogWarning(
				"DICTIONARY NOT FOUND - GET TEXT : " + (string.IsNullOrEmpty(anId) ? "EMPTY ID" : anId)
				+ "\n" + "IN DICTIONARY : " + (string.IsNullOrEmpty(aDictionaryAliasName) ? "EMPTY ALIASNAME" : aDictionaryAliasName)
				+ "\n" + "DEFAULT VALUE : " + (string.IsNullOrEmpty(aDefaultValue) ? "EMPTY DEFAULT VALUE" : aDefaultValue)
				);
			return aDefaultValue;
		}	
	}
	
	public class LangDictionary {
		public string assetPath = "";
		public string aliasName = "";
		public Dictionary<string,string> dictionary = new Dictionary<string, string>();
		public List<LanguageCode> languages = new List<LanguageCode>();
		
		private Lang.LanguageCode langCode = LanguageCode.Unassigned;
		
		public LangDictionary(string anAliasName, string anAssetPath){
		 	aliasName = anAliasName;	
			assetPath = anAssetPath;
		}
		
		public string GetText(string anId, string aDefaultValue = ""){
			if(selectedLanguage != Lang.LanguageCode.Unassigned && langCode != selectedLanguage) InitDictionary();
			if(dictionary.ContainsKey(anId)){
				return dictionary[anId].Replace(@"\n","\n");
			} else {
				Debug.LogWarning("RECORD NOT FOUND - GET TEXT : " + (string.IsNullOrEmpty(anId) ? "EMPTY ID" : anId)
				+ "\n" + "DEFAULT VALUE : " + (string.IsNullOrEmpty(aDefaultValue) ? "EMPTY DEFAULT VALUE" : aDefaultValue)
				);
				return aDefaultValue;
			}
		}
		
		public void InitDictionary(){
			Debug.Log("DICTIONARY INITIALIZED : " + aliasName + " ASSET PATH : " + assetPath + " LANGUAGE : " + Lang.selectedLanguage.ToString());
			TextAsset ta = (TextAsset)Resources.Load(assetPath, typeof(TextAsset));
			if(ta == null){
				Debug.LogError("DICTIONARY NOT FOUND AT RESOURCE PATH: " + assetPath);
				return;
			}
			languages.Clear();
			dictionary.Clear();
			string id = "";
			string line = "";
			string[] defs = new string[0];
			string[] values = new string[0];
			
			MemoryStream stream = new MemoryStream(ta.bytes);
			StreamReader sr = new StreamReader(stream);
			
			line = sr.ReadLine();
			defs = line.ToUpper().Split(";".ToCharArray()[0]);
			for(int i = 1; i<defs.Length;i++){
				languages.Add(GetLanguageEnum(defs[i]));
			}
			
			while(!sr.EndOfStream){
				line = sr.ReadLine();
				
				line = line.Replace(@"\;","_semicolon");
				values = line.Split(";".ToCharArray()[0]);
				for(int k = 0; k<languages.Count + 1;k++){
					if(k==0){
						id = values[k];	
					} else {
						if(selectedLanguage == languages[k-1]){
							if(dictionary.ContainsKey(id)){
								Debug.LogError("DICTIONARY : " + aliasName + " ALREADY CONTAINS KEY : " + id);	
							} else {
								try{
									dictionary.Add(id, values[k].Replace("_semicolon",";").Replace(@"\n","\n"));
								} catch (Exception e){
									Debug.LogError("DICTIONARY : " + aliasName + " KEY : " + id + " " + e.Message);		
								}
							}
						}
					}
				}
			}
			
			sr.Close();
		 	stream.Close();
			Resources.UnloadAsset(ta);
			langCode = selectedLanguage;
		}
	}

	public static LanguageCode GetOSLanguageCode(){
		LanguageCode localLang = LanguageNameToCode(Application.systemLanguage); 
        if(localLang == LanguageCode.Unassigned) localLang = GetLanguageEnum(CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
        return localLang;  	
	}
	
	public static LanguageCode GetLanguageEnum(string langCode)
    {
		langCode = langCode.ToUpper();
        foreach(LanguageCode item in System.Enum.GetValues(typeof(LanguageCode)) )
        {
            if(item.ToString() == langCode){
				return item;
            }
        }
        Debug.LogError("ERORR: There is no language: ["+langCode+"]");
        return LanguageCode.Unassigned;
    }
	
	public static LanguageCode LanguageNameToCode(SystemLanguage name){
        if (name == SystemLanguage.Afrikaans) return LanguageCode.AF;
        else if (name == SystemLanguage.Arabic) return LanguageCode.AR;
        else if (name == SystemLanguage.Basque) return LanguageCode.BA;
        else if (name == SystemLanguage.Belarusian) return LanguageCode.BE;
        else if (name == SystemLanguage.Bulgarian) return LanguageCode.BG;
        else if (name == SystemLanguage.Catalan) return LanguageCode.CA;
        else if (name == SystemLanguage.Chinese) return LanguageCode.ZH;
        else if (name == SystemLanguage.Czech) return LanguageCode.CS;
        else if (name == SystemLanguage.Danish) return LanguageCode.DA;
        else if (name == SystemLanguage.Dutch) return LanguageCode.NL;
        else if (name == SystemLanguage.English) return LanguageCode.EN;
        else if (name == SystemLanguage.Estonian) return LanguageCode.ET;
        else if (name == SystemLanguage.Faroese) return LanguageCode.FA;
        else if (name == SystemLanguage.Finnish) return LanguageCode.FI;
        else if (name == SystemLanguage.French) return LanguageCode.FR;
        else if (name == SystemLanguage.German) return LanguageCode.DE;
        else if (name == SystemLanguage.Greek) return LanguageCode.EL;
        else if (name == SystemLanguage.Hebrew) return LanguageCode.HE;
        else if (name == SystemLanguage.Hungarian) return LanguageCode.HU;
        else if (name == SystemLanguage.Icelandic) return LanguageCode.IS;
        else if (name == SystemLanguage.Indonesian) return LanguageCode.ID;
        else if (name == SystemLanguage.Italian) return LanguageCode.IT;
        else if (name == SystemLanguage.Japanese) return LanguageCode.JA;
        else if (name == SystemLanguage.Korean) return LanguageCode.KO;
        else if (name == SystemLanguage.Latvian) return LanguageCode.LA;
        else if (name == SystemLanguage.Lithuanian) return LanguageCode.LT;
        else if (name == SystemLanguage.Norwegian) return LanguageCode.NO;
        else if (name == SystemLanguage.Polish) return LanguageCode.PL;
        else if (name == SystemLanguage.Portuguese) return LanguageCode.PT;
        else if (name == SystemLanguage.Romanian) return LanguageCode.RO;
        else if (name == SystemLanguage.Russian) return LanguageCode.RU;
        else if (name == SystemLanguage.SerboCroatian) return LanguageCode.SH;
        else if (name == SystemLanguage.Slovak) return LanguageCode.SK;
        else if (name == SystemLanguage.Slovenian) return LanguageCode.SL;
        else if (name == SystemLanguage.Spanish) return LanguageCode.ES;
        else if (name == SystemLanguage.Swedish) return LanguageCode.SW;
        else if (name == SystemLanguage.Thai) return LanguageCode.TH;
        else if (name == SystemLanguage.Turkish) return LanguageCode.TR;
        else if (name == SystemLanguage.Ukrainian) return LanguageCode.UK;
        else if (name == SystemLanguage.Vietnamese) return LanguageCode.VI;
        else if (name == SystemLanguage.Hungarian) return LanguageCode.HU;
        else if (name == SystemLanguage.Unknown) return LanguageCode.Unassigned; 
        return LanguageCode.Unassigned;
    }
	
	public enum LanguageCode
	{
	    Unassigned,//null
	    AA, //Afar
	    AB, //Abkhazian
	    AF, //Afrikaans
	    AM, //Amharic
	    AR, //Arabic
	    AS, //Assamese
	    AY, //Aymara
	    AZ, //Azerbaijani
	    BA, //Bashkir
	    BE, //Byelorussian
	    BG, //Bulgarian
	    BH, //Bihari
	    BI, //Bislama
	    BN, //Bengali
	    BO, //Tibetan
	    BR, //Breton
	    CA, //Catalan
	    CO, //Corsican
	    CS, //Czech
	    CY, //Welch
	    DA, //Danish
	    DE, //German
	    DZ, //Bhutani
	    EL, //Greek
	    EN, //English
	    EO, //Esperanto
	    ES, //Spanish
	    ET, //Estonian
	    EU, //Basque
	    FA, //Persian
	    FI, //Finnish
	    FJ, //Fiji
	    FO, //Faeroese
	    FR, //French
	    FY, //Frisian
	    GA, //Irish
	    GD, //Scots Gaelic
	    GL, //Galician
	    GN, //Guarani
	    GU, //Gujarati
	    HA, //Hausa
	    HI, //Hindi
	    HE, //Hebrew
	    HR, //Croatian
	    HU, //Hungarian
	    HY, //Armenian
	    IA, //Interlingua
	    ID, //Indonesian
	    IE, //Interlingue
	    IK, //Inupiak
	    IN, //former Indonesian
	    IS, //Icelandic
	    IT, //Italian
	    IU, //Inuktitut (Eskimo)
	    IW, //former Hebrew
	    JA, //Japanese
	    JI, //former Yiddish
	    JW, //Javanese
	    KA, //Georgian
	    KK, //Kazakh
	    KL, //Greenlandic
	    KM, //Cambodian
	    KN, //Kannada
	    KO, //Korean
	    KS, //Kashmiri
	    KU, //Kurdish
	    KY, //Kirghiz
	    LA, //Latin
	    LN, //Lingala
	    LO, //Laothian
	    LT, //Lithuanian
	    LV, //Latvian, Lettish
	    MG, //Malagasy
	    MI, //Maori
	    MK, //Macedonian
	    ML, //Malayalam
	    MN, //Mongolian
	    MO, //Moldavian
	    MR, //Marathi
	    MS, //Malay
	    MT, //Maltese
	    MY, //Burmese
	    NA, //Nauru
	    NE, //Nepali
	    NL, //Dutch
	    NO, //Norwegian
	    OC, //Occitan
	    OM, //(Afan) Oromo
	    OR, //Oriya
	    PA, //Punjabi
	    PL, //Polish
	    PS, //Pashto, Pushto
	    PT, //Portuguese
	    QU, //Quechua
	    RM, //Rhaeto-Romance
	    RN, //Kirundi
	    RO, //Romanian
	    RU, //Russian
	    RW, //Kinyarwanda
	    SA, //Sanskrit
	    SD, //Sindhi
	    SG, //Sangro
	    SH, //Serbo-Croatian
	    SI, //Singhalese
	    SK, //Slovak
	    SL, //Slovenian
	    SM, //Samoan
	    SN, //Shona
	    SO, //Somali
	    SQ, //Albanian
	    SR, //Serbian
	    SS, //Siswati
	    ST, //Sesotho
	    SU, //Sudanese
	    SV, //Swedish
	    SW, //Swahili
	    TA, //Tamil
	    TE, //Tegulu
	    TG, //Tajik
	    TH, //Thai
	    TI, //Tigrinya
	    TK, //Turkmen
	    TL, //Tagalog
	    TN, //Setswana
	    TO, //Tonga
	    TR, //Turkish
	    TS, //Tsonga
	    TT, //Tatar
	    TW, //Twi
	    UG, //Uigur
	    UK, //Ukrainian
	    UR, //Urdu
	    UZ, //Uzbek
	    VI, //Vietnamese
	    VO, //Volapuk
	    WO, //Wolof
	    XH, //Xhosa
	    YI, //Yiddish
	    YO, //Yoruba
	    ZA, //Zhuang
	    ZH, //Chinese
	    ZU  //Zulu
	
	}
}
