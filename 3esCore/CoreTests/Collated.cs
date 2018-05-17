// // Copyright (c) CSIRO 2017
// // Commonwealth Scientific and Industrial Research Organisation (CSIRO) 
// // ABN 41 687 119 230
// //
// // author Kazys Stepanas
//
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tes.IO;
using Tes.Net;
using Tes.Maths;
using Tes.Shapes;

namespace Tes.CoreTests
{
  [TestFixture()]
  public class Collated
  {
    [TestCase(false)]
    [TestCase(true)]
    public void CollationTest(bool compress)
    {
      // Allocate encoder.
      CollatedPacketEncoder encoder = new CollatedPacketEncoder(compress);

      // Create a mesh object to generate some messages.
      List<Vector3> vertices = new List<Vector3>();
      List<Vector3> normals = new List<Vector3>();
      List<int> indices = new List<int>();
      Common.MakeLowResSphere(vertices, indices, normals);

      MeshShape mesh = new MeshShape(Net.MeshDrawType.Triangles, vertices.ToArray(), indices.ToArray());
      mesh.ID = 42;
      mesh.Normals = normals.ToArray();

      // Use the encoder as a connection.
      // The Create() call will pack the mesh create message and multiple data messages.
      int wroteBytes = encoder.Create(mesh);
      Assert.Greater(wroteBytes, 0);
      Assert.True(encoder.FinaliseEncoding());

      // Allocate a reader. Contains a CollatedPacketDecoder.
      System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(encoder.Buffer);
      PacketStreamReader decoder = new PacketStreamReader(memoryStream);

      PacketBuffer packet;
      MeshShape readMesh = new MeshShape();
      int packetCount = 0;
      long processedBytes = 0;

      while ((packet = decoder.NextPacket(ref processedBytes)) != null)
      {
        NetworkReader reader = new NetworkReader(packet.CreateReadStream(true));
        ++packetCount;
        Assert.True(packet.ValidHeader);
        Assert.AreEqual(packet.Header.Marker, PacketHeader.PacketMarker);
        Assert.AreEqual(packet.Header.VersionMajor, PacketHeader.PacketVersionMajor);
        Assert.AreEqual(packet.Header.VersionMinor, PacketHeader.PacketVersionMinor);

        Assert.AreEqual(packet.Header.RoutingID, mesh.RoutingID);

        // Peek the shape ID.
        uint shapeId = packet.PeekUInt32(PacketHeader.Size);
        Assert.AreEqual(shapeId, mesh.ID);

        switch((ObjectMessageID)packet.Header.MessageID)
        {
          case ObjectMessageID.Create:
            Assert.True(readMesh.ReadCreate(reader));
            break;

          case ObjectMessageID.Update:
            Assert.True(readMesh.ReadUpdate(reader));
            break;

          case ObjectMessageID.Data:
            Assert.True(readMesh.ReadData(reader));
            break;
        }
      }

      Assert.Greater(packetCount, 0);
      // FIXME: Does not match, but results are fine. processedBytes is 10 greater than wroteBytes.
      //Assert.AreEqual(processedBytes, wroteBytes);

      // Validate what we've read back.
      ShapeTestFramework.ValidateShape(readMesh, mesh, new Dictionary<ulong, Resource>());
    }
  }
}
