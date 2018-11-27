// Copyright (c) CSIRO 2016
// Commonwealth Scientific and Industrial Research Organisation (CSIRO)
// ABN 41 687 119 230
//
// author Kazys Stepanas
//
using Xunit;
using System;
using System.Diagnostics;
using System.IO;
using Tes.IO;
using Tes.TestSupport;

namespace Tes.CoreTests
{
  public class Packets
  {
    /// <summary>
    /// Test generalised packet streaming.
    /// </summary>
    /// <param name="collatedPackets">Use collated packets?</param>
    /// <param name="compressed">Use compressed packets? Requires <paramref name="collatedPackets"/>.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PacketStreamTests(bool collatedPackets, bool compressed)
    {
      Stream sceneStream = SceneGenerator.Generate(collatedPackets, compressed);
      Stopwatch timer = new Stopwatch();
      timer.Start();

      PacketStreamReader packetStream = new PacketStreamReader(sceneStream);
      PacketBuffer packet = null;

      // Reading
      Console.WriteLine("Reading to end of stream");
      long bytesRead = 0;
      uint packetCount1 = 0;
      uint packetCount2 = 0;
      while (!packetStream.EndOfStream)
      {
        packet = packetStream.NextPacket(ref bytesRead);
        if (!packetStream.EndOfStream)
        {
          Assert.NotNull(packet);//, "Failed to extract packet.");
          Assert.Equal(PacketBufferStatus.Complete, packet.Status);//, "Unpexected packet status: {0}", packet.Status.ToString());
          ++packetCount1;
        }
      }
      Console.WriteLine("Read {0} packets", packetCount1);

      Console.WriteLine("Resetting");
      packetStream.Reset();
      packetCount2 = 0;
      while (!packetStream.EndOfStream)
      {
        packet = packetStream.NextPacket(ref bytesRead);
        if (!packetStream.EndOfStream)
        {
          Assert.NotNull(packet);//, "Failed to extract packet.");
          Assert.Equal(PacketBufferStatus.Complete, packet.Status);//, "Unpexected packet status: {0}", packet.Status.ToString());
          ++packetCount2;
        }
      }
      Console.WriteLine("Read {0} packets", packetCount2);

      Assert.Equal(packetCount2, packetCount1);//, "Packet count mismatch after reset and replay.");

      timer.Stop();
      Console.WriteLine("Elapsed: {0}", timer.Elapsed);
    }
  }
}
