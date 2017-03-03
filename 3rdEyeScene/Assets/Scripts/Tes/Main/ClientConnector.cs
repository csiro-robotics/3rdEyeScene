using System.Net;
using System.Net.Sockets;

namespace Tes.Main
{
  /// <summary>
  /// A utility class which manages an asynchronous <code>TcpClient</code> connection.
  /// </summary>
  /// <remarks>
  /// Usage:
  /// <list type="bullet">
  /// <item>Instantiate the connector.</item>
  /// <item>Poll <code>Connecting</code> status.</item>
  /// <item>Check <code>Connected</code> once <code>Connecting</code> is <code>false</code></item>
  /// <item>If <code>Connected</code>, call <code>Accept()</code> and take ownership of the socket.</item>
  /// <item>Otherwise, call abort and dispose of this object.</item>
  /// </list>
  /// </remarks>
  public class ClientConnector
  {
    public ClientConnector(IPEndPoint endPoint)
    {
      _socket = new TcpClient();
      Connecting = true;
      _socket.BeginConnect(endPoint.Address, endPoint.Port,
        delegate (System.IAsyncResult res)
        {
          TcpClient socket = res.AsyncState as TcpClient;
          if (socket != null)
          { 
            if (_socket.Connected)
            {
              socket.EndConnect(res);
              Connected = true;
            }
            else
            {
              Connected = false;
            }
          }
          Connecting = false;
        }
        , _socket);
    }

    ~ClientConnector()
    {
      Abort();
    }

    public bool Connecting { get; private set; }
    public bool Connected { get; private set; }

    public TcpClient Accept()
    {
      if (Connected)
      {
        TcpClient client = _socket;
        _socket = null;
        return client;
      }

      return null;
    }

    public void Abort()
    {
      if (_socket != null)
      {
        _socket.Close();
        _socket = null;
      }
    }

    private TcpClient _socket;
  }
}