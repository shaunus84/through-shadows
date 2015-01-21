#if (UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN) && (!UNITY_IPHONE && !UNITY_WEBPLAYER)
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;

static public class Zip {
	static public void CompressFolder(string aFolderName, string aFullFileOuputName, string[] ExcludedFolderNames, string[] ExcludedFileNames){
		// Perform some simple parameter checking.  More could be done
		// like checking the target file name is ok, disk space, and lots
		// of other things, but for a demo this covers some obvious traps.
		if (!Directory.Exists(aFolderName)) {
			Debug.Log("Cannot find directory : " + aFolderName);
			return;
		}

		try
		{
			string[] exFileNames = new string[0];
			string[] exFolderNames = new string[0];
			if(ExcludedFileNames != null) exFileNames = ExcludedFileNames;
			if(ExcludedFolderNames != null) exFolderNames = ExcludedFolderNames;
			// Depending on the directory this could be very large and would require more attention
			// in a commercial package.
			List<string> filenames = GenerateFolderFileList(aFolderName, null);
			
			//foreach(string filename in filenames) Debug.Log(filename);
			// 'using' statements guarantee the stream is closed properly which is a big source
			// of problems otherwise.  Its exception safe as well which is great.
			using (ZipOutputStream zipOut = new ZipOutputStream(File.Create(aFullFileOuputName))){
			zipOut.Finish();
			zipOut.Close();
			}
			using(ZipFile s = new ZipFile(aFullFileOuputName)){
					s.BeginUpdate();
					int counter = 0;
					//add the file to the zip file
				   	foreach(string filename in filenames){
						bool include = true;
						string entryName = filename.Replace(aFolderName, "");
						//Debug.Log(entryName);
						foreach(string fn in exFolderNames){
							Regex regEx = new Regex(@"^" + fn.Replace(".",@"\."));
							if(regEx.IsMatch(entryName)) include = false;
						}
						foreach(string fn in exFileNames){
							Regex regEx = new Regex(@"^" + fn.Replace(".",@"\."));
							if(regEx.IsMatch(entryName)) include = false;
						}
						if(include){
							s.Add(filename, entryName);
						}
						counter++;
					}
				    //commit the update once we are done
				    s.CommitUpdate();
				    //close the file
				    s.Close();
				}
		}
		catch(Exception ex)
		{
			Debug.Log("Exception during processing" + ex.Message);
			
			// No need to rethrow the exception as for our purposes its handled.
		}
	}
	
	static List<string> GenerateFolderFileList(string aFolderName, List<string> anIterator){
		List<string> result = new List<string>();
		if(anIterator != null) result = anIterator;
        foreach (string file in Directory.GetFiles(aFolderName)) // add each file in directory
        {
            Regex regEx = new Regex(@"/\.");
			if(!regEx.IsMatch(file)){
				result.Add(file);
			}
        }
       
        foreach (string dirs in Directory.GetDirectories(aFolderName)) // recursive
        {
            GenerateFolderFileList(dirs, result);
        }
        return result; 	
	}

}
#endif
