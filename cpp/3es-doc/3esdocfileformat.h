
/*!
@page docfileformat File Format
3rd Eye Scene files are client recordings from a 3<sup>rd</sup> Eye Scene server connection. 3rd Eye Scene files normally have a '.3es' extension. The simplest such file is simply a direct serialisation of the incoming message packets, as documented in @ref docprotocol . The incoming messages are serialised as is; packet header, payload and CRC. However, a well formed 3es file should follow the guidelines described below and may optionally compress the file.

@section secfilelayload Layout
A well formed 3es file is always begins with a @ref secserverinfomsg "server info" message. This identifies the server characteristics and coordinate frame for correct playback. This message is immediately followed by a @em frame @em count @ref seccontrolmsg "control message". This identifies the total number of recorded frames in the file. Both these messages must always appear in the file uncompressed.

Item                      | Description
------------------------- | ---------------------------------------------------
Server Info               | A @ref secserverinfomsg "server info" message detailing the server setting.
Frame Count               | Optional @ref seccontrolmsg "control message" setting the number of frames in the recording.
[Compression Begins]      | Optional GZIP compression begins here. Preceding data are uncompressed.  
World State Serialisation | Optional (recommended) serialisation of the world state at the start of recording.
Message Serialisation     | Recorded messages.

Data following the server info and frame count control message may appear either compressed or uncompressed. The two leading message are then followed by a serialisation of the client world state at the start of recording. That is, on beginning recording, the viewer will serialise create and data messages required to instantiate the objects already present on the viewing client. This ensures the correct initial state for subsequent messages.

The remainder of the file is simply a serialisation of the incoming server message packets with optional file level compression.

@subsection secfileoptions
The following components of a 3es recording are optional, though some are highly recommended and should generally be expected to be present:
- A frame count @ref seccontrolmsg "control message" following the server info message (highly recommended).
- World state serialisation as described above (highly recommended).
- @ref secfilecompression "Compression"
- @ref seccameramsg "Camera messages" representing the viewing application's camera state.

Note the world state serialisation is optional, but highly recommended. While 3<sup>3r</sup> Eye Scene viewer does serialise the world state, some recording applications may be unable to recognise the world state. For instance, the core release of 3<sup>3r</sup> Eye Scene also features a command line recording application. This application is a dumb application which does nothing to interpret the incoming messages. As such it does not know the world state and cannot serialise one. Such an application is intended to connect as the server application starts and record all incoming packets, thus obviating the need for a world state serialisation.

Camera messages are also optional, primarily because not all recording applications have a client camera representation (such as the command line recording application). 



@subsection secfilecompression Compression
3es files use GZIP compression. The recommended supporting libraries for GZIP compression are <a href="http://www.zlib.net/">zlib</a> in GZIP mode, <a href="https://msdn.microsoft.com/en-us/library/system.io.compression.gzipstream(v=vs.110).aspx">GZipStream</a> from the .Net core libraries, or the GZipStream port to Unity provided as part of the Tes.Compression namespace, thanks to <a href="http://www.hitcents.com/">Hitcents</a>.

GZIP compression begins immeidately after the expected @ref secserverinfomsg "server info" and generally expected frame count messages.

NOTE: The GZIP compression scheme is likely to change soon to break up the file into smaller compressed sections. This is to support jumping to key frames in the file. The net result is that the GZIP stream should be terminated after every N bytes (to be decided) and an uncompressed maker packet inserted into the stream before resuming a new GZIP compression stream. The viewer can then serialise the world state at every marker packet as a key frame. Step back, which currently begins playback from the start of the file, can then jump to a specific keyframe, restore the serialised state for that keyframe, and resume streaming. Note that the viewing application manages the key frame serialisation; there are no keyframes in the 3es file itself.

*/