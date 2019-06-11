using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tes.IO;
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
      Console.WriteLine("Usage:");
      Console.WriteLine(string.Format("<program> [options] [shapes]"));
      Console.WriteLine("\nValid options:");
      Console.WriteLine("  help: show this message");
      Console.WriteLine("  compress: write collated and compressed packets");
      Console.WriteLine("  noaxes: Don't create axis arrow objects");
      Console.WriteLine("  nomove: don't move objects (keep stationary)");
      Console.WriteLine("  wire: Show wireframe shapes, not slide for relevant objects");
      Console.WriteLine("\nValid shapes:");
      Console.WriteLine("\tall: show all shapes");
      Console.WriteLine("\tarrow");
      Console.WriteLine("\tbox");
      Console.WriteLine("\tcapsule");
      Console.WriteLine("\tcloud");
      Console.WriteLine("\tcloudpart");
      Console.WriteLine("\tcone");
      Console.WriteLine("\tcylinder");
      Console.WriteLine("\tlines");
      Console.WriteLine("\tmesh");
      Console.WriteLine("\tplane");
      Console.WriteLine("\tpoints");
      Console.WriteLine("\tsphere");
      Console.WriteLine("\tstar");
      Console.WriteLine("\ttext2d");
      Console.WriteLine("\ttext3d");
      Console.WriteLine("\ttriangles");
    }

    static Shapes.MeshResource CreateTestMesh(Ids ids)
    {
      Shapes.SimpleMesh mesh = new Shapes.SimpleMesh(ids.NextMeshId++, MeshDrawType.Triangles, Shapes.MeshComponentFlag.Vertex | Shapes.MeshComponentFlag.Index | Shapes.MeshComponentFlag.Colour);

      mesh.AddVertex(new Vector3(-0.5f, 0, -0.5f));
      mesh.AddVertex(new Vector3(0.5f, 0, -0.5f));
      mesh.AddVertex(new Vector3(0.5f, 0, 0.5f));
      mesh.AddVertex(new Vector3(-0.5f, 0, 0.5f));

      mesh.AddIndex(0);
      mesh.AddIndex(1);
      mesh.AddIndex(2);
      mesh.AddIndex(0);
      mesh.AddIndex(2);
      mesh.AddIndex(3);

      mesh.AddColour(0xff0000ff);
      mesh.AddColour(0xffff00ff);
      mesh.AddColour(0xff00ffff);
      mesh.AddColour(0xffffffff);

      return mesh;
    }

    static Shapes.MeshResource CreateTestCloud(Ids ids)
    {
      Shapes.PointCloud cloud = new Shapes.PointCloud(ids.NextMeshId++, 8);  // Considered a Mesh for ID purposes.

      cloud.AddPoint(new Vector3(0, 0, 0));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);
      cloud.AddPoint(new Vector3(1, 0, 0));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);
      cloud.AddPoint(new Vector3(0, 1, 0));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(255, 255, 255).Value);
      cloud.AddPoint(new Vector3(0, 0, 1));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);
      cloud.AddPoint(new Vector3(1, 1, 0));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 0, 0).Value);
      cloud.AddPoint(new Vector3(0, 1, 1));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);
      cloud.AddPoint(new Vector3(1, 0, 1));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);
      cloud.AddPoint(new Vector3(1, 1, 1));
      cloud.AddNormal(new Vector3(0, 0, 1));
      cloud.AddColour(new Colour(0, 255, 255).Value);

      return cloud;
    }

    static void CreateAxes(Ids ids, List<Shapes.Shape> shapes, List<ShapeMover> movers, List<Resource> resources, string[] args)
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

    static void CreateShapes(Ids ids, List<Shapes.Shape> shapes, List<ShapeMover> movers, List<Resource> resources, string[] args)
    {
      bool allShapes = HaveOption("all", args);
      bool noMove = HaveOption("nomove", args);
      int initialShapeCount = shapes.Count;

      if (allShapes || HaveOption("arrow", args))
      {
        Shapes.Arrow arrow = new Shapes.Arrow(ids.NextShapeId++);
        arrow.Radius = 0.5f;
        arrow.Length = 1.0f;
        arrow.Colour = Colour.Colours[(int)PredefinedColour.SeaGreen].Value;
        shapes.Add(arrow);
        if (!noMove)
        {
          movers.Add(new Oscilator(arrow, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("box", args))
      {
        Shapes.Box box = new Shapes.Box(ids.NextShapeId++);
        box.Scale = new Vector3(0.45f);
        box.Colour = Colour.Colours[(int)PredefinedColour.MediumSlateBlue].Value;
        shapes.Add(box);
        if (!noMove)
        {
          movers.Add(new Oscilator(box, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("capsule", args))
      {
        Shapes.Capsule capsule = new Shapes.Capsule(ids.NextShapeId++);
        capsule.Length = 2.0f;
        capsule.Radius = 0.3f;
        capsule.Colour = Colour.Colours[(int)PredefinedColour.LavenderBlush].Value;
        shapes.Add(capsule);
        if (!noMove)
        {
          movers.Add(new Oscilator(capsule, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("cone", args))
      {
        Shapes.Cone cone = new Shapes.Cone(ids.NextShapeId++);
        cone.Length = 2.0f;
        cone.Angle = 15.0f / 180.0f * (float)Math.PI;
        cone.Colour = Colour.Colours[(int)PredefinedColour.SandyBrown].Value;
        shapes.Add(cone);
        if (!noMove)
        {
          movers.Add(new Oscilator(cone, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("cylinder", args))
      {
        Shapes.Cylinder cylinder = new Shapes.Cylinder(ids.NextShapeId++);
        cylinder.Scale = new Vector3(0.45f);
        cylinder.Colour = Colour.Colours[(int)PredefinedColour.FireBrick].Value;
        shapes.Add(cylinder);
        if (!noMove)
        {
          movers.Add(new Oscilator(cylinder, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("plane", args))
      {
        Shapes.Plane plane = new Shapes.Plane(ids.NextShapeId++);
        plane.Normal = new Vector3(1.0f, 1.0f, 0.0f).Normalised;
        plane.Scale = 1.5f;
        plane.NormalLength = 0.5f;
        plane.Colour = Colour.Colours[(int)PredefinedColour.LightSlateGrey].Value;
        shapes.Add(plane);
        if (!noMove)
        {
          movers.Add(new Oscilator(plane, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("sphere", args))
      {
        Shapes.Sphere sphere = new Shapes.Sphere(ids.NextShapeId++);
        sphere.Radius = 0.75f;
        sphere.Colour = Colour.Colours[(int)PredefinedColour.Coral].Value;
        shapes.Add(sphere);
        if (!noMove)
        {
          movers.Add(new Oscilator(sphere, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("star", args))
      {
        Shapes.Star star = new Shapes.Star(ids.NextShapeId++);
        star.Radius = 0.75f;
        star.Colour = Colour.Colours[(int)PredefinedColour.DarkGreen].Value;
        shapes.Add(star);
        if (!noMove)
        {
          movers.Add(new Oscilator(star, 2.0f, 2.5f));
        }
      }

      if (allShapes || HaveOption("lines", args))
      {
        Vector3[] lineSet = new Vector3[]
        {
          new Vector3(0, 0, 0), new Vector3(0, 0, 1),
          new Vector3(0, 0, 1), new Vector3(0.25f, 0, 0.8f),
          new Vector3(0, 0, 1), new Vector3(-0.25f, 0, 0.8f)
        };
        Shapes.MeshShape lines = new Shapes.MeshShape(MeshDrawType.Lines, lineSet, ids.NextShapeId++);
        shapes.Add(lines);
        // if (!noMove)
        // {
        //   movers.Add(new Oscilator(mesh, 2.0f, 2.5f));
        // }
      }

      if (allShapes || HaveOption("triangles", args))
      {
        Vector3[] triangleSet = new Vector3[]
        {
          new Vector3(0, 0, 0), new Vector3(0, 0.25f, 1), new Vector3(0.25f, 0, 1),
          new Vector3(0, 0, 0), new Vector3(-0.25f, 0, 1), new Vector3(0, 0.25f, 1),
          new Vector3(0, 0, 0), new Vector3(0, -0.25f, 1), new Vector3(-0.25f, 0, 1),
          new Vector3(0, 0, 0), new Vector3(0.25f, 0, 1), new Vector3(0, -0.25f, 1)
        };
        UInt32[] colours = new UInt32[]
        {
          Colour.Colours[(int)PredefinedColour.Red].Value, Colour.Colours[(int)PredefinedColour.Red].Value, Colour.Colours[(int)PredefinedColour.Red].Value,
          Colour.Colours[(int)PredefinedColour.Green].Value, Colour.Colours[(int)PredefinedColour.Green].Value, Colour.Colours[(int)PredefinedColour.Green].Value,
          Colour.Colours[(int)PredefinedColour.Blue].Value, Colour.Colours[(int)PredefinedColour.Blue].Value, Colour.Colours[(int)PredefinedColour.Blue].Value,
          Colour.Colours[(int)PredefinedColour.White].Value, Colour.Colours[(int)PredefinedColour.White].Value, Colour.Colours[(int)PredefinedColour.White].Value,
        };
        Shapes.MeshShape triangles = new Shapes.MeshShape(MeshDrawType.Triangles, triangleSet, ids.NextShapeId++);
        triangles.Colours = colours;
        shapes.Add(triangles);
        // if (!noMove)
        // {
        //   movers.Add(new Oscilator(mesh, 2.0f, 2.5f));
        // }
      }

      if (allShapes || HaveOption("mesh", args))
      {
        Shapes.MeshResource mesRes = CreateTestMesh(ids);
        resources.Add(mesRes);
        Shapes.MeshSet mesh = new Shapes.MeshSet(ids.NextShapeId++);
        mesh.AddPart(mesRes);
        shapes.Add(mesh);
        // if (!noMove)
        // {
        //   movers.Add(new Oscilator(mesh, 2.0f, 2.5f));
        // }
      }

      if (allShapes || HaveOption("points", args))
      {
        Vector3[] pts = new Vector3[]
        {
          new Vector3(0, 0, 0),
          new Vector3(0, 0.25f, 1),
          new Vector3(0.25f, 0, 1),
          new Vector3(-0.25f, 0, 1),
          new Vector3(0, -0.25f, 1)
        };
        UInt32[] colours = new UInt32[]
        {
          Colour.Colours[(int)PredefinedColour.Black].Value,
          Colour.Colours[(int)PredefinedColour.Red].Value,
          Colour.Colours[(int)PredefinedColour.Green].Value,
          Colour.Colours[(int)PredefinedColour.Blue].Value,
          Colour.Colours[(int)PredefinedColour.White].Value
        };
        Shapes.MeshShape points = new Shapes.MeshShape(MeshDrawType.Points, pts, ids.NextShapeId++);
        points.Colours = colours;
        shapes.Add(points);
        // if (!noMove)
        // {
        //   movers.Add(new Oscilator(mesh, 2.0f, 2.5f));
        // }
      }

      if (allShapes || HaveOption("cloud", args) || HaveOption("cloudpart", args))
      {
        Shapes.MeshResource cloud = CreateTestCloud(ids);
        Shapes.PointCloudShape points = new Shapes.PointCloudShape(cloud, ids.NextShapeId++, (byte)16);
        if (HaveOption("cloudpart", args))
        {
          // Partial indexing.
          List<uint> partialIndices = new List<uint>();
          uint nextIndex = 0;
          for (int i = 0; i < partialIndices.Count; ++i)
          {
            partialIndices.Add(nextIndex);
            nextIndex += 2;
          }
          points.SetIndices(partialIndices.ToArray());
        }
        shapes.Add(points);
        resources.Add(cloud);
        // if (!noMove)
        // {
        //   movers.Add(new Oscilator(points, 2.0f, 2.5f));
        // }
      }

      if (HaveOption("wire", args))
      {
        for (int i = initialShapeCount; i < shapes.Count; ++i)
        {
          shapes[i].Wireframe = true;
        }
      }

      // Position the shapes so they aren't all on top of one another.
      if (shapes.Count > initialShapeCount)
      {
        Vector3 pos = Vector3.Zero;
        const float spacing = 2.0f;
        pos.X -= spacing * ((shapes.Count - initialShapeCount) / 2u);

        for (int i = initialShapeCount; i < shapes.Count; ++i)
        {
          shapes[i].Position = pos;
          pos.X += spacing;
        }

        foreach (ShapeMover mover in movers)
        {
          mover.Reset();
        }
      }


      // Add text after positioning and mover changes to keep fixed positions.
      if (allShapes || HaveOption("text2d", args))
      {
        Shapes.Text2D text;
        text = new Shapes.Text2D("Hello Screen", ids.NextShapeId++, new Vector3(0.25f, 0.75f, 0.0f));
        shapes.Add(text);
        text = new Shapes.Text2D("Hello World 2D", ids.NextShapeId++, new Vector3(1.0f, 1.0f, 1.0f));
        text.InWorldSpace = true;
        shapes.Add(text);
      }

      if (allShapes || HaveOption("text3d", args))
      {
        Shapes.Text3D text;
        text = new Shapes.Text3D("Hello World 3D", ids.NextShapeId++, new Vector3(-1.0f, -1.0f, 1.0f));
        text.FontSize = 16;
        shapes.Add(text);
        text = new Shapes.Text3D("Hello World 3D Facing", ids.NextShapeId++, new Vector3(-1.0f, -1.0f, 0.0f), 8);
        text.FontSize = 16;
        text.ScreenFacing = true;
        shapes.Add(text);
      }

      // Did we create anything?
      if (initialShapeCount == shapes.Count)
      {
        // Nothing created. Create the default shape by providing some fake arguments.
        string[] defaultArgs = new string[]
        {
          "sphere"
        };

        CreateShapes(ids, shapes, movers, resources, defaultArgs);
      }
    }

    static void Main(string[] args)
    {
      if (HaveOption("help", args))
      {
        ShowUsage();
        return;
      }

      Console.CancelKeyPress += new ConsoleCancelEventHandler(ControlCHandler);
      
      ServerSettings serverSettings = ServerSettings.Default;
      serverSettings.PortRange = 10;
      ServerInfoMessage info = ServerInfoMessage.Default;
      info.CoordinateFrame = CoordinateFrame.XYZ;
      if (HaveOption("compress", args))
      {
        serverSettings.Flags |= ServerFlag.Compress;
      }

      IServer server = new TcpServer(serverSettings, info);
      List<Shapes.Shape> shapes = new List<Shapes.Shape>();
      List<ShapeMover> movers = new List<ShapeMover>();
      List<Resource> resources = new List<Resource>();
      Ids ids = new Ids();
      ids.NextShapeId = 1;

      CreateAxes(ids, shapes, movers, resources, args);
      CreateShapes(ids, shapes, movers, resources, args);

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

      if (!server.ConnectionMonitor.Start(ConnectionMonitorMode.Asynchronous))
      {
        Console.WriteLine("Failed to start listen socket.");
        return;
      }
      Console.WriteLine(string.Format("Listening on port {0}", server.ConnectionMonitor.Port));

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

        foreach (ShapeMover mover in movers)
        {
          mover.Update(time, dt);
          server.Update(mover.Shape);
        }

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

    private static void ControlCHandler(object sender, ConsoleCancelEventArgs args)
    {
      Quit = true;
      args.Cancel = true;
    }
  }
}
