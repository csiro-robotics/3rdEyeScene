# 3rd Eye Scene

3rd Eye Scene is a visual debugger and debugging aid in the vein of [rviz](http://wiki.ros.org/rviz) or physics engine viewers such as [Havok Visual Debugger](https://www.havok.com/physics/) or [PhysX Visual Debugger](https://developer.nvidia.com/physx-visual-debugger). Whereas those tools are tightly bound to their respective SDKs, 3rd Eye Scene is a generalised tool and can be used to remotely visualise and debug any real time or non real time 3D algorithm. Conceptually, it can be thought of as a remote rendering application. A 3es server may be embedded into any program, then 3es render commands are used to instrument the target program. The 3es viewer client application is then used to view, record and playback these commands.

## Key Features

- Remote 3D rendering from any application
- Record rendered data
- Playback and step through recorded data
- Open, extensible protocol
- Plugin extensible to visualise specialised geometry

## Use Cases

- Visualising geometric algorithms
  - Mesh operations
  - Geometric intersection tests
  - Point cloud processing
- Remote visualisation
  - Visualise 3D data from headless processes
- Real time visualisation
  - Remote visualisation
  - Visualise "hidden" data
    - Physics geometry
    - AI logic and constructs
- QA testing
  - Record test sessions and attach 3es files to bug reports.

## Integrating 3rd Eye Scene Server Code
The 3rd Eye Scene core includes code for both a C++ and a C# based server. This section focuses on integrating the C++ code to debug an application. The example presented here is the 3rd-occupancy example included in the TES source code release. The 3es macro interface is presented later.

Before sending TES messages, a `tes::Server` object must be declared and initialised as shown below.

```
tes::Server *g_tesServer = nullptr;  // Global declaration.

void initialiseTes()
{
  // Configure settings: compression and collation on (required by compression)
  tes::ServerSettings settings(tes::SF_Compress | tes::SF_Collate);
  // Setup server info to the client.
  tes::ServerInfoMessage serverInfo;
  tes::initDefaultServerInfo(&serverInfo);
  // Coordinate axes listed as left/right, forward, up
  serverInfo.coordinateFrame = tes::XYZ;

  // Create the server.
  g_tesServer = tes::Server::create(settings, &serverInfo);
  // Setup asynchronous connection monitoring.
  // Connections must be committed synchronously.
  g_tesServer->connectionMonitor()->start(tes::ConnectionMonitor::Asynchronous);

  // Optional: wait 1000ms for the first connection before continuing.
  if (g_tesServer->connectionMonitor()->waitForConnection(1000) > 0)
  {
    g_tesServer->connectionMonitor()->commitConnections();
  }
}
```

Several key server methods must be called periodically to manage the connection. These calls are listed and explained below.

```
void endFrame(float dt = 0.0f)
{
  // Mark the end of frame. Flushed collated packets.
  g_tesServer->updateFrame(dt);
  // In synchronous mode, listen for incoming connections.
  if (g_tesServer->connectionMonitor()->mode() == tes::ConnectionMonitor::Synchronous)
  {
    g_tesServer->connectionMonitor()->monitorConnections();
  }
  // Activate any newly accepted connections and expire old ones.
  g_tesServer->connectionMonitor()->commitConnections();
  // Update any bulk resource data transfer.
  g_tesServer->updateTransfers(0);
}
```

Once the server has been created and initialised it becomes possible to invoke object creation and update commands. The code below shows the creation and animation of a box shape as well as the creation of some transient objects.

```
void animateBox(tes::Server &server)
{
  // Declare a box.
  tes::Box box(
    1,  // ID
    tes::Vector3f(0, 0, 0), // Position
    tes::Vector3f(0.2f, 0.1f, 0.5f)); // Dimensions

  box.setColour(tes::Colour(0, 0, 255));

  // Create the box on the client.
  server.create(box);

  const int steps = 90;
  for (int i = 0; i <= steps; ++i)
  {
    // Update the box.
    box.setPosZ(std::sin(tes::degToRad(i / float(steps) * float(M_PI))));
    server.update(box);
    endFrame(1.0f / float(steps));
  }

  // Destroy the box.
  server.destroy(box);
  endFrame(0.0f);
}
```

To correct dispose of the server, call `dispose()`.

```
void releaseTes()
{
  if (g_tesServer)
  {
    // Close connections.
    g_tesServer->close();
    // Destroy the server.
    g_tesServer->dispose();
    g_tesServer = nullptr;
  }
}
```

## Using Categories

Categories may be used to logically group objects in the viewer client. Objects from specific categories can be hidden and shown as a group. Categories are form a hierarchy, with each category having an optional parent. The code below shows an example category initialisation.

```
void defineCategory(tes::Server &server, const char *name, uint16_t category, uint16_t parent, bool defaultActive)
{
  tes::CategoryNameMessage msg;
  msg.categoryId = category;
  msg.parentId = parent;
  const size_t nameLen = (name) ? strlen(name) : 0;
  msg.name = name;
  tes::sendMessage(server, tes::MtCategory, tes::CategoryNameMessage::MessageId, msg);
}

void initCategories()
{
  // CAT_Xxx are members of an enum. This builds the following tree:
  // - Map
  // - Populate
  //   - Rays
  //   - Free
  //   - Occupied
  // - Info
  defineCategory(*g_tesServer, "Map", CAT_Map, 0, true);
  defineCategory(*g_tesServer, "Populate", CAT_Populate, 0, true);
  defineCategory(*g_tesServer, "Rays", CAT_Rays, CAT_Populate, true);
  defineCategory(*g_tesServer, "Free", CAT_FreeCells, CAT_Populate, false);
  defineCategory(*g_tesServer, "Occupied", CAT_OccupiedCells, CAT_Populate, true);
  defineCategory(*g_tesServer, "Info", CAT_Info, 0, true);
}
```

## Using the Macro Interface

It is also possible to use preprocessor macros to invoke must 3rd Eye Scene API calls. This is to support removing all debugging code via the preprocessor thereby eliminating all associated overhead. The examples above can be rewritten using the macro interface as shown below. The `animateBox2()` function is equivalent to the `animateBox()` function, but uses transient objects instead of updating a single object.

```
// Declare global server pointer.
TES_SERVER_DECL(g_tesServer);

void initialiseTes()
{
  // Initialise TES
  TES_SETTINGS(settings, tes::SF_Compress | tes::SF_Collate);
  // Initialise server info.
  TES_SERVER_INFO(info, tes::XYZ);
  // Create the server. Use tesServer declared globally above.
  TES_SERVER_CREATE(g_tesServer, settings, &info);

  // Start the server and wait for the connection monitor to start.
  TES_SERVER_START(g_tesServer, tes::ConnectionMonitor::Asynchronous);
  TES_SERVER_START_WAIT(g_tesServer, 1000);
}

void initCategories()
{
  // CAT_Xxx are members of an enum. This builds the following tree:
  // - Map
  // - Populate
  //   - Rays
  //   - Free
  //   - Occupied
  // - Info
  TES_CATEGORY(*g_tesServer, "Map", CAT_Map, 0, true);
  TES_CATEGORY(*g_tesServer, "Populate", CAT_Populate, 0, true);
  TES_CATEGORY(*g_tesServer, "Rays", CAT_Rays, CAT_Populate, true);
  TES_CATEGORY(*g_tesServer, "Free", CAT_FreeCells, CAT_Populate, false);
  TES_CATEGORY(*g_tesServer, "Occupied", CAT_OccupiedCells, CAT_Populate, true);
  TES_CATEGORY(*g_tesServer, "Info", CAT_Info, 0, true);
}

void animateBox(tes::Server &server)
{
  // Create a box on the client.
  TES_BOX(server, TES_COLOUR(Blue),
          1,  // ID
          tes::Vector3(0.0f), // Position
          tes::Vector3f(0.2f, 0.1f, 0.5f)); // Dimensions

  for (int i = 0; i <= steps; ++i)
  {
    // Update the box.
    TES_POS_UPDATE(server, Box, 1, tes::Vector3f(0, 0, std::sin(tes::deg2Rad(i / float(steps) * float(M_PI))));
    TES_SERVER_UPDATE(1.0f / float(steps));
  }

  // Destroy the box.
  TES_BOX_END(server, 1);
  TES_SERVER_UPDATE(0.0f);
}

void animateBox2(tes::Server &server)
{
  for (int i = 0; i <= steps; ++i)
  {
    // Create a transient box on each iteration.
    TES_BOX(server, TES_COLOUR(Blue),
            0,  // Transient ID
            tes::Vector3(0.0f, 0.0f,
                std::sin(tes::deg2Rad(i / float(steps) * float(M_PI)))), // Position
            tes::Vector3f(0.2f, 0.1f, 0.5f)); // Dimensions
    TES_SERVER_UPDATE(1.0f / float(steps));
  }

  TES_SERVER_UPDATE(0.0f);
}

void releaseTes()
{
  TES_SERVER_STOP(g_tesServer);
}
```

Additional documentation can be found at [csiro-robotics.github.io/3rdEyeScene/](https://csiro-robotics.github.io/3rdEyeScene/)
