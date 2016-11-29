using System.Net.Sockets;

namespace Tes.Net
{
  /// <summary>
  /// Helper functions for TCP/IP client sockets.
  /// </summary>
  public static class TcpClientUtil
  {
    /// <summary>
    /// Check if <paramref name="socket"/> is still connected.
    /// </summary>
    /// <param name="socket">The socket of interest.</param>
    /// <returns>True if still connected.</returns>
    public static bool Connected(TcpClient socket)
    {
      try
      {
        return socket != null && socket.Connected && !(socket.Client.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
      }
      catch (SocketException)
      {
      }
      return false;
    }
  }
}
