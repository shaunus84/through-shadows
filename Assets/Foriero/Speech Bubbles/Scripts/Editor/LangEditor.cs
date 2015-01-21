using UnityEngine;
using UnityEditor;
using System;
using System.Net;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonFx.Json;
using System.Data;
using System.Threading;

public class LangEditor : EditorWindow {
	public string OSXOpenOffice = "/Applications/OpenOffice.org.app/Contents/MacOS/scalc";
	public string WINOpenOffice = "C:\\Program Files (x86)\\OpenOffice.org 3\\program\\scalc";
	public bool async = false;
	
	public static string dataPath = Application.dataPath;
	
	[MenuItem ("Foriero/Language Tool &l")]
    static void Init () {
        // Get existing open window or if none, make a new one:
       EditorWindow.GetWindow(typeof(LangEditor));
    }
		
	void Update(){
		for(int i = 0; i< langItems.Count(); i++){
			if(langItems[i].state == ToolState.TXTSaved) {
				langItems[i].lastDownload = System.DateTime.Now.ToShortTimeString() + " " + System.DateTime.Now.ToShortDateString() ;
				langItems[i].state = ToolState.Ok;
				Repaint();
			}
		}
	}
		
	public static void ODSToDictionary(string aFileName){
		OdsReadWrite.OdsReaderWriter reader = new OdsReadWrite.OdsReaderWriter();
		System.Data.DataSet dataset = reader.ReadOdsFile(aFileName);
		System.Data.DataRow columnRow = null;		
		
		foreach(System.Data.DataTable table in dataset.Tables){
			string result = "";
			bool test = false;
			bool isDictionary = false;
			foreach(System.Data.DataRow row in table.Rows){
				string rowResult = "";
				if(!test) {
					test = true;
					if((row[0] as string).ToUpper() != "ID") break;
					columnRow = row;
					isDictionary = true;
				}
				int colId = 0;
				bool appendRow = true;
				foreach(System.Data.DataColumn column in table.Columns){
					
					if(colId == 0 && string.IsNullOrEmpty((row[column.ColumnName] as string))) {
						appendRow = false;
						break;
					}
					if(string.IsNullOrEmpty((columnRow[column.ColumnName] as string))) continue;
					
					string append = (row[column.ColumnName] as string);
					if(!string.IsNullOrEmpty(append)){
						append = append.Replace(";", "_semicolon");
						append = append.Replace("\n", @"\n");
					}
					if(rowResult == "")	rowResult = append;	
					else rowResult = rowResult + ";" + append;
					colId++;
				}
				if(appendRow){
					if(result == "") result = rowResult;
					else result = result + "\n" + rowResult;
				}
			}
			if(isDictionary){
				string fileName = dataPath + "/Resources/Dictionaries/" + table.TableName + ".txt";
				SaveToTxt(result, fileName, System.Text.Encoding.UTF8);
				Debug.Log("Saved dictionary : " + fileName);
			}
		}		
	}
	
	public static bool ByteArrayToFile(string _FileName, byte[] _ByteArray)
    {
        try
        {
            // Open file for reading
            System.IO.FileStream _FileStream = new System.IO.FileStream(_FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);

            // Writes a block of bytes to this stream using data from a byte array.
            _FileStream.Write(_ByteArray, 0, _ByteArray.Length);

            // close file stream
            _FileStream.Close();

            return true;
        }
        catch (Exception _Exception)
        {
            // Error
            Debug.Log("Exception caught in process: " + _Exception.Message);
        }

    // error occured, return false
        return false;
    }
	
	public enum ToolState{
		None,
		Downloading,
		Downloaded,
		Error,
		Canceled,
		ODSSaved,
		TXTSaved,
		Ok
	}
	
	[Serializable]
	public class LanguageItem{
		public string odsFileName = "";
		public string publicKey = "";
		public string odsURL{
			get{
				return "https://docs.google.com/spreadsheets/d/" + publicKey + "/export?format=ods";
			}
		}
		public string documentURL{
			get{
				return "https://docs.google.com/spreadsheet/ccc?key=" + publicKey;
			}
		}
		public string lastDownload = "";
		public float r = 1f;
		public float g = 1f;
		public float b = 1f;
		public float a = 1f;
		public bool selected = true;
		public string errorMessage = "";
		public volatile ToolState state = ToolState.None;
		private string threadOdsURL = "";
		private string threadOdsFileName = "";	
		private volatile bool isDone = true;
		private Thread thread;
		
		public LanguageItem(){
			isDone = true;	
		}
		
		public void SetColor(Color aColor){
			r = aColor.r;
			g = aColor.g;
			b = aColor.b;
			a = aColor.a;
		}
		
		public Color GetColor(){
			switch(state){
			case ToolState.None:
				SetColor(Color.gray);
			break;
			case ToolState.Downloading:
				SetColor(Color.blue);
			break;
			case ToolState.Downloaded:
				SetColor(Color.cyan);
			break;
			case ToolState.ODSSaved:
				SetColor(Color.yellow);
			break;	
			case ToolState.TXTSaved:
				SetColor(Color.white);
			break;
			case ToolState.Ok:
				SetColor(Color.green);
			break;
			case ToolState.Error:
				SetColor(Color.red);
			break;
			case ToolState.Canceled:
				SetColor(Color.red);
			break;
			}
			return new Color(r,g,b,a);
		}
		
		public void Download(bool async, Action onRepaint){
			threadOdsURL = odsURL;
			threadOdsFileName = dataPath + "/Resources Localization/" + odsFileName + ".ods";
			state = LangEditor.ToolState.None;
			if(async){
				if(isDone){
					isDone = false;
					if(onRepaint != null) onRepaint();
					thread = new Thread(ThreadDownload);
					thread.Start();
				}
			} else {
				state = LangEditor.ToolState.None;
				WWW www = new WWW(odsURL);
				state = LangEditor.ToolState.Downloading;
				while(!www.isDone){};
				
				if(!string.IsNullOrEmpty(www.error)){
					state = LangEditor.ToolState.Error;
				} else {
					state = LangEditor.ToolState.Downloaded;
					if(ByteArrayToFile(threadOdsFileName, www.bytes)){
						Debug.Log("Saved ods : " + threadOdsFileName);
						state = LangEditor.ToolState.ODSSaved;
						ODSToDictionary(threadOdsFileName);
						state = LangEditor.ToolState.TXTSaved;
					} else {
						Debug.LogError("LIKELY SHARING VIOLATION ON THE FILE : " + threadOdsFileName);	
					}
				}
				if(onRepaint != null) onRepaint();
			}
		}
				
		public void ThreadDownload(){
			try{ 
	            WebRequest request = WebRequest.Create (threadOdsURL);
	            request.Credentials = CredentialCache.DefaultCredentials;
	            WebResponse response = request.GetResponse ();
	            if(((HttpWebResponse)response).StatusCode == HttpStatusCode.OK) state = LangEditor.ToolState.Downloaded;
	            else {
					state = LangEditor.ToolState.Error;
					Debug.LogError(((HttpWebResponse)response).StatusDescription);
				}
				byte[] b;
				using(Stream s = response.GetResponseStream ()){;
	            	b = ReadFully(s);
				}
				response.Close ();
				if(state == ToolState.Downloaded){
					if(ByteArrayToFile(threadOdsFileName, b)){
						Debug.Log("Saved ods : " + threadOdsFileName);
						state = LangEditor.ToolState.ODSSaved;
						ODSToDictionary(threadOdsFileName);
						state = LangEditor.ToolState.TXTSaved;
					} else {
						Debug.LogError("LIKELY SHARING VIOLATION ON THE FILE : " + threadOdsFileName);	
					}
				}
			} catch ( Exception e ){
				state = LangEditor.ToolState.Error;
				Debug.Log(e.Message);
			} finally {	
				isDone = true;
			}
		}
	}
	
	public static byte[] ReadFully(Stream input)
	{
	    byte[] buffer = new byte[16*1024];
	    using (MemoryStream ms = new MemoryStream())
	    {
	        int read;
	        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
	        {
	            ms.Write(buffer, 0, read);
	        }
	        return ms.ToArray();
	    }
	}
	
	
	public class LanguageItems{
		public string OSXOpenOffice = "/Applications/OpenOffice.org.app/Contents/MacOS/scalc";
		public string WINOpenOffice = "C:\\Program Files (x86)\\OpenOffice.org 3\\program\\scalc";
		public bool async = false;
		public Lang.LanguageCode langCode = Lang.LanguageCode.Unassigned;
		public List<LanguageItem> langItems = new List<LanguageItem>();
	}
	
	[SerializeField]
	public List<LanguageItem> langItems = new List<LanguageItem>();
	LanguageItem langItem;
			
	Color backgroundColor;
	Color contentColor;
	
	Lang.LanguageCode langCode = Lang.LanguageCode.Unassigned;
	
	void OnGUI(){
		EditorGUILayout.BeginHorizontal();
						
		if(GUILayout.Button("Save")){
			Save();
			ShowNotification(new GUIContent("Dictionaries saved"));
		}
		
		if(GUILayout.Button("Load")){
			Load();
			ShowNotification(new GUIContent("Dictionaries loaded"));
		}
		
		if(GUILayout.Button("Add")){
			langItems.Add(new LanguageItem());
		}
										
		if(GUILayout.Button("Update Camera")){
			LangDictionaries ld = Camera.main.gameObject.GetComponent<LangDictionaries>() as LangDictionaries;
			if(ld == null) ld = Camera.main.gameObject.AddComponent<LangDictionaries>() as LangDictionaries;
			List<LanguageItem> selection = new List<LanguageItem>(from i in langItems where i.selected == true select i);
			ld.dictionaries.Clear();
			ld.langCode = langCode;
			for(int i = 0; i<selection.Count(); i++){
				string fileName = Application.dataPath + "/Resources Localization/" + selection[i].odsFileName + ".ods";
				if(System.IO.File.Exists(fileName)){
					OdsReadWrite.OdsReaderWriter reader = new OdsReadWrite.OdsReaderWriter();
					System.Data.DataSet dataset = reader.ReadOdsFile(fileName);
				
					foreach(System.Data.DataTable table in dataset.Tables){
						bool isDictionary = false;
						foreach(System.Data.DataRow row in table.Rows){
							if((row[0] as string).ToUpper() == "ID") isDictionary = true;
							else isDictionary = false;
							break;
						}
						if(isDictionary){
							ld.dictionaries.Add(new LangDictionaries.LangDictionary());
							ld.dictionaries.Last().aliasName = table.TableName;
							ld.dictionaries.Last().assetPath = "Dictionaries/" + table.TableName;
						}
					}		
				} else {
					Debug.LogError("FILE NOT EXISTS : " + fileName);	
				}
			}
			ShowNotification(new GUIContent("Camera.main dictionaries updated"));
		}
		
		langCode = (Lang.LanguageCode)EditorGUILayout.EnumPopup(langCode, GUILayout.Width(80));
		
		GUILayout.Label("Async", GUILayout.Width(40));
		async = EditorGUILayout.Toggle(async, GUILayout.Width(20));
		
		if(GUILayout.Button("Download from GoogleDocs")){
			for(int i = 0; i< langItems.Count; i++){
				if(langItems[i].selected){
					langItems[i].Download(async, ()=>{Repaint();});
				}
			}
		}
		
		
	
		EditorGUILayout.EndHorizontal();
		
		OSXOpenOffice = EditorGUILayout.TextField("OpenOffice(OSX)", OSXOpenOffice);
		WINOpenOffice = EditorGUILayout.TextField("OpenOffice(WIN)", WINOpenOffice);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Box("Filename",GUILayout.Width(263));
		//GUILayout.Box("ODS Url",GUILayout.Width(300));
		//GUILayout.Box("Document Url",GUILayout.Width(260));
		GUILayout.Box("Public Document Key", GUILayout.Width(363));
		GUILayout.Box("",GUILayout.Width(20));
		GUILayout.Box("Last Download",GUILayout.Width(140));
		GUILayout.Box("ODS->TXT",GUILayout.Width(80));
		GUILayout.Box("Delete",GUILayout.ExpandWidth(true));
		EditorGUILayout.EndHorizontal();
		backgroundColor = GUI.backgroundColor;
		contentColor = GUI.contentColor;
		for(int i = 0; i< langItems.Count(); i++){
			langItem = langItems[i];
			EditorGUILayout.BeginHorizontal();
			GUI.backgroundColor = langItems[i].GetColor();	
			langItem.odsFileName = GUILayout.TextField(langItem.odsFileName, GUILayout.Width(200));
			if(GUILayout.Button("Open",GUILayout.Width(60))){
				string filePath = Application.dataPath + "/Resources Localization/" + langItems[i].odsFileName + ".ods";
				if(!File.Exists(filePath)){
					File.Copy(Application.dataPath + "/Foriero/Localization/Templates/Localization.ods", filePath);
					AssetDatabase.Refresh();
				} 
				if(Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX){
					System.Diagnostics.Process.Start(OSXOpenOffice, "\"" + filePath + "\"");
				} else {
					System.Diagnostics.Process.Start(WINOpenOffice, "\"" + filePath.Replace("/",@"\") + "\"");
				}
			}
			langItem.publicKey = GUILayout.TextField(langItem.publicKey, GUILayout.Width(196));
			//langItem.odsURL = GUILayout.TextField(langItem.odsURL, GUILayout.Width(196));
			if(GUILayout.Button("Download",GUILayout.Width(100))){
				langItems[i].Download(async, ()=>{Repaint();});
			}
			//langItem.documentURL = GUILayout.TextField(langItem.documentURL, GUILayout.Width(196));
			if(GUILayout.Button("Open",GUILayout.Width(60))){
				Application.OpenURL(langItem.documentURL);	
			}
			langItem.selected = EditorGUILayout.Toggle(langItem.selected,GUILayout.Width(20));
						
			GUILayout.Box(langItems[i].lastDownload, GUILayout.Width(140));
			
			if(GUILayout.Button("ODS->TXT", GUILayout.Width(80))){
				string fileNameOds = Application.dataPath + "/Resources Localization/" + langItems[i].odsFileName + ".ods"; 
				if(!File.Exists(fileNameOds)){
					File.Copy(Application.dataPath + "/Foriero/Localization/Templates/Localization.ods", fileNameOds);
					AssetDatabase.Refresh();	
				}
				ODSToDictionary(fileNameOds);
			}
			if(GUILayout.Button("Delete")){
				langItems.Remove(langItems[i]);
				break;
			}
			
			if(langItem.errorMessage != "OK") GUI.contentColor = Color.red;
			GUI.backgroundColor = backgroundColor;
			GUI.contentColor = contentColor;
			EditorGUILayout.EndHorizontal();
		}
	}
	
	
	void Save(){
		LanguageItems litems = new LanguageItems();
		litems.langCode = langCode;
		litems.OSXOpenOffice = OSXOpenOffice;
		litems.WINOpenOffice = WINOpenOffice;
		litems.async = async;
		litems.langItems = new List<LanguageItem>(langItems);
		string s = JsonWriter.Serialize(litems);
		SaveToTxt(s, Application.dataPath + "/Resources Localization/langeditor.ini", System.Text.Encoding.UTF8);
		AssetDatabase.Refresh();
	}
	
	public static void SaveToTxt(string aString, string aFilePath, System.Text.Encoding anEncoding){
			if(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(aFilePath))){
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(aFilePath));
			}
			using( TextWriter tw = new StreamWriter(aFilePath, false, anEncoding) ){;
				tw.Write(aString);
				tw.Close();	
			}
	}
	
	public static string LoadFromTxt(string aFilePath, System.Text.Encoding anEncoding){
		string result = "";
		if(System.IO.File.Exists(aFilePath)){
			using( TextReader tr = new StreamReader(aFilePath, anEncoding)){;
				result = tr.ReadToEnd();				
			}
		} else {
			Debug.LogError("LoadFromTxt file not found : " + aFilePath);	
		}
		return result;
	}
	
	void Load(){
		string aFilePath = Application.dataPath + "/Resources Localization/langeditor.ini";
		if(System.IO.File.Exists(aFilePath)){
			string s = LoadFromTxt(aFilePath, System.Text.Encoding.UTF8);
			LanguageItems litems = JsonReader.Deserialize<LanguageItems>(s);
			if(litems == null) { 
				langItems = new List<LangEditor.LanguageItem>();
			} else {
				langItems = new List<LanguageItem>(litems.langItems);
				langCode = litems.langCode;	
				OSXOpenOffice = litems.OSXOpenOffice;
				WINOpenOffice = litems.WINOpenOffice;
				async = litems.async;
			}
		} else {
			langItems.Add(new LanguageItem());
			langItems.Last().publicKey = "0AlVMfPD5xIYAdGhKVGNlVkNydWZFc0RHLXlNTUNURWc";
			//langItems.Last().documentURL = "https://docs.google.com/spreadsheet/ccc?key=0AlVMfPD5xIYAdGhKVGNlVkNydWZFc0RHLXlNTUNURWc";
			langItems.Last().odsFileName = "Localization";
			//langItems.Last().odsURL = "https://docs.google.com/spreadsheet/pub?key=0AlVMfPD5xIYAdGhKVGNlVkNydWZFc0RHLXlNTUNURWc&output=ods";
			langItems.Last().selected = true;
		}
	}
			
	void OnEnable(){
		if(!Directory.Exists(Application.dataPath + "/Resources Localization")) {
			Directory.CreateDirectory(Application.dataPath + @"/Resources Localization");
			AssetDatabase.Refresh();
		}
		if(!Directory.Exists(Application.dataPath + "/Resources")) {
			Directory.CreateDirectory(Application.dataPath + @"/Resources");
			AssetDatabase.Refresh();
		}
		if(!Directory.Exists(Application.dataPath + "/Resources/Dictionaries")) {
			Directory.CreateDirectory(Application.dataPath + @"/Resources/Dictionaries");
			AssetDatabase.Refresh();	
		}
		dataPath = Application.dataPath;
		Load();
	}
	
	void OnDisable(){
		Save();	
	}
	
	void OnDestroy(){
		Save();	
	}
}