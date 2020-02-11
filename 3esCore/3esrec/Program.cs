// #define PACKET_TIMING

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Tes.IO;
using Tes.Logging;
using Tes.Net;

namespace Tes
{
  class Program
  {
    public enum Mode
    {
      CollateAndCompress,
      CollateOnly,
      FileCompression,
      Uncompressed,
      Passthrough,
    }

#if PACKET_TIMING
    private const uint PacketLimit = 500u;
#endif // PACKET_TIMING

    public bool Quit { get; set; }
    public bool ArgsOk { get; private set; }
    public bool ShowUsage { get; private set; }
    public bool Connected { get; private set; }
    public bool Persist { get; private set; }
    public bool Overwrite { get; private set; }
    public bool Quiet { get; private set; }

    public static Mode DefaultDecodeMode { get { return Mode.Passthrough; } }
    public Mode DecodeMode { get; private set; }
    public uint TotalFrames { get; private set; }
    public IPEndPoint ServerEndPoint { get; private set; }
    public string OutputPrefix { get; private set; }
    public static string DefaultPrefix { get { return "tes"; } }
    public static int DefaultPort { get { return 33500; } }
    public static string DefaultIP { get { return "127.0.0.1"; } }

    public static string[] DefaultArgs
    {
      get
      {
        return new string[]
        {
          "--ip", DefaultIP,
          "--port", DefaultPort.ToString()
        };
      }
    }

    public static readonly string[] ModeArgStrings = new string[]
    {
      "mc",
      "mC",
      "mz",
      "mu",
      "m-"
    };

    public static string ModeToArg(Mode m)
    {
      return ModeArgStrings[(int)m];
    }

    public static Mode ArgToMode(string arg)
    {
      while (arg.Length > 0 && arg[0] == '-')
      {
        arg = arg.Substring(1);
      }

      for (int i = 0; i < ModeArgStrings.Length; ++i)
      {
        if (ModeArgStrings[i].CompareTo(arg) == 0)
        {
          return (Mode)i;
        }
      }

      return DefaultDecodeMode;
    }

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
        FrameDisplay frameDisplay = new FrameDisplay();
        try
        {
          prog.Run(frameDisplay);
        }
        finally
        {
          // Ensure frameDisplay thread terminates.
          frameDisplay.Stop();
        }
      }
      else
      {
        Usage();
      }
    }

    Program(string[] args)
    {
      DecodeMode = DefaultDecodeMode;
      if (args.Length > 0)
      {
        ParseArgs(args);
      }
      else
      {
        ParseArgs(DefaultArgs);
      }
    }

    public static void Usage()
    {
      Console.Write(string.Format(
@"Usage:
  3esrec --ip <server-ip> [--port <server-port>] [prefix]

This program attempts to connect to and record a Third Eye Scene server.
--help, -?:
  Show usage.

--ip <server-ip>:
  Specifies the server IP address to connect to.

-m[c,C,v,z,-]:
  Specifies how incoming packets are handled. In all modes except m-, incoming
  collated packets are first decoded.
  - mc : Packet collation and compression. Recollate and compress.
  - mC : Packet collation no compression. Recollate only.
  - mu : Uncompressed. Save packets as is. No compression.
  - mz : File level compression. Decode incoming packets and compress at the
         file level.
  - m- : Passthrough. Packets are saved exactly as they come in.
  The fastest mode is -m- as this performs no additional calculations other than
  CRC validation. However, this mode requires naked frame packets for accurate
  frame count finalisation.

  The default mode is: {0}

--port <server-port>:
  Specifies the port to connect on.  The default port is {1}

--persist, -p:
  Persist beyond the first connection. The program keeps running awaiting
  further connections. Use Control-C to terminate.

--quiet, -q:
  Run in quiet mode (disable non-critical logging).

--overwrite, -w:
  Overwrite existing files using the current prefix. The current session
  numbering will not overwrite until they loop to 0.

[prefix]:
  Specifies the file prefix used for recording. The recording file is
  formulated as {{prefix###.3es}}, where the number used is the first missing
  file up to 999. At that point the program will complain that there are no
  more available file names.
", ModeToArg(DefaultDecodeMode), DefaultPort)
      );
    }


    public void Run(FrameDisplay frameDisplay)
    {
      int connectionPollTimeSecMs = 250;
      TcpClient socket = null;
      PacketBuffer packetBuffer = null;
      CollatedPacketDecoder collatedDecoder = new CollatedPacketDecoder();
      BinaryWriter recordingWriter = null;
#if PACKET_TIMING
      Stopwatch timer = new Stopwatch();
#endif // PACKET_TIMING
      bool once = true;

      Console.CancelKeyPress += new ConsoleCancelEventHandler(ControlCHandler);

      if (!Quiet)
      {
        Console.WriteLine(string.Format("Connecting to {0}", ServerEndPoint));
      }

      while (!Quit && (Persist || once))
      {
        once = false;
        // First try establish a connection.
        while (!Quit && !Connected)
        {
          if ((socket = AttemptConnection()) != null)
          {
#if PACKET_TIMING
            timer.Reset();
            timer.Start();
#endif // PACKET_TIMING
            TotalFrames = 0u;
            frameDisplay.Reset();
            if (!Quiet)
            {
              frameDisplay.Start();
            }
            recordingWriter = CreateOutputWriter();
            if (recordingWriter != null)
            {
              Connected = true;
              // Create a new packet buffer for this connection.
              packetBuffer = new PacketBuffer(4 * 1024);
            }
            Log.Flush();
          }
          else
          {
            Log.Flush();
            // Wait the timeout period before attempting to reconnect.
            System.Threading.Thread.Sleep(connectionPollTimeSecMs);
          }
        }

        // Read while connected or data still available.
        while (!Quit && socket != null && (TcpClientUtil.Connected(socket) || socket.Available > 0))
        {
          // We have a connection. Read messages while we can.
          if (socket.Available > 0)
          {
            // Data available. Read from the network stream into a buffer and attempt to
            // read a valid message.
            packetBuffer.Append(socket.GetStream(), socket.Available);
            PacketBuffer completedPacket;
            bool exportPacket = true;
            bool crcOk = true;
            while ((completedPacket = packetBuffer.PopPacket(out crcOk)) != null || !crcOk)
            {
              if (crcOk)
              {
                if (packetBuffer.DroppedByteCount != 0)
                {
                  Console.Error.WriteLine("Dropped {0} bad bytes", packetBuffer.DroppedByteCount);
                  packetBuffer.DroppedByteCount = 0;
                }

                if (DecodeMode == Mode.Passthrough)
                {
                  completedPacket.ExportTo(recordingWriter);
                  // Approximate packets as frames. Not exactly correct though.
                  if (completedPacket.Header.RoutingID == (ushort) RoutingID.Control)
                  {
                    if (completedPacket.Header.MessageID == (ushort)ControlMessageID.EndFrame)
                    {
                      ++TotalFrames;
                      frameDisplay.IncrementFrame();
#if PACKET_TIMING
                      if (TotalFrames >= PacketLimit)
                      {
                        timer.Stop();
                        Quit = true;
                      }
#endif // PACKET_TIMING
                    }
                  }
                  continue;
                }

                // Decode and decompress collated packets. This will just return the same packet
                // if not collated.
                collatedDecoder.SetPacket(completedPacket);
                while ((completedPacket = collatedDecoder.Next()) != null)
                {
                  //Console.WriteLine("Msg: {0} {1}", completedPacket.Header.RoutingID, completedPacket.Header.MessageID);
                  switch (completedPacket.Header.RoutingID)
                  {
                  case (ushort)RoutingID.Control:
                    ushort controlMessageId = completedPacket.Header.MessageID;
                    if (controlMessageId == (ushort)ControlMessageID.EndFrame)
                    {
                      ++TotalFrames;
                      frameDisplay.IncrementFrame();
#if PACKET_TIMING
                      if (TotalFrames >= PacketLimit)
                      {
                        timer.Stop();
                        Quit = true;
                      }
#endif // PACKET_TIMING
                    }
                      break;

                  case (ushort)RoutingID.ServerInfo:
                    NetworkReader packetReader = new NetworkReader(completedPacket.CreateReadStream(true));
                    _serverInfo.Read(packetReader);
                      exportPacket = false;
                    break;

                  default:
                    break;
                  }

                  if (exportPacket)
                  {
                    completedPacket.ExportTo(recordingWriter);
                  }
                }
              }
              else
              {
                Console.Error.WriteLine("CRC Failure");
                // TODO: Log CRC failure.
              }
            }
            Log.Flush();
          }
          else
          {
            Log.Flush();
            System.Threading.Thread.Sleep(0);
          }
        }

        frameDisplay.Stop();

        if (packetBuffer != null && packetBuffer.DroppedByteCount != 0)
        {
          Console.Error.WriteLine("Dropped {0} bad bytes", packetBuffer.DroppedByteCount);
          packetBuffer.DroppedByteCount = 0;
        }

        if (recordingWriter != null)
        {
          FinaliseOutput(recordingWriter, TotalFrames);
          recordingWriter.Flush();
          recordingWriter.Close();
          recordingWriter = null;
        }

        if (!Quiet)
        {
          Console.WriteLine("Connection closed");
        }

        // Disconnected.
        if (socket != null)
        {
          socket.Close();
          socket = null;
        }

        Connected = false;

        // GC to force flushing streams and collect other resource.
        GC.Collect();
      }

#if PACKET_TIMING
      Console.WriteLine($"Processed {PacketLimit} packets in {timer.ElapsedMilliseconds}ms");
#endif // PACKET_TIMING
    }


    private TcpClient AttemptConnection()
    {
      TcpClient socket = new TcpClient();
      try
      {
        socket.Connect(ServerEndPoint);
        if (socket.Connected)
        {
          return socket;
        }
      }
      catch (SocketException e)
      {
        switch (e.ErrorCode)
        {
        // case 10061: // WSAECONNREFUSED (Mono)
        //   break;
        case 111: // Connection refused (.Net Core)
          break;
        default:
          Console.Error.WriteLine(e);
          break;
        }
      }
      catch (System.Exception e)
      {
        Console.Error.WriteLine(e);
      }

      if (socket != null)
      {
        socket.Close();
      }

      return null;
    }


    private BinaryWriter CreateOutputWriter()
    {
      try
      {
        string filePath = GenerateNewOutputFile();
        if (string.IsNullOrEmpty(filePath))
        {
          Console.WriteLine($"Unable to generate a numbered file name using the prefix: {OutputPrefix}. Try cleaning up the output directory.");
          return null;
        }
        Console.WriteLine($"Recording to: {filePath}");

        Stream stream = null;
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        stream = fileStream;

        // Write the recording header uncompressed to the file.
        // We'll rewind here later and update the frame count.
        // Write to a memory stream to prevent corruption of the file stream when we wrap it
        // in a GZipStream.
        MemoryStream headerStream = new MemoryStream(512);
        BinaryWriter writer = new NetworkWriter(headerStream);
        WriteRecordingHeader(writer);
        writer.Flush();
        byte[] headerBytes = headerStream.ToArray();

        // Copy header to the file.
        fileStream.Write(headerBytes, 0, headerBytes.Length);
        // Dispose of the temporary objects.
        headerBytes = null;
        writer = null;
        headerStream = null;

        switch (DecodeMode)
        {
          case Mode.CollateAndCompress:
            stream = new CollationStream(fileStream, true);
            break;
          case Mode.CollateOnly:
            stream = new CollationStream(fileStream, false);
            break;
          case Mode.FileCompression:
            stream = new GZipStream(fileStream, CompressionLevel.Fastest);
            break;
          case Mode.Uncompressed:
            stream = fileStream;
            break;
        }

        return new NetworkWriter(stream);
      }
      catch (Exception e)
      {
        Console.Error.WriteLine(e);
      }
      return null;
    }


    private string GenerateNewOutputFile()
    {
      const int maxFiles = 1000;
      _nextOutputNumber = _nextOutputNumber % maxFiles;
      for (int i = _nextOutputNumber; i < maxFiles; ++i)
      {
        string outputPath = string.Format("{0}{1:000}.3es", OutputPrefix, i);
        if (Overwrite || !File.Exists(outputPath))
        {
          _nextOutputNumber = i + 1;
          return outputPath;
        }
      }

      return null;
    }


    private void WriteRecordingHeader(BinaryWriter writer)
    {
      // First write a server info message to define the coordinate frame and other server details.
      PacketBuffer packet = new PacketBuffer();
      PacketHeader header = PacketHeader.Default;
      ControlMessage frameCountMsg = new ControlMessage();

      header.RoutingID = (ushort)RoutingID.ServerInfo;

      packet.Reset();
      packet.WriteHeader(header);
      _serverInfo.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);

      // Next write a placeholder control packet to define the total number of frames.
      header.RoutingID = (ushort)RoutingID.Control;
      header.MessageID = (ushort)ControlMessageID.FrameCount;
      header.PayloadSize = 0;
      header.PayloadOffset = 0;

      frameCountMsg.ControlFlags = 0;
      frameCountMsg.Value32 = 0;  // Placeholder. Frame count is currently unknown.
      frameCountMsg.Value64 = 0;
      packet.Reset();
      packet.WriteHeader(header);
      frameCountMsg.Write(packet);
      packet.FinalisePacket();
      packet.ExportTo(writer);
    }



    private void FinaliseOutput(BinaryWriter writer, uint frameCount)
    {
      // Rewind the stream to the beginning and find the first RoutingID.ServerInfo message
      // and RoutingID.Control message with a ControlMessageID.FrameCount ID. These should be
      // the first and second messages in the stream.
      // We'll limit searching to the first 5 messages.
      long serverInfoMessageStart = -1;
      long frameCountMessageStart = -1;
      writer.Flush();

      // Extract the stream from the writer. The first stream may be a GZip stream, in which case we must
      // extract the stream that is writing to instead as we can't rewind compression streams
      // and we wrote the header raw.
      writer.BaseStream.Flush();

      Stream outStream = null;
      CollationStream zipStream = writer.BaseStream as CollationStream;
      if (zipStream != null)
      {
        outStream = zipStream.BaseStream;
        zipStream.Flush();
      }
      else
      {
        outStream = writer.BaseStream;
      }

      // Check we are allowed to seek the stream.
      if (!outStream.CanSeek)
      {
        // Stream does not support seeking. The frame count will not be fixed.
        return;
      }

      // Record the initial stream position to restore later.
      long restorePos = outStream.Position;
      outStream.Seek(0, SeekOrigin.Begin);

      long streamPos = 0;

      byte[] headerBuffer = new byte[PacketHeader.Size];
      PacketHeader header = new PacketHeader();
      byte[] markerValidationBytes = BitConverter.GetBytes(Tes.IO.Endian.ToNetwork(PacketHeader.PacketMarker));
      byte[] markerBytes = new byte[markerValidationBytes.Length];
      bool markerValid = false;

      int attemptsRemaining = 5;
      int byteReadLimit = 0;

      markerBytes[0] = 0;
      while ((frameCountMessageStart < 0 || serverInfoMessageStart < 0) && attemptsRemaining > 0 && outStream.CanRead)
      {
        --attemptsRemaining;
        markerValid = false;

        // Limit the number of bytes we try read in each attempt.
        byteReadLimit = 1024;
        while (byteReadLimit > 0)
        {
          --byteReadLimit;
          outStream.Read(markerBytes, 0, 1);
          if (markerBytes[0] == markerValidationBytes[0])
          {
            markerValid = true;
            int i = 1;
            for (i = 1; markerValid && outStream.CanRead && i < markerValidationBytes.Length; ++i)
            {
              outStream.Read(markerBytes, i, 1);
              markerValid = markerValid && markerBytes[i] == markerValidationBytes[i];
            }

            if (markerValid)
            {
              break;
            }
            else
            {
              // We've failed to fully validate the maker. However, we did read and validate
              // one byte in the marker, then continued reading until the failure. It's possible
              // that the last byte read, the failed byte, may be the start of the actual marker.
              // We check this below, and if so, we rewind the stream one byte in order to
              // start validation from there on the next iteration. We can ignore the byte if
              // it is does not match the first validation byte. We are unlikely to ever make this
              // match though.
              --i;  // Go back to the last read byte.
              if (markerBytes[i] == markerValidationBytes[0])
              {
                // Potentially the start of a new marker. Rewind the stream to attempt to validate it.
                outStream.Seek(-1, SeekOrigin.Current);
              }
            }
          }
        }

        if (markerValid && outStream.CanRead)
        {
          // Potential packet target. Record the stream position at the start of the marker.
          streamPos = outStream.Position - markerBytes.Length;
          outStream.Seek(streamPos, SeekOrigin.Begin);

          // Test the packet.
          int bytesRead = outStream.Read(headerBuffer, 0, headerBuffer.Length);
          if (bytesRead == headerBuffer.Length)
          {
            // Create a packet.
            if (header.Read(new NetworkReader(new MemoryStream(headerBuffer, false))))
            {
              // Header is OK. Looking for RoutingID.Control
              if (header.RoutingID == (ushort)RoutingID.ServerInfo)
              {
                serverInfoMessageStart = streamPos;
              }
              else if (header.RoutingID == (ushort)RoutingID.Control)
              {
                // It's control message. Complete and validate the packet.
                // Read the header. Determine the expected size and read that much more data.
                PacketBuffer packet = new PacketBuffer(header.PacketSize + Crc16.CrcSize);
                packet.Emplace(headerBuffer, bytesRead);
                packet.Emplace(outStream, header.PacketSize + Crc16.CrcSize - bytesRead);
                if (packet.Status == PacketBufferStatus.Complete)
                {
                  // Packet complete. Extract the control message.
                  NetworkReader packetReader = new NetworkReader(packet.CreateReadStream(true));
                  ControlMessage message = new ControlMessage();
                  if (message.Read(packetReader) && header.MessageID == (ushort)ControlMessageID.FrameCount)
                  {
                    // Found the message location.
                    frameCountMessageStart = streamPos;
                  }
                }
              }
              else
              {
                // At this point, we've failed to find the right kind of header. We could use the payload size to
                // skip ahead in the stream which should align exactly to the next message.
                // Not done for initial testing.
              }
            }
          }
        }
      }

      if (serverInfoMessageStart >= 0)
      {
        // Found the correct location. Seek the stream to here and write a new FrameCount control message.
        outStream.Seek(serverInfoMessageStart, SeekOrigin.Begin);
        PacketBuffer packet = new PacketBuffer();

        header = PacketHeader.Create((ushort)RoutingID.ServerInfo, 0);

        packet.WriteHeader(header);
        _serverInfo.Write(packet);
        packet.FinalisePacket();
        BinaryWriter patchWriter = new Tes.IO.NetworkWriter(outStream);
        packet.ExportTo(patchWriter);
        patchWriter.Flush();
      }

      if (frameCountMessageStart >= 0)
      {
        // Found the correct location. Seek the stream to here and write a new FrameCount control message.
        outStream.Seek(frameCountMessageStart, SeekOrigin.Begin);
        PacketBuffer packet = new PacketBuffer();
        ControlMessage frameCountMsg = new ControlMessage();

        header = PacketHeader.Create((ushort)RoutingID.Control, (ushort)ControlMessageID.FrameCount);

        frameCountMsg.ControlFlags = 0;
        frameCountMsg.Value32 = frameCount;  // Placeholder. Frame count is currently unknown.
        frameCountMsg.Value64 = 0;
        packet.WriteHeader(header);
        frameCountMsg.Write(packet);
        packet.FinalisePacket();
        BinaryWriter patchWriter = new Tes.IO.NetworkWriter(outStream);
        packet.ExportTo(patchWriter);
        patchWriter.Flush();
      }

      if (outStream.Position != restorePos)
      {
        outStream.Seek(restorePos, SeekOrigin.Begin);
      }
    }



    private void ParseArgs(string[] args)
    {
      bool ok = args.Length != 0;
      string ipStr = null;
      int port = 0;

      ArgsOk = false;
      for (int i = 0; i < args.Length; ++i)
      {
        if (args[i] == "--help" || args[i] == "-?" || args[i] == "-h")
        {
          ShowUsage = true;
        }
        else if (args[i] == "--ip")
        {
          if (i + 1 < args.Length)
          {
            ipStr = args[++i];
          }
          else
          {
            ok = false;
          }
        }
        else if (args[i].StartsWith("-m"))
        {
          DecodeMode = ArgToMode(args[i]);
        }
        else if (args[i] == "--overwrite" || args[i] == "-w")
        {
          Overwrite = true;
        }
        else if (args[i] == "--persist" || args[i] == "-p")
        {
          Persist = true;
        }
        else if (args[i] == "--quiet" || args[i] == "-q")
        {
          Quiet = true;
          Console.WriteLine("Setting Quiet");
        }
        else if (args[i] == "--port")
        {
          if (i + 1 < args.Length)
          {
            if (!int.TryParse(args[++i], out port))
            {
              ok = false;
              Console.WriteLine("Error parsing port");
            }
          }
          else
          {
            ok = false;
          }
        }
        else if (!args[i].StartsWith("-") && string.IsNullOrEmpty(OutputPrefix))
        {
          OutputPrefix = args[i];
        }
      }

      if (ok)
      {
        if (port == 0)
        {
          port = DefaultPort;
        }

        if (string.IsNullOrEmpty(ipStr))
        {
          ipStr = DefaultIP;
        }

        if (!string.IsNullOrEmpty(ipStr) && port >= 0)
        {
          try
          {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
          }
          catch (Exception)
          {
            ok = false;
            Console.Error.WriteLine("Invalid server end point");
          }
        }
        else
        {
          ok = false;
          Console.Error.WriteLine("Missing valid server IP address and port.");
        }
      }

      if (string.IsNullOrEmpty(OutputPrefix))
      {
        OutputPrefix = DefaultPrefix;
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
    private ServerInfoMessage _serverInfo =  ServerInfoMessage.Default;
    private int _nextOutputNumber = 0;
  }
}
