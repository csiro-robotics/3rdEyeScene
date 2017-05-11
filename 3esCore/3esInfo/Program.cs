﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Tes.IO;
using Tes.Net;

namespace Tes
{
  class Program
  {
    public bool Quit { get; set; }
    public bool ArgsOk { get; private set; }
    public bool ShowUsage { get; private set; }
    public bool Decode { get; private set; }

    public string TargetFile { get; private set; }

    static void Main(string[] args)
    {
      Program prog = new Program(args);

      if (prog.ShowUsage)
      {
        Usage();
        return;
      }

      if (prog.ArgsOk)
      {
        prog.Run();
      }
      else
      {
        Usage();
      }
    }

    Program(string[] args)
    {
      if (args.Length > 0)
      {
        ParseArgs(args);
      }
      else
      {
        Usage();
      }
    }

    public static void Usage()
    {
      Console.Write(string.Format(
@"Usage:
  3esInfo [options] <file.3es>

This program provides information about a 3rd Eye Scene file.
--help, -?:
  Show usage.
-d:
  Decode the entire stream to verify the data packets and frame count.
")
      );
    }


    public void Run()
    {
      int infoSearchPacketLimit = 10; // Don't look for info packets beyond this packet count. Should be first up.
      PacketBuffer packetBuffer = new PacketBuffer(4 * 1024);
      PacketStreamReader packetStream = new PacketStreamReader(new FileStream(TargetFile, FileMode.Open, FileAccess.Read));

      Console.CancelKeyPress += new ConsoleCancelEventHandler(ControlCHandler);
      Console.WriteLine(string.Format("Reading {0}", TargetFile));

      PacketBuffer packet = null;
      long bytesRead = 0;
      bool foundFrameCount = false;
      bool foundServerInfo = false;
      while (!Quit && !packetStream.EndOfStream)
      {
        packet = packetStream.NextPacket(ref bytesRead);

        if (packet == null)
        {
          if (!packetStream.EndOfStream)
          {
            Console.Error.WriteLine(string.Format("Null packet at {0}:{1}", _actualFrameCount, _packetCount));
          }
          continue;
        }

        if (packet.Status == PacketBufferStatus.Complete)
        {
          ++_packetCount;
          //Console.WriteLine("Msg: {0} {1}", completedPacket.Header.RoutingID, completedPacket.Header.MessageID);
          switch (packet.Header.RoutingID)
          {
            case (ushort)RoutingID.Control:
              switch (packet.Header.MessageID)
              {
                case (ushort)ControlMessageID.EndFrame:
                  ++_actualFrameCount;
                  break;
                case (ushort)ControlMessageID.FrameCount:
                  if (foundFrameCount)
                  {
                    Console.Error.WriteLine(string.Format("Found additional FrameCount message at frame {0}:{1}",
                                            _actualFrameCount, _packetCount));
                  }
                  else
                  {
                    _frameCountPacketNumber = _packetCount;
                  }
                  HandleFrameCount(packet);
                  foundFrameCount = true;
                  break;
              }
              break;
            case (ushort)RoutingID.ServerInfo:
              if (foundServerInfo)
              {
                Console.Error.WriteLine(string.Format("Found additional ServerInfo message at frame {0}:{1}",
                                                  _actualFrameCount, _packetCount));
              }
              else
              {
                _serverInfoPacket = _packetCount;
              }
              HandleServerInfo(packet);
              foundServerInfo = true;
              break;
          }
        }
        else
        {
          Console.Error.WriteLine(string.Format("Invalid packet static at {0}:{1}", _actualFrameCount, _packetCount));
          Console.Error.WriteLine(string.Format("Invalid packet status is {0} ({1})", packet.Status.ToString(), (int)packet.Status));;

          if (packet.Header.RoutingID == (ushort)RoutingID.CollatedPacket && packet.Header.PayloadSize > 0)
          {
            NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
            CollatedPacketMessage msg = new CollatedPacketMessage();
            if (msg.Read(packetReader))
            {
              Console.WriteLine(string.Format("Failed collated packet message:"));
              Console.WriteLine(string.Format("  Flags: {0}", msg.Flags));
              Console.WriteLine(string.Format("  Reserved: {0}", msg.Reserved));
              Console.WriteLine(string.Format("  UncompressedBytes: {0}", msg.UncompressedBytes));
              Console.WriteLine(string.Format("  PacketSize: {0}", packet.Header.PacketSize));
            }
          }
        }
      }

      if (!Decode)
      {
        if (foundServerInfo && foundFrameCount)
        {
          Quit = true;
        }
      }

      if (_packetCount >= infoSearchPacketLimit)
      {
        if (!foundServerInfo)
        {
          Quit = true;
          Console.Error.WriteLine(string.Format("Failed to locate ServerInfo packet within {0} packets.", infoSearchPacketLimit));
        }
        if (!foundFrameCount)
        {
          Quit = true;
          Console.Error.WriteLine(string.Format("Failed to locate FrameCount packet within {0} packets.", infoSearchPacketLimit));
        }
      }

      Console.WriteLine(string.Format("Processed {0} packets{1}\n", _packetCount, Decode ? "" : " (info only)"));
      if (Decode)
      {
        if (_reportedFrameCount != _actualFrameCount)
        {
          Console.Error.WriteLine(string.Format("Frame count mismatch. Expected {0}, processed {1}", _reportedFrameCount, _actualFrameCount));
        }
      }
    }


    private void HandleFrameCount(PacketBuffer packet)
    {
      NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
      ControlMessage msg = new ControlMessage();
      if (msg.Read(packetReader))
      {
        _reportedFrameCount = msg.Value32;
        Console.WriteLine("Total frames: {0}", _reportedFrameCount);
      }
      else
      {
        Console.Error.WriteLine(string.Format("Failed to decode FrameCount message at {0}:{1}",
                                              _reportedFrameCount, _packetCount));
      }
    }


    private void HandleServerInfo(PacketBuffer packet)
    {
      NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));

      if (_serverInfo.Read(packetReader))
      {
        Console.WriteLine(string.Format("Info packet protocol version: {0}.{1}",
                                        packet.Header.VersionMajor, packet.Header.VersionMinor));
        ShowServerInfo(_serverInfo);
      }
      else
      {
        Console.Error.WriteLine(string.Format("Failed to decode ServerInfo message at {0}:{1}",
                            _reportedFrameCount, _packetCount));
      }
    }


    private string TimeString(ulong microseconds)
    {
      // Convert ms/s if we can.
      if (microseconds / 1000000 * 1000000 == microseconds)
      {
        // Can use seconds.
        return string.Format("{0}s", microseconds / 1000000);
      }
      else if (microseconds / 1000 * 1000 == microseconds)
      {
        // Can use milliseconds.
        return string.Format("{0}ms", microseconds / 1000);
      }
      return string.Format("{0}us", microseconds);
    }


    private void ShowServerInfo(ServerInfoMessage info)
    {
      Console.WriteLine("Server info");
      Console.WriteLine(string.Format("  Coordinate frame: {0} ({1})", info.CoordinateFrame.ToString(),
                                      info.IsLeftHanded ? "left" : "right"));
      Console.WriteLine(string.Format("  Time unit: {0}", TimeString(info.TimeUnit)));
      Console.WriteLine(string.Format("  Default time: {0} {1} Hz",
                                      TimeString(info.DefaultFrameTime * info.TimeUnit),
                                      1e6 / (info.DefaultFrameTime * info.TimeUnit)));
    }


    private void ParseArgs(string[] args)
    {
      bool ok = args.Length != 0;

      ArgsOk = false;
      for (int i = 0; i < args.Length; ++i)
      {
        if (args[i] == "--help" || args[i] == "-?" || args[i] == "-h")
        {
          ShowUsage = true;
        }
        else if (args[i] == "-d")
        {
          Decode = true;
        }
        else if (!args[i].StartsWith("-") && string.IsNullOrEmpty(TargetFile))
        {
          TargetFile = args[i];
        }
      }

      if (ok)
      {
        if (string.IsNullOrEmpty(TargetFile))
        {
          ok = false;
          Console.Error.WriteLine("No file specified.");
        }
        else if (!File.Exists(TargetFile))
        {
          ok = false;
          Console.Error.WriteLine(string.Format("No such file {0}", TargetFile));
        }
      }

      ArgsOk = ok;
    }


    private void ControlCHandler(object sender, ConsoleCancelEventArgs args)
    {
      Quit = true;
      args.Cancel = true;
    }

    /// <summary>
    /// Initialised to the latest incoming server info message received (normally on connection).
    /// </summary>
    private ServerInfoMessage _serverInfo = ServerInfoMessage.Default;
    private uint _reportedFrameCount = 0;
    private uint _actualFrameCount = 0;
    private uint _serverInfoPacket = 0;
    private uint _frameCountPacketNumber = 0;
    private uint _packetCount = 0;
  }
}
