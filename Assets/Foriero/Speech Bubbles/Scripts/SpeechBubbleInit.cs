using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections;
using System.IO;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Localization")]
	[Tooltip("Init a SpeechBubble Engine. Call this only once, when the scene starts or if you need to load a new languge font or change default settings for Speech Bubble Action.")]
	public class SpeechBubbleInit : FsmStateAction
	{
		public enum PlacementEnum{
			Center = 1, RightCenter = 2, LeftCenter = 3, TopRight = 4, TopCenter = 5, TopLeft = 6, BottomRight = 7, BottomCenter = 8, BottomLeft = 9	
		}
		
		[RequiredField]
		[Tooltip("A skin where your Speech Bubble style resides.")]
		public GUISkin skin;
		[RequiredField]
		[Tooltip("You can have more bubble style in your skin. This sets the default skin the Speech Bubble Action will use.")]
		public string style;
		[RequiredField]
		[Tooltip("E.g. chinese language needs special font, you need to assign it here in order to see it properly in your Speech Bubbles.")]
		public Font font;
		[RequiredField]
		[Tooltip("Minimal pixel size of a bubble when text is too short to fit paddings and boarders.")]
		public Vector2 minBubbleSize;
		[RequiredField]
		[Tooltip("Width and height of a Speech Bubble is calculated according these values. X for character widht and Y for character or line height. If you don't use a monospace font, be carefull to set and avarage character width.")]
		public int lineCharacterCount;
		[RequiredField]
		[Tooltip("Gui Depth")]
		public int guiDepth;
		[RequiredField]
		[Tooltip("Is a default offset from the point of your placement.")]
		public float bubbleOffset;
		[RequiredField]
		[Tooltip("Is a user offset from the point of your placement.")]
		public Vector2 bubbleOffsetVector;
		[RequiredField]
		[Tooltip("Specifies a placement of a Speech Bubble.")]
		public PlacementEnum placement = PlacementEnum.Center;
		[RequiredField]
		[Tooltip("GUI color of of your Speech Bubble. Default is (1,1,1,1).")]
		public Color color;
		[RequiredField]
		[Tooltip("A time to fade in your speech bubble.")]
		public float fadeIn;
		[RequiredField]
		[Tooltip("A time to fade out your speech bubble. WARNING. If you call a transition from other action than Speech Bubble one you don't get the fade out effect.")]
		public float fadeOut;
		[Tooltip("Default Audio for a Speech Bubble.")]
		public AudioClip defaultAudioClip;
		[RequiredField]
		[Tooltip("Default volume setting for a Speech Bubble.")]
		public float volume;
		[Tooltip("Default audio delay setting for a Speech Bubble.")]
		public float audioDelay;
		[Tooltip("Default audio pitch setting for a Speech Bubble.")]
		public float audioPitch;
		
		[Tooltip("Global Audio Volume - if not set default (1) is used or local bubble's one")]
		public FsmFloat audioVolume;
		[Tooltip("Either multiply global volume with local one or use glogal one is when true")]
		public bool audioVolumeMultiply;
		
		public bool debug;
							
		public override void Reset()
		{
			skin = null;
			style = null;
			font = null;
			minBubbleSize = new Vector2(64f, 64f);
			bubbleOffset = 0f;
			bubbleOffsetVector = Vector2.zero;
			placement = PlacementEnum.Center;
			color = Color.white;
			fadeIn = 0.3f;
			fadeOut = 0.3f;
			volume = 1f;
			audioDelay = 0f;
			guiDepth = 0;
			debug = false;
			audioVolume = null;
			audioVolumeMultiply = false;
			audioPitch = 1f;
		}

		public override void OnEnter()
		{
			SpeechBubble._initialized = false;
			SpeechBubble._skin = skin;
			SpeechBubble._skin.font = font;
			SpeechBubble._style = style;
			SpeechBubble._font = font;
			SpeechBubble._minBubbleSize = minBubbleSize;
			SpeechBubble._offset = bubbleOffset;
			SpeechBubble._offsetVector = bubbleOffsetVector;
			SpeechBubble._placement = (SpeechBubble.PlacementEnum)placement;
			SpeechBubble._color = color;
			SpeechBubble._fadeIn = fadeIn;
			SpeechBubble._fadeOut = fadeOut;
			SpeechBubble._guiDepth = guiDepth;
			SpeechBubble._debug = debug;
			SpeechBubble._audioVolume = audioVolume.IsNone ? 1f : audioVolume.Value;
			SpeechBubble._audioVolumeMultiply = audioVolumeMultiply;
			SpeechBubble._audioPitch = audioPitch;
			
			SpeechBubble._defaultAudioClip = defaultAudioClip;
			SpeechBubble._volume = volume;
			SpeechBubble._audioDelay = audioDelay;
			SpeechBubble.textLineLengthGUI = lineCharacterCount;
								
			SpeechBubble._initialized = true;
			Finish();
		}		
	}
}