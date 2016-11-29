using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class SocketTest : MonoBehaviour
{
  
  void Start()
  {
    try
    {
      IPEndPoint addr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3350);
      _listener = new TcpListener(addr);
      _listener.Start();
      _client = new TcpClient();
      _client.Connect(addr);
    }
    catch (System.Exception e)
    {
      Debug.LogException(e);
    }
  }
  
  void Update()
  {
    try
    {
      if (_listener != null && _client != null)
      {
        if (_server == null)
        {
          _server = _listener.AcceptTcpClient();
        }
        else
        {
          int bytesRead = 0;
          if (_ping)
          {
            StreamWriter writer = new StreamWriter(_server.GetStream());
            writer.Write("ping\n");
            writer.Flush();
            while (_client.Available > 0 && (bytesRead = _client.GetStream().Read(_buffer, 0, _buffer.Length)) > 0)
            {
              _ping = false;
              string incoming = System.Text.Encoding.UTF8.GetString(_buffer, 0, bytesRead);
              Debug.Log(string.Format("Client: {0}", incoming));
            }
          }
          else
          {
            StreamWriter writer = new StreamWriter(_client.GetStream());
            writer.Write("pong\n");
            writer.Flush();
            while (_server.Available > 0 && (bytesRead = _server.GetStream().Read(_buffer, 0, _buffer.Length)) > 0)
            {
              _ping = true;
              string incoming = System.Text.Encoding.UTF8.GetString(_buffer, 0, bytesRead);
              Debug.Log(string.Format("Client: {0}", incoming));
            }
          } 
        }
      }
    }
    catch (System.Exception e)
    {
      Debug.LogException(e);
    }
  }
  
  private TcpListener _listener = null;
  private TcpClient _server = null;
  private TcpClient _client = null;
  bool _ping = true;
  byte[] _buffer = new byte[1 * 1024];
}
