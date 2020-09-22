//
// author Kazys Stepanas
//
using Xunit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Tes.IO;
using Tes.Net;
using Tes.Server;
using Tes.Shapes;

namespace Tes.CoreTests
{
  public static class ShapeTestFramework
  {
    public delegate void ShapeValidationFunction(Shape shape, Shape reference, Dictionary<ulong, Resource> resources);
    public delegate Shape CreateShapeFunction();

    public static void TestShape(Shape reference, CreateShapeFunction createShape)
    {
      TestShape(reference, createShape, ShapeTestFramework.ValidateShape);
    }

    public static void TestShape(Shape reference, CreateShapeFunction createShape, ShapeValidationFunction validate)
    {
      ServerInfoMessage info = ServerInfoMessage.Default;
      info.CoordinateFrame = CoordinateFrame.XYZ;

      ServerSettings serverSettings = ServerSettings.Default;
      serverSettings.Flags &= ~(ServerFlag.Collate | ServerFlag.Compress);
      serverSettings.PortRange = 1000;
      IServer server = new TcpServer(serverSettings);

      // std::cout << "Start on port " << serverSettings.listenPort << std::endl;
      Assert.True(server.ConnectionMonitor.Start(ConnectionMonitorMode.Asynchronous));
      // std::cout << "Server listening on port " << server.connectionMonitor()->port() << std::endl;;

      // Create client and connect.

      TcpClient client = new TcpClient("127.0.0.1", server.ConnectionMonitor.Port);

      // Wait for connection.
      if (server.ConnectionMonitor.WaitForConnections(5000u) > 0)
      {
        server.ConnectionMonitor.CommitConnections(null);
      }

      Assert.True(server.ConnectionCount > 0);
      Assert.True(client.Connected);

      // Send server messages from another thread. Otherwise large packets may block.
      Thread sendThread = new Thread(() =>
      {
        Assert.True(server.Create(reference) > 0);
        Assert.True(server.UpdateTransfers(0) >= 0);
        Assert.True(server.UpdateFrame(0.0f) > 0);

        // Send end message.
        ControlMessage ctrlMsg = new ControlMessage();
        Assert.True(ServerUtil.SendMessage(server, (ushort)RoutingID.Control, (ushort)ControlMessageID.End, ctrlMsg) > 0);
        Assert.True(server.UpdateFrame(0.0f) > 0);
      });
      sendThread.Start();

      // Process client messages.
      ValidateClient(client, reference, info, createShape, validate);

      client.Close();

      sendThread.Join();
      server.Close();

      server.ConnectionMonitor.Stop();
      server.ConnectionMonitor.Join();
    }

    static void ValidateClient(TcpClient socket, Shape reference, ServerInfoMessage serverInfo, CreateShapeFunction createShape, ShapeValidationFunction validate, uint timeoutSec = 10u)
    {
      Stopwatch timer = new Stopwatch();
      ServerInfoMessage readServerInfo = new ServerInfoMessage();
      Dictionary<ulong, Resource> resources = new Dictionary<ulong, Resource>();
      PacketBuffer packetBuffer = new PacketBuffer(64 * 1024);
      Shape shape = createShape();
      bool endMsgReceived = false;
      bool serverInfoRead = false;
      bool shapeMsgRead = false;

      timer.Start();

      // Keep looping until we get a CIdEnd ControlMessage or timeoutSec elapses.
      // Timeout ignored when debugger is attached.
      while (!endMsgReceived && (Debugger.IsAttached || timer.ElapsedMilliseconds / 1000u < timeoutSec))
      {
        if (socket.Available <= 0)
        {
          Thread.Yield();
          continue;
        }

        // Data available. Read from the network stream into a buffer and attempt to
        // read a valid message.
        packetBuffer.Append(socket.GetStream(), socket.Available);

        PacketBuffer completedPacket;
        bool crcOk = true;
        while ((completedPacket = packetBuffer.PopPacket(out crcOk)) != null || !crcOk)
        {
          Assert.True(crcOk);
          if (crcOk)
          {
            if (packetBuffer.DroppedByteCount != 0)
            {
              Console.Error.WriteLine("Dropped {0} bad bytes", packetBuffer.DroppedByteCount);
              packetBuffer.DroppedByteCount = 0;
            }

            Assert.Equal(PacketHeader.PacketMarker, completedPacket.Header.Marker);
            Assert.Equal(PacketHeader.PacketVersionMajor, completedPacket.Header.VersionMajor);
            Assert.Equal(PacketHeader.PacketVersionMinor, completedPacket.Header.VersionMinor);

            NetworkReader packetReader = new NetworkReader(completedPacket.CreateReadStream(true));

            switch (completedPacket.Header.RoutingID)
            {
              case (int)RoutingID.ServerInfo:
                serverInfoRead = true;
                Assert.True(readServerInfo.Read(packetReader));

                // Validate server info.
                Assert.Equal(serverInfo.TimeUnit, readServerInfo.TimeUnit);
                Assert.Equal(serverInfo.DefaultFrameTime, readServerInfo.DefaultFrameTime);
                Assert.Equal(serverInfo.CoordinateFrame, readServerInfo.CoordinateFrame);

                unsafe
                {
                  for (int i = 0; i < ServerInfoMessage.ReservedBytes; ++i)
                  {
                    Assert.Equal(serverInfo.Reserved[i], readServerInfo.Reserved[i]);
                  }
                }
                break;

              case (int)RoutingID.Control:
                {
                  // Only interested in the CIdEnd message to mark the end of the stream.
                  ControlMessage msg = new ControlMessage();
                  Assert.True(msg.Read(packetReader));

                  if (completedPacket.Header.MessageID == (int)ControlMessageID.End)
                  {
                    endMsgReceived = true;
                  }
                  break;
                }

              case (int)RoutingID.Mesh:
                HandleMeshMessage(completedPacket, packetReader, resources);
                break;

              default:
                if (completedPacket.Header.RoutingID == reference.RoutingID)
                {
                  shapeMsgRead = true;
                  HandleShapeMessage(completedPacket, packetReader, shape, reference);
                }
                break;
            }
          }
        }
        // else fail?
      }

      Assert.True(serverInfoRead);
      Assert.True(shapeMsgRead);
      Assert.True(endMsgReceived);

      // Validate the shape state.
      if (shapeMsgRead)
      {
        validate(shape, reference, resources);
      }
    }


    static void HandleShapeMessage(PacketBuffer packet, NetworkReader reader, Shape shape, Shape reference)
    {
      // Shape message the shape.
      uint shapeId = 0;

      // Peek the shape ID.
      shapeId = packet.PeekUInt32(PacketHeader.Size);

      Assert.Equal(shapeId, reference.ID);

      switch (packet.Header.MessageID)
      {
        case (int)ObjectMessageID.Create:
          Assert.True(shape.ReadCreate(packet, reader));
          break;

        case (int)ObjectMessageID.Update:
          Assert.True(shape.ReadUpdate(packet, reader));
          break;

        case (int)ObjectMessageID.Data:
          Assert.True(shape.ReadData(packet, reader));
          break;
      }
    }


    static void HandleMeshMessage(PacketBuffer packet, NetworkReader reader, Dictionary<ulong, Resource> resources)
    {
      uint meshId = 0;
      // Peek the mesh ID.
      meshId = packet.PeekUInt32(PacketHeader.Size);

      Resource resource;
      SimpleMesh mesh = null;

      // If it exists, make sure it's a mesh.
      if (resources.TryGetValue(ResourceUtil.UniqueKey(new PlaceholderMesh(meshId)), out resource))
      {
        Assert.Equal((ushort)RoutingID.Mesh, resource.TypeID);
        mesh = (SimpleMesh)resource;
      }

      switch (packet.Header.MessageID)
      {
        case (int)MeshMessageType.Invalid:
          Assert.True(false, "Invalid mesh message sent");
          break;

        case (int)MeshMessageType.Destroy:
          Assert.NotNull(mesh);
          resources.Remove(mesh.UniqueKey());
          break;

        case (int)MeshMessageType.Create:
          // Create message. Should not already exists.
          Assert.Null(mesh);//, "Recreating existing mesh.");
          mesh = new SimpleMesh(meshId);
          Assert.True(mesh.ReadCreate(packet, reader));
          resources.Add(mesh.UniqueKey(), mesh);
          break;

        // Not handling these messages.
        case (int)MeshMessageType.Redefine:
        case (int)MeshMessageType.Finalise:
          break;

        default:
          Assert.NotNull(mesh);
          mesh.ReadTransfer(packet.Header.MessageID, reader);
          break;
      }
    }

    public static void ValidateShape(Shape shape, Shape reference, Dictionary<ulong, Resource> resources)
    {
      Assert.Equal(reference.RoutingID, shape.RoutingID);
      Assert.Equal(reference.IsComplex, shape.IsComplex);

      Assert.Equal(reference.ID, shape.ID);
      Assert.Equal(reference.Category, shape.Category);
      Assert.Equal(reference.Flags, shape.Flags);
      //Assert.Equal(reference.data().reserved, shape.data().reserved);
      //Assert.Equal(reference.ID, shape.ID);

      Assert.Equal(reference.Colour, shape.Colour);

      Assert.Equal(reference.X, shape.X);
      Assert.Equal(reference.Y, shape.Y);
      Assert.Equal(reference.Z, shape.Z);

      Assert.Equal(reference.Rotation, shape.Rotation);

      Assert.Equal(reference.ScaleX, shape.ScaleX);
      Assert.Equal(reference.ScaleY, shape.ScaleY);
      Assert.Equal(reference.ScaleZ, shape.ScaleZ);
    }

    public static void ValidateShape(MeshShape shape, MeshShape reference, Dictionary<ulong, Resource> resources)
    {
      ValidateShape((Shape)shape, (Shape)reference, resources);

      Assert.Equal(reference.DrawType, shape.DrawType);

      if (reference.Vertices != null)
      {
        Assert.NotNull(shape.Vertices);
        Assert.Equal(reference.Vertices.Count, shape.Vertices.Count);
        Assert.Equal(reference.Vertices.ComponentCount, shape.Vertices.ComponentCount);
      }

      if (reference.Normals != null)
      {
        Assert.NotNull(shape.Normals);
        Assert.Equal(reference.Normals.Count, shape.Normals.Count);
        Assert.Equal(reference.Normals.ComponentCount, shape.Normals.ComponentCount);
      }

      if (reference.Colours != null)
      {
        Assert.NotNull(shape.Colours);
        Assert.Equal(reference.Colours.Count, shape.Colours.Count);
        Assert.Equal(reference.Colours.ComponentCount, shape.Colours.ComponentCount);
      }

      if (reference.Indices != null)
      {
        Assert.NotNull(shape.Indices);
        Assert.Equal(reference.Indices.Count, shape.Indices.Count);
        Assert.Equal(reference.Indices.ComponentCount, shape.Indices.ComponentCount);
      }

      if (reference.Vertices != null)
      {
        for (int i = 0; i < reference.Vertices.Count * reference.Vertices.ComponentCount; ++i)
        {
          Assert.Equal(reference.Vertices.GetSingle(i), shape.Vertices.GetSingle(i));
        }
      }

      if (reference.Normals != null)
      {
        for (int i = 0; i < reference.Normals.Count * reference.Normals.ComponentCount; ++i)
        {
          Assert.Equal(reference.Normals.GetSingle(i), shape.Normals.GetSingle(i));
        }
      }

      if (reference.Colours != null)
      {
        for (int i = 0; i < reference.Colours.Count * reference.Colours.ComponentCount; ++i)
        {
          Assert.Equal(reference.Colours.GetUInt32(i), shape.Colours.GetUInt32(i));
        }
      }

      if (reference.Indices != null)
      {
        for (int i = 0; i < reference.Indices.Count * reference.Indices.ComponentCount; ++i)
        {
          Assert.Equal(reference.Indices.GetUInt32(i), shape.Indices.GetUInt32(i));
        }
      }
    }
  }
}
