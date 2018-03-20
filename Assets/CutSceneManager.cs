using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

/* Dialogue Scene Manager v0.4b
 * 		By SolAZDev
 * 
 * 		ABOUT
 *	This is based on EasyDialogue (http://u3d.as/2NH) 
 * which is based on a choice-driven dialogue, see this 
 * the example to see what I mean. However this bare-bone
 * dialuge system doesn't come with a UI implementation,
 * you need to make it yourself. BUT it has an extra string
 * we can use to our advantage. choice.userData. You can add
 * a LOT of things, such as an argument system! Just like
 * you would with a command line. (PlayVideo file.mp4 --subs=file)
 * 
 * 		USAGE
 * Here's how it works.
 * You have your speakers (used as characters in this script),
 * your dialogues, and then the user data with all the arguments.
 * You can use as many arguments as you like but you need to set 
 * them in order... I'll explain in the argument section. You also
 * need to set up the object variables in the inspector. 
 * Yes/No text changes if there's 2 choices (simple dialogue).
 * Please do your set up according to the scrippt. you know what I mean.
 *
* A proper documentation is long overdue and incoming!
 */

public class CutSceneManager : MonoBehaviour {
	[Header ("Scene Details")]
	public DialogueFile file;
	public string fileUrl;
	public string SceneID;
	public bool FadeText = false;
	public AudioSource Voices;
	public AudioSource Sounds;
	public AudioSource BGMPlayer;
	public List<Animator> Actors;
	public List<Image> Actors2D;
	public string ReturnScene;

	[Space]
	[Header ("UI Settings")]
	public Text TextBox;
	public Button ContinueBtn;
	public Animator TextAnim;
	public List<Text> Buttons;
	public Image FlashFade, MiscImage;

	[Space]
	public UnityEvent ExitEvent;

	string Line;
	public DialogueManager manager;
	public Dialogue ActiveLine;
	public Dialogue.Choice ActiveChoice;

	[HideInInspector] public Vector3[] MoveList, RotateList;
	[HideInInspector] public float[] MoveRotSpeed;

	[HideInInspector] public bool fade, SceneIn, flash, UpGUI = false;
	// Use this for initialization
	public void Start () {
		if (fileUrl.Contains ("://")) {
			StartCoroutine (GetJSON (fileUrl));
			return;
		}
		if (file != null) {
			manager = DialogueManager.LoadDialogueFile (file);
		} else {
			return;
		}
		AfterStart ();
	}

	public void AfterStart () {
		ActiveLine = manager.GetDialogue (SceneID);

		MoveList = new Vector3[Actors.Count];
		RotateList = new Vector3[Actors.Count];
		MoveRotSpeed = new float[Actors.Count];

		for (int i = 0; i < Buttons.Count; i++) {
			Buttons[i].transform.parent.gameObject.SetActive (false);
		}
		ContinueBtn.interactable = true;
		SceneIn = true;
		fade = true;
		Continue ();
	}
	// Update is called once per frame
	public void Update () {
		//if(!UpGUI){StartCoroutine(UpdateGUI());}
		TextBox.text = Line;
		for (int i = 0; i < Actors.Count; i++) {
			if (Actors[i].transform.position != MoveList[i]) {
				Actors[i].transform.position = Vector3.Lerp (Actors[i].transform.position, MoveList[i], Time.deltaTime * MoveRotSpeed[i]);
			}
			if (Actors[i].transform.rotation.eulerAngles != RotateList[i]) {
				Actors[i].transform.rotation = Quaternion.Lerp (Actors[i].transform.rotation, Quaternion.Euler (RotateList[i]), Time.deltaTime * MoveRotSpeed[i]);
			}
		}
		if (fade) {
			if (SceneIn) {
				FlashFade.gameObject.SetActive (true);
				if (FlashFade.color != Color.clear) {
					FlashFade.color = Color.Lerp (FlashFade.color, Color.clear, Time.deltaTime * 8f);
				} else { FlashFade.gameObject.SetActive (false); fade = false; }
			} else {
				FlashFade.gameObject.SetActive (true);
				if (FlashFade.color != Color.black) {
					FlashFade.color = Color.Lerp (FlashFade.color, Color.black, Time.deltaTime * 8f);
				} else { FlashFade.gameObject.SetActive (false); fade = false; }
			}
		}
	}

	public void Initialize () {
		Start ();
		//Continue ();

	}

	public void SetScene (string scene) {
		SceneID = scene;
	}

	public void Continue () {
		if (ActiveLine.GetChoices ().Count == 1) {
			ContinueBtn.interactable = false;
			StopCoroutine (UpdateLine ());
			ActiveChoice = ActiveLine.GetChoices () [0];
			ActiveLine.PickChoice (ActiveChoice);
			StartCoroutine (UpdateLine ());
			ParseArguments ();
		}
		if (ActiveLine.GetChoices ().Count == 0) {
			SceneIn = false;
			fade = true;
			//Exit Command
			ExitEvent.Invoke ();
		}
	}

	public void AnswerQuestion (int yesno) {
		Line = System.String.Empty;
		StopCoroutine (UpdateLine ());
		Line = System.String.Empty;
		for (int i = 0; i < Buttons.Count; i++) {
			Buttons[i].transform.parent.gameObject.SetActive (false);
		}
		ContinueBtn.interactable = true;
		ActiveChoice = ActiveLine.GetChoices () [yesno];
		ActiveLine.PickChoice (ActiveChoice);
		StartCoroutine (UpdateLine ());
		ParseArguments ();
	}

	public void ExitScene () {
		UnityEngine.SceneManagement.SceneManager.LoadScene (ReturnScene);
	}
	public IEnumerator UpdateLine () {
		if (FadeText && TextAnim != null) {
			TextAnim.SetTrigger ("Fade");
			yield return new WaitForSeconds (.5f);
			Line = ActiveChoice.dialogue;
			yield return null;
		} else {
			Line = System.String.Empty;
			foreach (char letter in ActiveChoice.dialogue.ToCharArray ()) {
				Line += letter;
				yield return null;
			}
		}
		yield return new WaitForSecondsRealtime (.1f);
		ContinueBtn.interactable = true;

		if (ActiveLine.GetChoices ().Count > 1) {
			ContinueBtn.interactable = false;
			for (int i = 0; i < ActiveLine.GetChoices ().Count; i++) {
				Buttons[i].transform.parent.gameObject.SetActive (true);
				Buttons[i].text = ActiveLine.GetChoices () [i].dialogue;
			}
		}
		yield return null;
	}

	public IEnumerator UpdateGUI () {
		UpGUI = true;
		yield return null;
		//yield return new WaitForSecondsRealtime (0.01f);
		TextBox.text = Line;
		UpGUI = false;
	}

	public IEnumerator Flash () {
		FlashFade.gameObject.SetActive (true);
		FlashFade.color = Color.white;
		yield return new WaitForSecondsRealtime (.01f);
		FlashFade.color = Color.clear;
		FlashFade.gameObject.SetActive (false);
		yield return null;
	}

	public IEnumerator GetJSON (string url) {
		UnityWebRequest webReuest = UnityWebRequest.Get (url);
		yield return webReuest.SendWebRequest ();
		if (webReuest.isNetworkError || webReuest.isHttpError) {
			TextBox.text = "ERROR\n" + webReuest.error;
		} else {
			manager = DialogueManager.GetFileFromJSON (webReuest.downloadHandler.text);
			ActiveLine = manager.GetDialogue (SceneID);

			MoveList = new Vector3[Actors.Count];
			RotateList = new Vector3[Actors.Count];
			MoveRotSpeed = new float[Actors.Count];

			for (int i = 0; i < Buttons.Count; i++) {
				Buttons[i].transform.parent.gameObject.SetActive (false);
			}
			ContinueBtn.interactable = true;
			SceneIn = true;
			fade = false;
			AfterStart ();
		}
	}

	public void ParseArguments () {
		if (ActiveChoice.userData == "") { return; }
		string[] arguements;
		if (ActiveChoice.userData.Contains (" ")) {
			arguements = ActiveChoice.userData.Split (new [] { ' ' });
		} else {
			arguements = new string[1];
			arguements[0] = ActiveChoice.userData;
		}
		string[] argArgs;

		//The Parsing! Check the Usage

		//This is done originally in a specific order but I've expanded it to add more
		//arguments, such as 2+ animations per lines, sounds and voices played together
		//and other things.
		for (int i = 0; i < arguements.Length; i++) {

			//Animation
			if (arguements[i].Contains ("a_") || arguements[i].Contains ("PlayAnimation_	")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[1])) { //check if such an actor even exists
					switch (int.Parse (argArgs[2])) {
						case 0: //Set Trigger
							Actors[int.Parse (argArgs[1])].SetTrigger (argArgs[3]);
							break;
						case 1: //Set Bool true.
							Actors[int.Parse (argArgs[1])].SetBool (argArgs[3], true);
							break;
						case 2: //Set Bool false.
							Actors[int.Parse (argArgs[1])].SetBool (argArgs[3], false);
							break;
						case 3: //Hide!
							Actors[int.Parse (argArgs[1])].gameObject.SetActive (false);
							break;
						case 4: //Show!
							Actors[int.Parse (argArgs[1])].gameObject.SetActive (true);
							break;
					}
				}
			}

			//Moving an Actor
			if (arguements[i].Contains ("m_") || arguements[i].Contains ("Move_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[1])) {
					MoveList[int.Parse (argArgs[1])] = new Vector3 (
						(argArgs[2] != "N") ? float.Parse (argArgs[2]) : Actors[0].transform.position.x,
						(argArgs[3] != "N") ? float.Parse (argArgs[3]) : Actors[0].transform.position.y,
						(argArgs[4] != "N") ? float.Parse (argArgs[4]) : Actors[0].transform.position.z
					);
					MoveRotSpeed[int.Parse (argArgs[1])] = (argArgs.Length > 4) ? float.Parse (argArgs[4]) : 8f;
				}

			}

			//Rotate an Actor
			if (arguements[i].Contains ("r_") || arguements[i].Contains ("Rotate_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[1])) {
					RotateList[int.Parse (argArgs[1])] = new Vector3 (
						(argArgs[2] != "N") ? float.Parse (argArgs[2]) : Actors[0].transform.rotation.eulerAngles.x,
						(argArgs[3] != "N") ? float.Parse (argArgs[3]) : Actors[0].transform.rotation.eulerAngles.y,
						(argArgs[4] != "N") ? float.Parse (argArgs[4]) : Actors[0].transform.rotation.eulerAngles.z
					);
					MoveRotSpeed[int.Parse (argArgs[1])] = (argArgs.Length > 4) ? float.Parse (argArgs[4]) : 8f;
				}

			}

			//Look at (ironically)
			if (arguements[i].Contains ("l_") || arguements[i].Contains ("LookAt_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors.Count > int.Parse (argArgs[1]) && Actors.Count > int.Parse (argArgs[2])) {
					//Actors [int.Parse(argArgs[1])].transform.LookAt (Actors [int.Parse(argArgs[2])].transform.position);
					Actors[int.Parse (argArgs[1])].transform.LookAt (MoveList[int.Parse (argArgs[2])]);
				}
			}

			//2D STUFF!!
			//Change MiscImage and adjust to size
			if (arguements[i].Contains ("ShowImage_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				MiscImage.gameObject.SetActive (true);
				Sprite spr = Resources.Load ("Images/" + argArgs[1]) as Sprite;
				if (spr != null) {
					MiscImage.sprite = spr;
					MiscImage.rectTransform.sizeDelta = new Vector2 (float.Parse (argArgs[2]), float.Parse (argArgs[3]));
				} else {
					MiscImage.gameObject.SetActive (false);
				}
			}
			//Change It's Color; ColorImage_RRGGBBAA
			if (arguements[i].Contains ("ColorImage_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				Color color = Color.white;
				if (ColorUtility.TryParseHtmlString ("#" + argArgs[1], out color)) {
					MiscImage.color = color;
				}
			}
			//Hide Image
			if (arguements[i].Contains ("HideImage")) {
				MiscImage.gameObject.SetActive (false);
			}
			//Set up a face actor
			//SetupFace_NAME_IMG_WIDTH_HEIGHT
			if (arguements[i].Contains ("SetupFace_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors2D.Count > 0) { //Don't Get Bamboozled again!
					Sprite sprite = Resources.Load ("Faces/" + ((ActiveChoice.speaker != System.String.Empty) ? ActiveChoice.speaker : Actors2D[int.Parse (argArgs[1])].gameObject.name) + "/" + argArgs[2]) as Sprite;
					if (sprite != null) {
						Actors2D[int.Parse (argArgs[1])].sprite = sprite;
						Actors2D[int.Parse (argArgs[1])].rectTransform.sizeDelta = new Vector2 (float.Parse (argArgs[3]), float.Parse (argArgs[4]));
					}
				}
			}
			//Change A face
			//ChangeFace_ID_IMG
			if (arguements[i].Contains ("ChangeFace_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				if (Actors2D.Count > 0) { //Don't Get Bamboozled again!
					Sprite sprite = Resources.Load ("Faces/" + ((ActiveChoice.speaker != System.String.Empty) ? ActiveChoice.speaker : Actors2D[int.Parse (argArgs[1])].gameObject.name) + "/" + argArgs[2]) as Sprite;
					if (sprite != null) {
						Actors2D[int.Parse (argArgs[1])].sprite = sprite;
					}
				}
			}

			//UNIMPLEMENTED BUT STILl CODED
			//MORELIKEUNTESTED

			//Sounds
			if (arguements[i].Contains ("s_") || arguements[i].Contains ("PlaySound_")) {
				argArgs = arguements[i].Split (new [] { '_' });
				//if()
				Sounds.clip = Resources.Load ("Audio/Sounds/" + argArgs[1]) as AudioClip;
				if (argArgs.Length > 2) { Sounds.volume = float.Parse (argArgs[2]); } else { Sounds.volume = 1; }
				if (argArgs.Length > 3) { Sounds.pitch = float.Parse (argArgs[3]); } else { Sounds.pitch = 1; }
				if (argArgs.Length > 4) { Sounds.panStereo = float.Parse (argArgs[4]); } else { Sounds.panStereo = 0; }
				Sounds.Play ();
			}

			//Voice Acting (Simpler version of sound tbh)
			if (arguements[i].Contains ("v_") || arguements[i].Contains ("PlayVoiceLine_")) {
				string voice = arguements[i].Split (new [] { '_' }) [1];
				Voices.clip = Resources.Load ("Audio/Voices/" + ActiveChoice.speaker + "/" + voice) as AudioClip;
				Sounds.Play ();
			}

			//Songs
			if (arguements[i].Contains ("b_") || arguements[i].Contains ("PlayBGM_")) {
				print ("Attepting to play music");
				string song = arguements[i].Split (new [] { '_' }) [1];
				BGMPlayer.Stop ();
				BGMPlayer.clip = Resources.Load ("Audio/BGM/" + song) as AudioClip;
				if (BGMPlayer.clip == null) {
					BGMPlayer.clip = Resources.Load ("Audio/Music/" + song) as AudioClip;
				}
				BGMPlayer.Play ();
			}

			//STAHP PLAYING
			if (arguements[i].Contains ("bs") || arguements[i].Contains ("StopBGM")) { BGMPlayer.Stop (); }
			if (arguements[i].Contains ("ls") || arguements[i].Contains ("SilenceAllVoice")) { Voices.Stop (); }
			if (arguements[i].Contains ("ss") || arguements[i].Contains ("StopAllSounds")) { Sounds.Stop (); }

			//Panel Effects
			if (arguements[i] == "fadein") { fade = true; SceneIn = true; }
			if (arguements[i] == "fadein") { fade = true; SceneIn = false; }
			if (arguements[i] == "flash") { StartCoroutine (Flash ()); }

		}
	}

}