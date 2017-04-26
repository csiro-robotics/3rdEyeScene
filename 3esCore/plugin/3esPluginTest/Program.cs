using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tes.Net;
using Tes.Maths;
using Tes.Server;

namespace Tes
{
  class Program
  {
    class Ids
    {
      public uint NextShapeId = 1;
      public uint NextMeshId = 1;
    }

    static bool Quit { get; set; }

    static bool HaveOption(string option, string[] args)
    {
      for (int i = 0; i < args.Length; ++i)
      {
        if (option.CompareTo(args[i]) == 0)
        {
          return true;
        }
      }

      return false;
    }

    static void ShowUsage()
    {
      //Console.WriteLine("Usage:");
      //Console.WriteLine(string.Format("<program> [options] [shapes]"));
    }

    static void CreateAxes(Ids ids, List<Shapes.Shape> shapes, List<Resource> resources, string[] args)
    {
      if (HaveOption("noaxes", args))
      {
        return;
      }

      const float arrowLength = 1.0f;
      const float arrowRadius = 0.025f;
      Vector3 pos = Vector3.Zero;
      Shapes.Arrow arrow;

      arrow = new Shapes.Arrow(ids.NextShapeId++, pos, new Vector3(1, 0, 0), arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.Red].Value;
      shapes.Add(arrow);

      arrow = new Shapes.Arrow(ids.NextShapeId++, pos, new Vector3(0, 1, 0), arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.ForestGreen].Value;
      shapes.Add(arrow);

      arrow = new Shapes.Arrow(ids.NextShapeId++, pos, new Vector3(0, 0, 1), arrowLength, arrowRadius);
      arrow.Colour = Colour.Colours[(int)PredefinedColour.DodgerBlue].Value;
      shapes.Add(arrow);

    }

    static void CreateShapes(Ids ids, List<Shapes.Shape> shapes, List<Resource> resources, string[] args)
    {
      int initialShapeCount = shapes.Count;

      Pyramid pyramid = new Pyramid(ids.NextShapeId++, new Vector3(-2, 0, 0), new Vector3(1, 1, 1), Quaternion.Identity);
      shapes.Add(pyramid);
    }

    static void Main(string[] args)
    {
      if (HaveOption("help", args))
      {
        ShowUsage();
        return;
      }

      ServerSettings serverSettings = ServerSettings.Default;
      ServerInfoMessage info = ServerInfoMessage.Default;
      info.CoordinateFrame = CoordinateFrame.XYZ;
      if (HaveOption("compress", args))
      {
        serverSettings.Flags |= ServerFlag.Compress;
      }
      serverSettings.Flags &= ~ServerFlag.Collate;

      IServer server = new TcpServer(serverSettings);
      List<Shapes.Shape> shapes = new List<Shapes.Shape>();
      List<Resource> resources = new List<Resource>();
      Ids ids = new Ids();
      ids.NextShapeId = 1;

      CreateAxes(ids, shapes, resources, args);
      CreateShapes(ids, shapes, resources, args);

      const int targetFrameTimeMs = 1000 / 30;
      int updateElapsedMs = 0;
      float dt = 0;
      float time = 0;

      Stopwatch frameTimer = new Stopwatch();
      Stopwatch sleepTimer = new Stopwatch();

      // Register shapes with server.
      foreach (Shapes.Shape shape in shapes)
      {
        server.Create(shape);
      }

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

        server.UpdateFrame(dt);

        if (server.ConnectionMonitor.Mode == ConnectionMonitorMode.Synchronous)
        {
          server.ConnectionMonitor.MonitorConnections();
        }
        server.ConnectionMonitor.CommitConnections((IServer s, IConnection connection) =>
        {
          foreach (Shapes.Shape shape in shapes)
          {
            connection.Create(shape);
          }
        });

        server.UpdateTransfers(64 * 1024);

        Console.Write(string.Format("\rFrame {0}: {1} connection(s)      ", dt, server.ConnectionCount));
        // Console Flush?

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

      foreach (Shapes.Shape shape in shapes)
      {
        server.Destroy(shape);
      }

      server.UpdateFrame(0);

      server.ConnectionMonitor.Stop();
      server.ConnectionMonitor.Join();
    }
  }
}
