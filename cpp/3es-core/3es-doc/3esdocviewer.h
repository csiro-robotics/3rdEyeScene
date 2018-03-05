/*!
@page docviewer 3rd Eye Scene Viewer Application
The 3<sup>rd</sup> Eye Scene Viewer application supports remote visualisation of 3es server commands. This section describes the basic UI and usage of the client application.

@section secmainui
The viewer UI is a minimalist UI consisting of a 3D scene view, a playback bar and various panel buttons. The 3D scene view renders the current state of the currently connected server, or the current frame of a previously recorded session. The playback bar supports recording, playback, scrubbing and stepping frames. The panel buttons access additional UI components used to establish a connection or control viewer settings.

@image html images/ui/mainui.png "Main UI"

@section secpanelbuttons Panel Buttons
The panel buttons select the active UI panel. The current panels are listed below.

Button                                  | Panel             | Purpose
--------------------------------------- | ----------------- | -------------------------------
@image html connect.png "Connect"       | Connection Panel  | Manage the current connection
@image html categories.png "Categories" | Categories Panel  | Toggle active display categories
@image html settings.png "Settings"     | Settings Panel    | Access viewer settings

@subsection subconnection Connection Panel
The connection panel is used to establish a new connection, or to disconnect the current connection.

@image html images/ui/connectionpanel.png 

To establish a new connection;
-# Open the connection panel.
-# Enter the target host address and port.
-# Click "Connect"

The connection is made immediately if the server is already running. When the server is not yet running, use "Auto Reconnect" to keep trying to establish a connection, or reconnect so long as the viewer is running. Once connected, the "Connect" button changes to "Disconnect" and can be used to disconnect. The connection panel button also changes to reflect the status change.

Previous connections are listed in the Connection History. Each entry acts as a button. Simply click on the desired history item to attempt that connection again.

@subsection subcategories Categories Panel
The categories panel identifies all the object categories published by the server. Each category can be enabled or disabled by clicking the check box. Disabled categories do not appear in the 3D view. The categories are entirely server dependent.

@image html images/ui/categoriespanel.png 

@subsection subsettings Settings Panel
The settings panel is used to edit the viewer settings. The panel itself shows the settings categories. Click on each item to edit settings relevant to that category. Settings are preserved between viewer sessions.

@image html images/ui/settingspanel.png 


@section secplayback Playback Bar
The playback bar is mostly used during playback of a previously recorded session. It is also used to initiate recording of the current session.

@image html images/ui/playbackbar.png

Button                                      | Purpose 
------------------------------------------- | --------------------------------------------------------------
@image html record.png "Record"             | Start recording of the current session. Changes to "Stop".
@image html stop.png "Stop"                 | Stop current recording or playback.
@image html play.png "Play"                 | Start/resume playback of a recorded file. Changes to "Pause".
@image html pause.png "Pause"               | Pause playback. Enabled frame stepping.
@image html skip-backward.png  "Skip Start" | Skip to the start of playback.
@image html seek-backward.png "Step Back"   | Step back one frame (while paused)
@image html seek-forward.png "Step Forward" | Step forwards one frame (while paused)  
@image html skip-forward.png "Skip End"     | Skip to the end of playback.

To start recording, click the record button. This opens the file dialog. From here select the file to record to. Recording begins immediately if already connected, or as soon as a connection is established.

To playback a previously recorded file, disconnect if connected, then press play. This opens the file dialog. Browse to the recorded file and select open. Playback begins paused.

During playback, the step and skip buttons can be used to step single frames, or to either end of the recording. These buttons only function while paused. The timeline can also be used to scrub to a desired frame. Finally, the current frame number can be edited to select a desired frame number.

*/
