﻿// // Copyright (c) CSIRO 2016
// // Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// // ABN 41 687 119 230
// //
// // author Kazys Stepanas
//
using System;
using System.Collections.Generic;
using System.IO;
using Tes.IO;
using Tes.Maths;
using Tes.Net;
using Tes.Shapes;

namespace Tes.TestSupport
{
  /// <summary>
  /// A utility class for generating simple scene packet streams for testing.
  /// </summary>
  public static class SceneGenerator
  {
    /// <summary>
    /// Generate a simple, multi-frame scene and return the results in a decodable stream object.
    /// </summary>
    /// <param name="collate">True use use collated packets.</param>
    /// <param name="compress">True to compress collated packets.</param>
    /// <returns>A stream object containing packets which define the scene.</returns>
    /// <remarks>
    /// The returned stream can be decoded using a <see cref="T:PacketStreamReader"/>.
    /// </remarks>
    public static Stream Generate(bool collate, bool compress)
    {
      MemoryStream stream = new MemoryStream(16 * 1024);
      PopulateScene(stream, collate, compress);
      return new MemoryStream(stream.GetBuffer(), 0, (int)stream.Position, false);
    }

    /// <summary>
    /// Populates <paramref name="stream"/> with a series of data packets describing a simple, multi-frame scene.
    /// </summary>
    /// <param name="stream">The stream to write the scene packets to.</param>
    public static void PopulateScene(Stream stream, bool collate, bool compress)
    {
      CollatedPacketEncoder collator = (collate) ? new CollatedPacketEncoder(compress) : null;
      List<Shape> shapes = new List<Shape>();
      PacketBuffer packet = new PacketBuffer(16 * 1024);
      uint objId = 1;
      Vector3 pos = new Vector3(0, 0, 0);
      Vector3 posDelta = new Vector3(0.5f, 0.5f, 0.5f);
      ControlMessage endFrameMsg = new ControlMessage();

      endFrameMsg.Value32 = 1000000 / 30; // ~30Hz

      // Initialise scene objects.
      shapes.Add(new Arrow(objId++, pos, pos + new Vector3(0.2f, 0, 1), 0.05f));
      pos += posDelta;

      shapes.Add(new Box(objId++, pos, new Vector3(0.2f, 0.3f, 0.5f)));
      pos += posDelta;

      shapes.Add(new Capsule(objId++, pos, pos + new Vector3(0, 0, 2.0f), 0.2f));
      pos += posDelta;

      shapes.Add(new Cone(objId++, pos, pos + new Vector3(0, 0, 1.0f), 0.2f));
      pos += posDelta;

      shapes.Add(new Cylinder(objId++, pos, pos + new Vector3(0, 0, 2.0f), 0.2f));
      pos += posDelta;

      shapes.Add(new Sphere(objId++, pos));
      pos += posDelta;

      shapes.Add(new Plane(objId++, pos, new Vector3(1, 0.5f, 0.3f).Normalised));
      pos += posDelta;

      shapes.Add(new Star(objId++, pos));
      pos += posDelta;

      // Serialise creation packets.
      uint progress = 0;
      int res = 0;
      foreach (Shape shape in shapes)
      {
        shape.WriteCreate(packet);
        WritePacket(stream, collator, packet);

        if (shape.IsComplex)
        {
          while ((res = shape.WriteData(packet, ref progress)) >= 0)
          {
            WritePacket(stream, collator, packet);
          }
        }
      }

      // Write a frame flush.
      packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.EndFrame);
      endFrameMsg.Write(packet);
      WritePacket(stream, collator, packet);
      Flush(stream, collator);

      // Move things around.
      for (int i = 0; i < 100; ++i)
      {
        posDelta = new Vector3(3.0f * (float)Math.Sin(i * (5.0 / Math.PI * 180)), 0, 0);
        foreach (Shape s in shapes)
        {
          s.Position = s.Position + posDelta;
          s.WriteUpdate(packet);
          WritePacket(stream, collator, packet);
        }

        // Write a frame flush.
        packet.Reset((ushort)RoutingID.Control, (ushort)ControlMessageID.EndFrame);
        endFrameMsg.Write(packet);
        WritePacket(stream, collator, packet);
        Flush(stream, collator);
      }

      Flush(stream, collator);
      stream.Flush();
    }

    /// <summary>
    /// Helper function to write a packet to a stream with an optional <see cref="T:CollatedPacketEncoder"/>.
    /// </summary>
    /// <param name="stream">Stream to write <paramref name="packet"/> to.</param>
    /// <param name="encoder">Optional collated packet encoder. May be null for no collation.</param>
    /// <param name="packet">The packet data to write.</param>
    /// <remarks>
    /// The <paramref name="encoder"/> is flushed if required.
    /// </remarks>
    static void WritePacket(Stream stream, CollatedPacketEncoder encoder, PacketBuffer packet)
    {
      if (packet.Status != PacketBufferStatus.Complete)
      {
        if (!packet.FinalisePacket())
        {
          throw new IOException("Packet finalisation failed.");
        }
      }

      if (encoder == null)
      {
        stream.Write(packet.Data, 0, packet.Count);
      }
      else
      {
        if (encoder.Add(packet) == -1)
        {
          // Failed to add. Try flushing the packet first.
          Flush(stream, encoder);
          if (encoder.Add(packet) == -1)
          {
            // Failed after flush. Raise exception.
            throw new IOException("Failed to collate data packet after flush.");
          }
        }
      }
    }

    /// <summary>
    /// A helper function for flushing <paramref name="encoder"/> into <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">The stream to flush into.</param>
    /// <param name="encoder">The collated packet encoder to flush and reset. May be null (noop).</param>
    static void Flush(Stream stream, CollatedPacketEncoder encoder)
    {
      if (encoder != null)
      {
        // Ensure we have some data to flush.
        if (encoder.CollatedBytes > 0)
        {
          if (!encoder.FinaliseEncoding())
          {
            throw new IOException("Collated encoding finalisation failed.");
          }

          stream.Write(encoder.Buffer, 0, encoder.Count);
          encoder.Reset();
        }
      }
    }
  }
}
