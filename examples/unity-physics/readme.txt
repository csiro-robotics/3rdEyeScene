This directory contains code which can be integrated into a Unity scene in order to visualise the physics geometry. This code is experimental and is not intended to be an exhaustive and robust visualisation of the Unity physics geometry.

User code may be written on top of this code to visualise AI behaviour and decision making.

The steps below show how to integrate this code into an existing Unity scene by using the Unity Angry Bots demo as an example. Note that Angry Bots is not 100% compatible with Unity 5, but behaves sufficiently to serve this example.

Note: the code for this example appears under "Standard assets". This is to ensure Tes.TesServer is accessible from Javascript code as well as C#. The source may be moved from "Standard assets" if Javascript support is not required.

1. Build the 3esCore project and 3rd Eye Scene viewer application (see 3es documentation).
2. Download and open the Angry Bots demo from: https://www.assetstore.unity3d.com/en/#!/content/12175
3. Import the Angry Bots project into a Unity project.
4. Confirm conversion to Unity 5.
5. Copy the contents of the contents of this Assets folder into the Angry Bots Assets folder.
4. Create the folder "Assets/Standard assets/Tes/plugins" in the Angry Bots project.
5. Copy 3esCore.dll into "Assets/Standard assets/Tes/plugins"
6. Open the AngryBots scene in the Unity Editor.
7. Under the scene root, create an empty object named "3rd Eye Scene".
8. Add a component to the "3rd Eye Scene" object selecting "Scripts->Tes Server"
  - Alternatively, browse the project to "Standard assets/Tes/TesServer" and drag this onto the "3rd Eye Scene" object.
9. Select "3rd Eye Scene" in the scene hierarchy.
10. In the Unity Inspector, select "Add Views".
  - This adds the TesPhysicsView component to all objects which have a physics component.
11. Click Play
12. Run the 3rd Eye Scene Viewer
13. Click the Connect Panel button in the top left corner.
14. Enter the following host and port number and click "Connect":
  Host: 127.0.0.1
  Port: 33500
15. Make sure the Unity Angry Bots window is in focus.

At this point the 3rd Eye Scene viewer should show the physics geometry. You can click record to start recording the session. Once done, click disconnect on the Connect Panel, then click the play button on the play bar and select the recorded file.

Here are some other things you can try:
+ Press '1' in the 3rd Eye Scene viewer to view the scene from the perspective of the player camera.

+ Modify the Angry Bots AI scripts to show the AI state.
- Open SpiderAttackMoveController.js
- Add the following line to the top of the Awake() function:
  TesServer.Instance.Text2DWorld(0, "Awake", transform.position);
- Add the following to the top of Update() after the noticeTime if statement:
  TesServer.Instance.Text2DWorld(0, "Tracking", transform.position);
- Add the following to the top of Explode()
  TesServer.Instance.Text2DWorld(0, "Boom!", transform.position);
- Open SpiderReturnMoveController.js
- Add the following to the top of Update()
  TesServer.Instance.Text2DWorld(0, "Return", transform.position);
- Record playing the demo and note the AI state descriptions.

+ Expose additional AI functionality
- Try these:
  - Show other AI states and transitions
  - Highlight patrol routes
  - Expose health values

+ Remove physics visualisation
- Select the "3rd Eye Scene" object and click "Remove Views" to remove the physics visualisation.
