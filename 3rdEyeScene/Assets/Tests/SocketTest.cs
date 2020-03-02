using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class SocketTest
  {

    [UnityTest]
    public IEnumerator RoundTrip()
    {
      TcpListener listener = null;
      TcpClient server = null;
      TcpClient client = null;
      byte[] buffer = new byte[1 * 1024];

      IPEndPoint addr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3350);
      listener = new TcpListener(addr);
      listener.Start();
      yield return null;

      client = new TcpClient();
      client.Connect(addr);
      yield return null;

      server = listener.AcceptTcpClient();
      yield return null;

      Debug.Assert(server != null);
      Debug.Assert(client.Connected);
      Debug.Assert(server.Connected);

      int bytesRead = 0;

      string message = "ping\n";

      StreamWriter writer = new StreamWriter(server.GetStream());
      writer.Write(message);
      writer.Flush();
      yield return null;

      Debug.Assert(client.Available > 0 && (bytesRead = client.GetStream().Read(buffer, 0, buffer.Length)) > 0);

      string incoming = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
      Debug.Log(string.Format("Client: {0}", incoming));
      Debug.Assert(incoming == message);

      yield return null;

      message = "pong\n";
      writer = new StreamWriter(client.GetStream());
      writer.Write(message);
      writer.Flush();
      yield return null;

      Debug.Assert(server.Available > 0 && (bytesRead = server.GetStream().Read(buffer, 0, buffer.Length)) > 0);

      incoming = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
      Debug.Log(string.Format("Server: {0}", incoming));
      Debug.Assert(incoming == message);

      server.Close();
      client.Close();
      yield return new WaitForSeconds(0.5f);

      Debug.Assert(!server.Connected);
      Debug.Assert(!client.Connected);

      yield return null;
    }
  }
}