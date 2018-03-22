# CutSceneManager Guide
CutSceneManager is an (Neo)EasyDialogue UI script that was originally made to ease cutscenes similar to that of Persona (Q in particular) by making use of the Dialogue's userData variable, which can be used for various things. In this case, it's used as a command-line-like arguments which specify the action, these include (but are not limited to) moving, rotating and an "Actor", play and stop music, sounds and voice lines, and set Sprite based faces. Note that this project is still Work In Progress, and is open to suggestions.

### The Setup
Setting up is fairly easy but there are things that **MUST** be set, these will be listed with an N (as in not optional).

#### Scene Settings
* You need to build the UI Yourself.
* The Main text box should be a button, which OnClick event should be CutSceneManager.Continue
  * The child Text object should be set to the script's TextBox variable
*Multiple Choice buttons should redirect to CutSceneManager.Answer( an answer number)
  * set the text objects to the Buttons List.
* Once you have your objects set up, add them to the Actors (or Actors2D) list.
* If you're using an online file, specify it, you might wanna change the manager GetFileFromJSON line to suit your needs

#### Inspector Settings
| Scene Details | Optional? | Description |
|---------------|----------|-------------|
| File | Y/N | Offline file, must be set unless you're using a JSON URL |
| FileURL | Y/N | Online link to RAW Json text |
| SceneID | N | Scene in the file (as the file is a database) |
| FadeText | Y | Want it to fade? |
| Voices | Y | AudioSource for Voiced Lines |
| Sounds | Y | Same but for sounds |
| Actors | Y/N | List of Animators (these can be emtpy objects with an Animator Components) |
| Actors2D | Y/N | Same but with Images (uGUI) |
| ReturnScene | Y | Name of the Scene that ExitScene() will call |

| UI Settings | Otpional? | Descritpion |
|-------------|-----------|-------------|
| TextBox | N | Text (uGUI) that'll display the Dialogue |
| ContinueBtn | N | Button  that is dis/enabled when showing dialogue | 
| ~~TextAnim~~ | ?? | Removed? |
| Buttons | Y | List of Text (uGUI) (ironically) that shows the multiple choices |
| Flash Fade| Y | Flash Fade Image (uGUI) |
| ExitEvent | Y | This is accessed once the Dialogue reaches an end |


##### Things to know
* **VERY IMPORTANT** before going to the Command list, most files will be loaded from a folder inside the Resources folder.
  * Music Goes in Resources/Audio/(BGM or Music)/
  * Sounds go in Resources/Audio/Sounds/
  * Voiced Lines go in Resources/Voices/(Character Name)/
  * Faces (Actor2D sprites) go in Resources/Faces/(Character Name)/
  * Images for MiscImage go in Resources/Images/
  * When specifying files, do **NOT** include the extension!
* Actors (non 2D) can be empty gameObjects with an Animator component attached.

#### The Commands
This is where most of the customization takes place, take a look at the ParseArguments function for all the details. These need to be specified in the userData field (lower text field on the original editor)

| Command (3D Actors)| Description |
|--------------------|------------ |
| a_/PlayAnimation_ID_TYPE_NAME| ID is the actor, types 0-sets trigger, 1-sets bool true, 2-sets bool false, 3-disables the object, 4-enables the object|
| m_/Move_ID_X_Y_Z  | Moves an actor to a specified location, if you wish to leave an axis alone, use N as the value |
| r_/Rotate_ID_X_Y_Z | Similar to move. |
| l_/LookAt_TargetID | LookAt but into another Actor |

| Command (Actor2D) | Description |
|-------------------|-------------|
| SetupFace_ID_FACE_W_H | Sets an ACtor2D's sprite, width and height. |
| ChangeFace_ID_FACE | Similar to SetupFace but without width or height. |
| ShowImage_IMG_W_H | Shows an image on MiscImage's image, width and height. |
| HideImage | disabled MiscImage object, to renable use ShowImage*. |
| ColorImage_RRGGBBAA| Changes the color of MiscImage use a hex (but dont include #) to recolor.|

| Command (Audio) | Description |
|-----------------|-------------|
| s_/PlaySound_FILE_VOL_PITCH_PAN | Plays a sound, volume, pitch and pan are optional |
| v_/VoiceLine_FILE | Plays a voice line based on the speaker's (variable) name |
| b_/PlayBGM_FILE | Plays a specified song |
| bs/StopBGM | Stops music. |
| ls/SilenceAllVoice | Stops all voice lines |
| ss/StoPAllSounds | Stosp all sounds |

| Command (General) | Description |
|-------------------|-------------|
| fadein | Fades in. Uses FlashFade |
| fadeout | same as Fadein but inversed |
| flash | Uses FlashFade |
