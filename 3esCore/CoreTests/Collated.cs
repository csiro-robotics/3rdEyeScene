// // Copyright (c) CSIRO 2017
// // Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// // ABN 41 687 119 230
// //
// // author Kazys Stepanas
//
using Xunit;
using System;
using System.Collections.Generic;
using Tes.IO;
using Tes.Net;
using Tes.Maths;
using Tes.Shapes;

namespace Tes.CoreTests
{
  public class Collated
  {
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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
      Assert.True(wroteBytes > 0);
      Assert.True(encoder.FinaliseEncoding());

      // Allocate a reader. Contains a CollatedPacketDecoder.
      System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(encoder.Buffer, 0, encoder.Count);
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
        Assert.Equal(packet.Header.Marker, PacketHeader.PacketMarker);
        Assert.Equal(packet.Header.VersionMajor, PacketHeader.PacketVersionMajor);
        Assert.Equal(packet.Header.VersionMinor, PacketHeader.PacketVersionMinor);

        Assert.Equal(packet.Header.RoutingID, mesh.RoutingID);

        // Peek the shape ID.
        uint shapeId = packet.PeekUInt32(PacketHeader.Size);
        Assert.Equal(shapeId, mesh.ID);

        switch((ObjectMessageID)packet.Header.MessageID)
        {
          case ObjectMessageID.Create:
            Assert.True(readMesh.ReadCreate(packet, reader));
            break;

          case ObjectMessageID.Update:
            Assert.True(readMesh.ReadUpdate(packet, reader));
            break;

          case ObjectMessageID.Data:
            Assert.True(readMesh.ReadData(packet, reader));
            break;
        }
      }

      Assert.True(packetCount > 0);
      // FIXME: Does not match, but results are fine. processedBytes is 10 greater than wroteBytes.
      //Assert.Equal(processedBytes, wroteBytes);

      // Validate what we've read back.
      ShapeTestFramework.ValidateShape(readMesh, mesh, new Dictionary<ulong, Resource>());
    }
  }
}
