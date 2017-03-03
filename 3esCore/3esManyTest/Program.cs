using System;
using System.Diagnostics;
using Tes.Net;
using Tes.Maths;
using Tes.Server;
using Tes.Shapes;

namespace Tes
{
  /// <summary>
  /// Test creating many objects for visualisation.
  /// </summary>
  class Program
  {
    static bool Quit { get; set; }

    private static void CreateAxes(IConnection connection)
    {
      const float arrowLength = 1.0f;
      const float arrowRadius = 0.025f;
      Arrow arrow = new Arrow(1, Vector3.Zero, Vector3.AxisX, arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.FireBrick].Value;
      connection.Create(arrow);

      arrow = new Arrow(2, Vector3.Zero, Vector3.AxisY, arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.SeaGreen].Value;
      connection.Create(arrow);

      arrow = new Arrow(3, Vector3.Zero, Vector3.AxisZ, arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.SkyBlue].Value;
      connection.Create(arrow);
    }


    private static void OnNewConnection(IServer server, IConnection connection)
    {
      CreateAxes(connection);
    }

    static void ManyBoxes(IServer server, int gridX, int gridY, int gridZ, Vector3 offset, Vector3 boxSize)
    {
      int startX = gridX / -2;
      int endX = startX + gridX;
      int startY = gridY / -2;
      int endY = startY + gridY;
      int startZ = gridZ / -2;
      int endZ = startZ + gridZ;

      Vector3 boxPos = new Vector3();
      Box box = new Box(0, Vector3.Zero, boxSize, Quaternion.Identity);

      box.Colour = Colour.Colours[(int)PredefinedColour.LightSlateGrey].Value;
      box.Wireframe = true;

      for (int z = startZ; z <= endZ; ++z)
      {
        boxPos.Z = offset.Z + z * boxSize.Z;
        for (int y = startY; y <= endY; ++y)
        {
          boxPos.Y = offset.Y + y * boxSize.Y;
          for (int x = startX; x <= endX; ++x)
          {
            boxPos.X = offset.X + x * boxSize.X;
            box.Position = boxPos;
            server.Create(box);
          }
        }
      }
    }


    static void Main(string[] args)
    {
      //if (HaveOption("help", args))
      //{
      //  ShowUsage();
      //  return;
      //}

      ServerSettings serverSettings = ServerSettings.Default;
      ServerInfoMessage info = ServerInfoMessage.Default;
      info.CoordinateFrame = CoordinateFrame.XYZ;
      serverSettings.Flags |= ServerFlag.Collate;
      serverSettings.Flags |= ServerFlag.Compress;

      IServer server = new TcpServer(serverSettings);

      const int targetFrameTimeMs = 1000 / 5;
      int updateElapsedMs = 0;
      Vector3 offset = Vector3.Zero;
      float dt = 0;
      float time = 0;

      Stopwatch frameTimer = new Stopwatch();
      Stopwatch sleepTimer = new Stopwatch();

      server.ConnectionMonitor.Start(ConnectionMonitorMode.Asynchronous);

      frameTimer.Start();
      while (!Quit)
      {
        frameTimer.Stop();
        dt = frameTimer.ElapsedMilliseconds * 1e-3f;
        time += dt;
        frameTimer.Reset();
        frameTimer.Start();
        sleepTimer.Reset();
        sleepTimer.Start();

        if (server.ConnectionMonitor.Mode == ConnectionMonitorMode.Synchronous)
        {
          server.ConnectionMonitor.MonitorConnections();
        }
        server.ConnectionMonitor.CommitConnections(OnNewConnection);

        server.UpdateTransfers(0);
        server.UpdateFrame(dt);

        offset.X += 0.05f;
        if (offset.X >= 1.0f)
        {
          offset.X = 0.0f;
        }
        ManyBoxes(server, 50, 50, 50, offset, new Vector3(0.1f, 0.1f, 0.1f));

        Console.Write(string.Format("\rFrame {0}: {1} connection(s)      ", dt, server.ConnectionCount));

        // Work out how long to sleep for.
        sleepTimer.Stop();
        updateElapsedMs = (int)sleepTimer.ElapsedMilliseconds;
        if (updateElapsedMs + 1 < targetFrameTimeMs)
        {
          // Sleep until the next frame. Sleep for a millisecond less than we need to allow
          // a bit of overhead.
          System.Threading.Thread.Sleep(targetFrameTimeMs - (updateElapsedMs + 1));
        }
      }

      server.UpdateFrame(0);

      server.ConnectionMonitor.Stop();
      server.ConnectionMonitor.Join();
    }
  }
}
