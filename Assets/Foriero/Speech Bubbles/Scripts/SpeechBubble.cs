using UnityEngine;
using HutongGames.PlayMaker;
using System;
using System.Collections;



namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Localization")]
	[Tooltip("Speech Bubble Action lets you show styled GUI bubble. It can be related to a GameObject position or to a Screen resolution.")]
	public class SpeechBubble : FsmStateAction
	{
		#region Static Vars for global use
		
		public enum PlacementEnum{
			Default = 0, Center = 1, RightCenter = 2, LeftCenter = 3, TopRight = 4, TopCenter = 5, TopLeft = 6, BottomRight = 7, BottomCenter = 8, BottomLeft = 9	
		}
	
		static public GUISkin _skin = null;
		static public string _style = "";
		static public Font _font;
		static public bool _initialized = false;
		static public float _offset = 0f;
		static public Vector2 _offsetVector = Vector2.zero;
		static public Color _color;
		static public float _fadeIn = 0.3f;
		static public float _fadeOut = 0.3f;
		static public AudioClip _defaultAudioClip;
		static public float _volume = 1f;
		static public float _audioDelay = 0f;
		static public PlacementEnum _placement = PlacementEnum.TopCenter;
		static public Vector2 _minBubbleSize = new Vector2(64, 64);
		static public int _guiDepth = 0;
		static public bool _debug = false;
		static public float _audioVolume = 1f;
		static public bool _audioVolumeMultiply = false;
		static public float _audioPitch = 1f;
		
		static public int lineThickness = 8;
		static public int bubbleOverArrowTip = 16;
		static public int textLineLengthGUI = 45;
		static public int textLineLengthBillboard = 45;
		
		#endregion
			
		#region Fsm Fields
		
		public enum CornerEnum{ Default, BottomRight, BottomLeft, UpperRight, UpperLeft };
		
		[RequiredField]
		public FsmOwnerDefault gameObject;
		[Tooltip("Speech Bubble style defined in your skin. If IsNone then Default is used. You can set default values in Speech Bubble Init.")]
		public FsmString style;
		[Tooltip("Color your Speech Bubble.If IsNone then Default is used. You can set default values in Speech Bubble Init.")]
		public FsmColor color;
		[ActionSection("Position")]
		[Tooltip("Selects multistyle corners.")]
		public CornerEnum corner;
		[Tooltip("Placement point. If IsNone then Default is used. You can set default values in Speech Bubble Init.")]
		public PlacementEnum placement = PlacementEnum.Center;
		[Tooltip("Offset from a placement point. If IsNone then Default is used. You can set default values in Speech Bubble Init.")]
		public FsmFloat offset;
		[Tooltip("If you want to specify screen position unrelated to your GameObject, this is the right place to do it.")]
		public FsmFloat default_x;
		[Tooltip("If you want to specify screen position unrelated to your GameObject, this is the right place to do it.")]
		public FsmFloat default_y;
		[Tooltip("If true then default_x and default_y will be normalized.")]
		public bool normalized = true;
		[Tooltip("Screen or GUI depth of your Speech Bubbles.")]
		public FsmInt depth;
		[ActionSection("Text")]
		[Tooltip("If IsNone a default dicionary alias is used. You can set it with Set Default Dictionary Action. Please don't forget to call Dictionary Init Action, it will start the dictionary engine.")]
		public FsmString dictionary;
		[Tooltip("This is Id of your languate entry. If IsNone or the entry is not found then the 'text' parameter below will be used instead.")]
		public FsmString id;
		[Tooltip("Speech Bubble's text. Use it in the case you don't use a dictionary features.")]
		public FsmString text;
		[ActionSection("Time")]
		[Tooltip("How lond should be a Speech Bubble visible before finishedEvent is called.")]
		public FsmFloat time;
		[Tooltip("Delayed Speech Bubble popup.")]
		public FsmFloat delay;
		[Tooltip("Event called after the 'time' parametr will expire.")]
		public FsmEvent finishedEvent;
		[ActionSection("Audio")]
		[Tooltip("You can specify a GameObject that will be used to monitor its audio time in order to call the audioFinishedEvent.")]
		public FsmGameObject audioGameObject;
		[Tooltip("Audio to be played.")]
		public AudioClip audioClip;
		[Tooltip("Volume. If IsNone then a default value is used. You can set default values in Speech Bubble Init.")]
		public FsmFloat volume;
		[Tooltip("Audio Delay measured from the start of your Speech Bubble.")]
		public FsmFloat audioDelay;
		[Tooltip("Used for multilungual purposes. If IsNone then language 'id' is used instead. You need to store your audios in Resources/LanguageAudios/'dictionary alias'/'language e.g.EN'/audioId. Real example Resources/LanguageAudios/MYDICTIONARY/EN/001 were 001 is your audioId.")]
		public FsmString audioId;
		[Tooltip("You need to check this in order to use language aduio features.")]
		public bool loadLanguageAudio = false;
		[Tooltip("You might want to finish a state earlier than the audio will finish its playing. Here you can specify it. The comparison is done against the playing audio time.")]
		public FsmFloat greaterThanFinish;
		[Tooltip("This will be called when your audio have finished playing or if you specify the 'greaterThanFinish' parameter.")]
		public FsmEvent audioFinishedEvent;
		[Tooltip("Stops audio's palying.")]
		public bool stopAudioOnFinished;
		[ActionSection("Fading")]
		[Tooltip("Fade In Time. If IsNone then a default value is used. You can set default values in Speech Bubble Init.")]
		public FsmFloat fadeIn;
		[Tooltip("Fade Out Time. If IsNone then a default value is used. You can set default values in Speech Bubble Init.")]
		public FsmFloat fadeOut;
		[Tooltip("Speech Bubble is fading in. Would you like to fade in also your audio?")]
		public bool fadeAudioIn;
		[Tooltip("Speech Bubble is fading out. Would you like to fade out also your audio?")]
		public bool fadeAudioOut;
		[ActionSection("Mouse Exit")]
		public bool mouseExit;
		public float rayDistance;
		public FsmInt[] layerMask;
		public bool invertMask;
		
		public bool everyFrame;
		
		public FsmEvent recordingNextEvent;
		public FsmEvent recordingPrevEvent;
		#endregion
		
		#region Private Vars
		
		GameObject go;
		GameObject goaudio;
		
		string guiText = "";
		string rawText = "";
		Lang.LanguageCode lastLanguage = Lang.LanguageCode.Unassigned;
		Rect guiRect = new Rect(0f,0f,20f,10f);
		
		GUIStyle bubbleStyle;
		float bubbleOffset = 0.3f;
		Vector2 bubbleOffsetVector = Vector2.zero;
		
		
		Vector2 bubbleSizeTmp;
		GUISkin skinTmp;
		bool showText = false;
		
		
		bool initAgain = false;
		float delayTmp = 0f;
		float timeTmp = 0f;
		int depthTmp = 0;
		int guiDepth = 0;
		
		AudioSource audio;
		bool waitForAudioFinish = false;
		
		float alpha = 0f;
		Color colorTmp;
		bool audioFinished = false;
		float audioSourceVolume = 1f;
		bool audioStarted = false;
		float audioDelayTmp = 0f;
		bool audioDelaySet = false;
				
		BubbleState bubbleState = SpeechBubble.BubbleState.none; 
		
		#endregion
		
		#region Animation Easing
		
		EaseType easeType = EaseType.linear;
		protected delegate float EasingFunction(float start, float end, float value);
		protected EasingFunction ease;
		string lastId = "";
		
		#endregion
		
		bool _appendLanguage = false;
		
		public override void Reset()
		{
		  	recordingNextEvent = null;
			recordingPrevEvent = null;
								
			style = new FsmString{UseVariable = true};
			placement = PlacementEnum.Default;
			offset = new FsmFloat{UseVariable = true};
			dictionary = new FsmString{UseVariable = true};
			id = new FsmString{Value = "0000"};
			text = new FsmString{UseVariable = true};
			time = new FsmFloat{UseVariable = true};
			delay = new FsmFloat{UseVariable = true};
			finishedEvent = null;
			default_x = new FsmFloat{UseVariable = true};
			default_y = new FsmFloat{UseVariable = true};
			normalized = true;
			delayTmp = 0f;
			timeTmp = 0f;
			depth = null;
			depthTmp = 0;
			audioGameObject = new FsmGameObject{UseVariable = true};
			audioClip = null;
			audioId = new FsmString{UseVariable = true};
			audioDelay = new FsmFloat{UseVariable = true};
			loadLanguageAudio = false;
			audioFinishedEvent = null;
			volume = null;
			color = new FsmColor{UseVariable = true};
			stopAudioOnFinished = false;
			greaterThanFinish = new FsmFloat{UseVariable = true};
				
			fadeIn = new FsmFloat{UseVariable = true};
			fadeOut = new FsmFloat{UseVariable = true};
			fadeAudioIn = false;
			fadeAudioOut = false;	
			waitForAudioFinish = false;
			mouseExit = false;
			rayDistance = 100f;
			layerMask = new FsmInt[0];
			invertMask = false;
			corner = SpeechBubble.CornerEnum.Default;
			
			everyFrame = false;
		}
		
		enum BubbleState{
			none,
			effectin,
			bubble,
			effectout,
			finishing
		}
		
	    						
		public override void OnEnter()
		{
			go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value;
			if(go == null) {
				Finish();
				return;
			}
									
			delayTmp = delay.IsNone ? 0f : delay.Value;
			timeTmp = time.IsNone ? 0f : time.Value;
			depthTmp = depth.IsNone ? _guiDepth : depth.Value;
			waitForAudioFinish = false;
			alpha = 0f;
			bubbleState = SpeechBubble.BubbleState.none;
			audioFinished = false;
			if(!id.IsNone) lastId = id.Value;
			if(!audioGameObject.IsNone) goaudio = audioGameObject.Value; 
			if(goaudio == null) goaudio = go;
			if(goaudio) if(goaudio.audio) audioSourceVolume = goaudio.audio.volume;
			audioStarted = false;
			audioDelaySet = false;
			InitSpeechBubble();
		}
		
		public override void OnExit(){
			if(stopAudioOnFinished) if(goaudio.audio) goaudio.audio.Stop();	
		}
		
		private void InitSpeechBubble(){
			if(_initialized){
				initAgain = false;
				if(!style.IsNone) {
					bubbleStyle = _skin.GetStyle(style.Value + "_" + System.Enum.GetName(typeof(CornerEnum), corner).ToLower());
					if(bubbleStyle == null) bubbleStyle = _skin.GetStyle(style.Value);
				} else {
					bubbleStyle = _skin.GetStyle(_style + "_" + System.Enum.GetName(typeof(CornerEnum), corner).ToLower());
					if(bubbleStyle == null) {
						bubbleStyle = _skin.GetStyle(_style);
					}
				}
#if UNITY_IPHONE
				if(bubbleStyle != null) {
					bubbleStyle.fontSize = 0;			
					bubbleStyle.fontStyle = FontStyle.Normal;
				}
#endif
				if(!offset.IsNone) bubbleOffset = offset.Value;
				else bubbleOffset = _offset;
				bubbleOffsetVector = _offsetVector;
				if(!id.IsNone) {
					rawText = Lang.GetText(
	                                      dictionary.IsNone ? "" : dictionary.Value,
	                                       id.Value,
										   text.IsNone ? "" : text.Value.Trim()
	                                       );
					lastLanguage = Lang.selectedLanguage;
				} else {
					rawText = text.IsNone ? "" : text.Value.Trim();
				}
				Say(rawText);
				bubbleState = SpeechBubble.BubbleState.effectin;
			} else {
				initAgain = true;	
			}
		}
		
		public void ReInitSayText(){
			if(!id.IsNone) {
				rawText = Lang.GetText(
	                                  dictionary.IsNone ? "" : dictionary.Value,
	                                   id.Value,
									   text.IsNone ? "" : text.Value.Trim()
	                                   );
				lastLanguage = Lang.selectedLanguage;
			} else {
				rawText = text.IsNone ? "" : text.Value.Trim();
			}
			
			Say(rawText);	
		}
					
		void CheckMouseExit(){
		 	RaycastHit hitInfo = ActionHelpers.MousePick(rayDistance, ActionHelpers.LayerArrayToLayerMask(layerMask, invertMask));
			bool didPick = hitInfo.collider != null;
			if(didPick == false){
				mouseExit = true;	
				bubbleState = SpeechBubble.BubbleState.effectout;
			} else {
				if(go.name != hitInfo.collider.gameObject.name){
					mouseExit = true;
					bubbleState = SpeechBubble.BubbleState.effectout;
				}
			}
		}
		
		private void PlayAudio(){
			if(_defaultAudioClip != null) {
				if(goaudio.audio == null) goaudio.AddComponent<AudioSource>();	
				goaudio.audio.PlayOneShot(_defaultAudioClip, _volume);
			}
			
			goaudio.audio.pitch = _audioPitch;
			
			if(audioClip != null || loadLanguageAudio){
				if(loadLanguageAudio) {
					string path = "LanguageAudios/"
					                           +  (dictionary.IsNone ? "" : dictionary.Value) +"/"           
					                           +  Lang.selectedLanguage.ToString() + "/" 
					                           +  (audioId.IsNone ? id.Value : audioId.Value);
					audioClip = (AudioClip)Resources.Load(path, typeof(AudioClip));
					
					if(audioClip == null) {
						Debug.Log((id.IsNone ? "NO ID" : id.Value) + "  :  " + "NO AUDIO FOUND");	
						Debug.Log(path);
					}
				}
								
				if(audioClip != null){
					if(goaudio.audio == null) goaudio.AddComponent<AudioSource>();
					audio = goaudio.audio;
					audio.playOnAwake = false;
					if (audio != null)
					{
						if (audioClip == null)
						{
							if (volume.IsNone){
								audio.volume = _audioVolume;
							} else {
								if(_audioVolumeMultiply){
									audio.volume = _audioVolume * volume.Value;
								} else {
									audio.volume = volume.Value;
								}
							}
							if(_debug) Debug.Log("Audio Volume : " + audio.volume);
							audio.Play();
																					
							return;
						}
						else
						{
							audio.clip = audioClip;
							if (volume.IsNone){
								audio.volume = _audioVolume;
							} else {
								if(_audioVolumeMultiply){
									audio.volume = _audioVolume * volume.Value;
								} else {
									audio.volume = volume.Value;
								}
							}
							if(_debug) Debug.Log("Audio Volume : " + audio.volume);
							audio.Play();
							
							if(audioFinishedEvent != null) waitForAudioFinish = true;
							return;
						}
					}
				}
			}
		}
			
		public override void OnUpdate(){
			if(everyFrame) ReInitSayText();
				
			if(bubbleState != SpeechBubble.BubbleState.finishing){
				if(delayTmp >= 0f) {
					delayTmp -= Time.deltaTime;	
				} else {
					if(mouseExit) CheckMouseExit();
					if((lastLanguage != Lang.selectedLanguage) || (!id.IsNone && lastId != id.Value)){
					 	lastLanguage = Lang.selectedLanguage;
						if(!id.IsNone) {
						lastId = id.Value;
						rawText = Lang.GetText(
		                                      dictionary.IsNone ? "" : (dictionary.Value + (_appendLanguage ? "_" + Lang.selectedLanguage.ToString() : "")),
		                                       id.Value,
											   text.IsNone ? "" : text.Value.Trim()
		                                       );
						} else {
							rawText = text.IsNone ? "" : text.Value.Trim();
						}
						Say(rawText);
					}
					if(_initialized && delayTmp < 0f ){	
						if(!audioDelaySet){
							audioDelayTmp = audioDelay.IsNone ? _audioDelay : audioDelay.Value;
							audioDelaySet = true;
						}
						if(!audioStarted) {
							if(audioDelayTmp > 0f){
								audioDelayTmp -= Time.deltaTime;	
							} else {
							   PlayAudio();
							   audioStarted = true;
							}
						}
						if(bubbleState == SpeechBubble.BubbleState.effectin && alpha <= 1f) {
							alpha += (1f/(fadeIn.IsNone ? _fadeIn : fadeIn.Value))*Time.deltaTime;
							if(alpha > 1f) {
								alpha = 1f;
								bubbleState = SpeechBubble.BubbleState.bubble;
							}
							if(fadeAudioIn) if(goaudio && goaudio.audio) {
								goaudio.audio.volume = audioSourceVolume + (((volume.IsNone? _volume : volume.Value) - audioSourceVolume) * alpha);
							}
							
						}
						if(bubbleState == SpeechBubble.BubbleState.effectout && alpha >= 0f) {
							alpha -= (1f/(fadeOut.IsNone ? _fadeOut : fadeOut.Value))*Time.deltaTime;
							if(alpha < 0f) {
								alpha = 0f;
								bubbleState = SpeechBubble.BubbleState.finishing;
								Finish();
								if(audioFinished) {
									if(audioFinishedEvent != null) Fsm.Event(audioFinishedEvent);
								} else {
									if(finishedEvent != null) Fsm.Event(finishedEvent);
								}
							}
							if(fadeAudioOut) if(goaudio && goaudio.audio) goaudio.audio.volume = (volume.IsNone? _volume : volume.Value) * alpha;
							return;
						}
					}
					if(!time.IsNone && bubbleState != SpeechBubble.BubbleState.effectout) {
						if(timeTmp > 0f) {
							timeTmp -= Time.deltaTime;
						} else {
							bubbleState = SpeechBubble.BubbleState.effectout;
						}
					}
				}
				if(!greaterThanFinish.IsNone && goaudio != null){
					if(goaudio.audio != null && goaudio.audio.clip != null){
						if(goaudio.audio.isPlaying && goaudio.audio.time > greaterThanFinish.Value - (fadeOut.IsNone ? _fadeOut : fadeOut.Value)) {
							audioFinished = true;
							bubbleState = SpeechBubble.BubbleState.effectout;	
						}
					}
				}
				
				if(waitForAudioFinish && bubbleState != SpeechBubble.BubbleState.effectout) {
					if(audio.isPlaying && audio.time > audio.clip.length - (fadeOut.IsNone ? _fadeOut : fadeOut.Value)) {
						audioFinished = true;
						bubbleState = SpeechBubble.BubbleState.effectout;	
					}
				}
			}
		}
				
		public override void OnGUI(){
			if(Application.isPlaying){
				GUI.skin = _skin;
				guiDepth = GUI.depth;
				GUI.depth = depthTmp;
				if(_initialized && delayTmp <= 0 ){
					if(initAgain) {
						initAgain = false;
						InitSpeechBubble();
					}
					if (!guiText.Equals("") && showText){ 
						skinTmp = GUI.skin;
						GUI.skin = _skin;
						colorTmp = GUI.color;
						if(color.IsNone) GUI.color = new Color(_color.r, _color.b, _color.b, alpha);
						else GUI.color = new Color(color.Value.r, color.Value.b, color.Value.b, alpha);
						
						Vector2 bubbleSize = new Vector2(guiRect.width, guiRect.height);
						Vector2 topLeftPosition = GetbubblePosition(bubbleSize, placement);
						if(!default_x.IsNone) if(normalized) topLeftPosition.x = default_x.Value*Screen.width; else topLeftPosition.x = default_x.Value;
						if(!default_y.IsNone) if(normalized) topLeftPosition.y = default_y.Value*Screen.height; else topLeftPosition.y = default_y.Value;
						
						if(bubbleStyle == null) {
							if(showText) {
								GUI.Box(new Rect (topLeftPosition.x, topLeftPosition.y, guiRect.width, guiRect.height),guiText);
							} else {
								GUI.Box(new Rect (topLeftPosition.x, topLeftPosition.y, guiRect.width, guiRect.height),"");
							}
						} else {
							if(showText) {
								GUI.Box(new Rect (topLeftPosition.x, topLeftPosition.y, guiRect.width, guiRect.height),guiText, bubbleStyle);
							} else {
								GUI.Box(new Rect (topLeftPosition.x, topLeftPosition.y, guiRect.width, guiRect.height),"", bubbleStyle);
							}
						}
						GUI.skin = skinTmp;
						GUI.color = colorTmp;
						if(SpeechBubbleDebug._debug) {
							GUI.Box(new Rect (topLeftPosition.x, topLeftPosition.y - 40f, 100f, 40f),id.IsNone ? "No ID" : id.Value);
						}
					}
				}
			
				GUI.depth = guiDepth;
			}
		}
										
		public void Say(string aText){
			showText = false;
			if(_debug){
				Debug.Log((id.IsNone ? "NO ID" : id.Value) + "  :  " + aText);
			}
			if(aText.Equals("")) return;
			WordWrap(aText, textLineLengthGUI);
			bubbleSizeTmp = GetbubbleSize(guiText);
			guiRect = new Rect(0f, 0f, bubbleSizeTmp.x, bubbleSizeTmp.y);
			showText = true;
		}

		private void WordWrap(string sourceText, int textLineLength){
			string wrappedText = "";
			bool done = false;
			int lines = 1;
			int longestLine=0;
			
			while(!done){
				//Debug.Log("0");
				//Debug.Log(sourceText + "JE TO SPATNY");
				string line;
				if (sourceText.Length <= textLineLength || textLineLength <= 0){
					line = sourceText;
					if(line.Length > longestLine) {
						longestLine = line.Length;
					}
					done = true;
				} else {
					int numberOfCharactersToSearch =  System.Math.Min(textLineLength, sourceText.Length);
					int lastIndexOfSpace = sourceText.LastIndexOf(" ", numberOfCharactersToSearch - 1, numberOfCharactersToSearch);
					int lastIndexOfNewLine = sourceText.LastIndexOf(@"\n", numberOfCharactersToSearch - 1, numberOfCharactersToSearch);
					int splitIndex;
					
					if (lastIndexOfNewLine != -1) {
						if (lastIndexOfNewLine < lastIndexOfSpace){
							lastIndexOfSpace = lastIndexOfNewLine;
						}
					}
					if (lastIndexOfSpace != -1){
						splitIndex = lastIndexOfSpace;
					} else {
						splitIndex = textLineLength;
					}
					line = sourceText.Substring(0, splitIndex) + "\n";
					if(line.Length > longestLine) {
						longestLine = line.Length;
					}
					lines++;
					sourceText = sourceText.Substring(splitIndex);
					if (sourceText.StartsWith(" ")){
						sourceText = sourceText.Substring(1);
					}
					if (sourceText.StartsWith(@"\n")){
						sourceText = sourceText.Substring(2);
					}
				}
				wrappedText = wrappedText + line;
			}
			wrappedText = wrappedText.Replace(@"\n","\n");
			guiText = wrappedText;
		}
	
	    private Vector2 GetbubbleSize(string wrappedText){
			
			if(_skin) {
				GUIStyle style = _skin.GetStyle("speech_default");
				Vector2 boxSize = style.CalcSize(new GUIContent(guiText));
				boxSize.x += style.border.left + style.padding.left + style.border.right + style.padding.right;
				boxSize.y += style.border.top + style.padding.top + style.border.bottom + style.padding.bottom;
				return boxSize;
			} else {
				Debug.LogWarning("SpeechBubble not set SKIN");
				return Vector2.zero;
			}
		}
		
			
		private Vector2 GetbubblePosition(Vector2 bubbleSize, PlacementEnum aPlacement){
			if(aPlacement == PlacementEnum.Default){
				aPlacement = (PlacementEnum)_placement;	
			}
	        //Top Left Position = tlp
	        Vector3 pos = new Vector3();
	        Vector2 tlp = new Vector2();
	        switch (aPlacement)
			{
			    case PlacementEnum.Center:
			    //Debug.Log(Camera.main);
				//Debug.Log(go);
				//Debug.Log("____");
				pos = Camera.main.WorldToScreenPoint(go.transform.position);
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			        tlp.x -= bubbleSize.x/2;
			        tlp.y -= bubbleSize.y/2;
			        break;
			    case PlacementEnum.TopCenter:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x + bubbleOffsetVector.x, go.transform.position.y + bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			    	tlp.x -= bubbleSize.x/2; 
			    	tlp.y -= bubbleSize.y;
			    break;
			    case PlacementEnum.TopLeft:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x -  bubbleOffset + bubbleOffsetVector.x, go.transform.position.y + bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			    	tlp.x -= bubbleSize.x; 
			    	tlp.y -= bubbleSize.y;
			    break;
			    case PlacementEnum.TopRight:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x  +  bubbleOffset + bubbleOffsetVector.x, go.transform.position.y + bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			      	tlp.y -= bubbleSize.y;
			    break;
			    case PlacementEnum.BottomCenter:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x + bubbleOffsetVector.x, go.transform.position.y - bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			    	tlp.x -= bubbleSize.x/2; 
			    break;
			    case PlacementEnum.BottomLeft:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x -  bubbleOffset + bubbleOffsetVector.x, go.transform.position.y - bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			    	tlp.x -= bubbleSize.x; 
			    break;
			    case PlacementEnum.BottomRight:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x  +  bubbleOffset + bubbleOffsetVector.x, go.transform.position.y - bubbleOffset + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;  
			    break;
			    case PlacementEnum.RightCenter:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x  +  bubbleOffset + bubbleOffsetVector.x, go.transform.position.y + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;
			        tlp.y -= bubbleSize.y/2;  
			    break;
			    case PlacementEnum.LeftCenter:
			        pos = Camera.main.WorldToScreenPoint(new Vector3(go.transform.position.x  - bubbleOffset + bubbleOffsetVector.x, go.transform.position.y + bubbleOffsetVector.y, go.transform.position.z));
			        tlp.x = pos.x;
			        tlp.y = Camera.main.pixelHeight - pos.y;
			        tlp.x -= bubbleSize.x;
			        tlp.y -= bubbleSize.y/2;  
			    break;
			    
		    }		
		    return tlp;	
		}
		
		//instantiates a cached ease equation refrence:
		protected void SetEasingFunction(){
			switch (easeType){
			case EaseType.easeInQuad:
				ease  = new EasingFunction(easeInQuad);
				break;
			case EaseType.easeOutQuad:
				ease = new EasingFunction(easeOutQuad);
				break;
			case EaseType.easeInOutQuad:
				ease = new EasingFunction(easeInOutQuad);
				break;
			case EaseType.easeInCubic:
				ease = new EasingFunction(easeInCubic);
				break;
			case EaseType.easeOutCubic:
				ease = new EasingFunction(easeOutCubic);
				break;
			case EaseType.easeInOutCubic:
				ease = new EasingFunction(easeInOutCubic);
				break;
			case EaseType.easeInQuart:
				ease = new EasingFunction(easeInQuart);
				break;
			case EaseType.easeOutQuart:
				ease = new EasingFunction(easeOutQuart);
				break;
			case EaseType.easeInOutQuart:
				ease = new EasingFunction(easeInOutQuart);
				break;
			case EaseType.easeInQuint:
				ease = new EasingFunction(easeInQuint);
				break;
			case EaseType.easeOutQuint:
				ease = new EasingFunction(easeOutQuint);
				break;
			case EaseType.easeInOutQuint:
				ease = new EasingFunction(easeInOutQuint);
				break;
			case EaseType.easeInSine:
				ease = new EasingFunction(easeInSine);
				break;
			case EaseType.easeOutSine:
				ease = new EasingFunction(easeOutSine);
				break;
			case EaseType.easeInOutSine:
				ease = new EasingFunction(easeInOutSine);
				break;
			case EaseType.easeInExpo:
				ease = new EasingFunction(easeInExpo);
				break;
			case EaseType.easeOutExpo:
				ease = new EasingFunction(easeOutExpo);
				break;
			case EaseType.easeInOutExpo:
				ease = new EasingFunction(easeInOutExpo);
				break;
			case EaseType.easeInCirc:
				ease = new EasingFunction(easeInCirc);
				break;
			case EaseType.easeOutCirc:
				ease = new EasingFunction(easeOutCirc);
				break;
			case EaseType.easeInOutCirc:
				ease = new EasingFunction(easeInOutCirc);
				break;
			case EaseType.linear:
				ease = new EasingFunction(linear);
				break;
			case EaseType.spring:
				ease = new EasingFunction(spring);
				break;
			case EaseType.bounce:
				ease = new EasingFunction(bounce);
				break;
			case EaseType.easeInBack:
				ease = new EasingFunction(easeInBack);
				break;
			case EaseType.easeOutBack:
				ease = new EasingFunction(easeOutBack);
				break;
			case EaseType.easeInOutBack:
				ease = new EasingFunction(easeInOutBack);
				break;
			case EaseType.elastic:
				ease = new EasingFunction(elastic);
				break;
			}
		}
		
		#region EaseType
		public enum EaseType{
			easeInQuad,
			easeOutQuad,
			easeInOutQuad,
			easeInCubic,
			easeOutCubic,
			easeInOutCubic,
			easeInQuart,
			easeOutQuart,
			easeInOutQuart,
			easeInQuint,
			easeOutQuint,
			easeInOutQuint,
			easeInSine,
			easeOutSine,
			easeInOutSine,
			easeInExpo,
			easeOutExpo,
			easeInOutExpo,
			easeInCirc,
			easeOutCirc,
			easeInOutCirc,
			linear,
			spring,
			bounce,
			easeInBack,
			easeOutBack,
			easeInOutBack,
			elastic,
			punch
		}
		#endregion
		
		#region Easing Curves
		protected float linear(float start, float end, float value){
			return Mathf.Lerp(start, end, value);
		}
		
		protected float clerp(float start, float end, float value){
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) / 2.0f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half){
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}else if ((end - start) > half){
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}else retval = start + (end - start) * value;
			return retval;
	    }
	
		protected float spring(float start, float end, float value){
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}
	
		protected float easeInQuad(float start, float end, float value){
			end -= start;
			return end * value * value + start;
		}
	
		protected float easeOutQuad(float start, float end, float value){
			end -= start;
			return -end * value * (value - 2) + start;
		}
	
		protected float easeInOutQuad(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value + start;
			value--;
			return -end / 2 * (value * (value - 2) - 1) + start;
		}
	
		protected float easeInCubic(float start, float end, float value){
			end -= start;
			return end * value * value * value + start;
		}
	
		protected float easeOutCubic(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}
	
		protected float easeInOutCubic(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value + 2) + start;
		}
	
		protected float easeInQuart(float start, float end, float value){
			end -= start;
			return end * value * value * value * value + start;
		}
	
		protected float easeOutQuart(float start, float end, float value){
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}
	
		protected float easeInOutQuart(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value + start;
			value -= 2;
			return -end / 2 * (value * value * value * value - 2) + start;
		}
	
		protected float easeInQuint(float start, float end, float value){
			end -= start;
			return end * value * value * value * value * value + start;
		}
	
		protected float easeOutQuint(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}
	
		protected float easeInOutQuint(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value * value * value + 2) + start;
		}
	
		protected float easeInSine(float start, float end, float value){
			end -= start;
			return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
		}
	
		protected float easeOutSine(float start, float end, float value){
			end -= start;
			return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
		}
	
		protected float easeInOutSine(float start, float end, float value){
			end -= start;
			return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
		}
	
		protected float easeInExpo(float start, float end, float value){
			end -= start;
			return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
		}
	
		protected float easeOutExpo(float start, float end, float value){
			end -= start;
			return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
		}
	
		protected float easeInOutExpo(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
			value--;
			return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
		}
	
		protected float easeInCirc(float start, float end, float value){
			end -= start;
			return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
		}
	
		protected float easeOutCirc(float start, float end, float value){
			value--;
			end -= start;
			return end * Mathf.Sqrt(1 - value * value) + start;
		}
	
		protected float easeInOutCirc(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
		}
	
		protected float bounce(float start, float end, float value){
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f)){
				return end * (7.5625f * value * value) + start;
			}else if (value < (2 / 2.75f)){
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}else if (value < (2.5 / 2.75)){
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}else{
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}
	
		protected float easeInBack(float start, float end, float value){
			end -= start;
			value /= 1;
			float s = 1.70158f;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}
	
		protected float easeOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value = (value / 1) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}
	
		protected float easeInOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value /= .5f;
			if ((value) < 1){
				s *= (1.525f);
				return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
			}
			value -= 2;
			s *= (1.525f);
			return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}
	
		protected float punch(float amplitude, float value){
			float s = 9;
			if (value == 0){
				return 0;
			}
			if (value == 1){
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * 1 - s) * (2 * Mathf.PI) / period));
	    }
		
		protected float elastic(float start, float end, float value){
			//Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
			end -= start;
			
			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;
			
			if (value == 0) return start;
			
			if ((value /= d) == 1) return start + end;
			
			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
				}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			
			return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
		}		
		
		#endregion	
	}
}