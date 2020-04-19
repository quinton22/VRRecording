# VR/AR Recording

## Author(s)
Quinton Hoffman

## How to use
1. Load project in Unity
2. If recording (on PC), set <code>isRecording</code> to <code>true</code> on the <code>RecordingController</code> game object. Otherwise, set it to false.
3. Press play, use the arrow keys or w, a, s, d to move around and the mouse to look. If using AR (currently only supported on Android), connect the Android to the computer, make sure USB debugging is enabled on the Android, and click <i>File->Build and Run</i>
4. In AR, find a flat area, adjust the scale of the scene using a slider, and place the scene by pressing the button. Make sure when build <code>isRecording</code> is set to <code>false</code> when building for AR.

## How to modify
- If adding an object that should be recordable, add the <code>RecordedObject</code> prefab and replace the default mesh with your mesh.
- <code>mSceneManager</code> controls which obejcts are in the PC version or AR version. when adding something only for AR, set <code>Set Active</code> to <code>true</code>.
- The script ARTapToPlace.cs contains the code that controls placing objects in the AR app.
- The <code>ARCanvas</code> displays UI and information in the AR application.
