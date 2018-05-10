using System;

namespace Tes.Server
{
  /// <summary>
  /// Defines the initialisation settings for <see cref="IServer"/>.
  /// </summary>
  [Serializable]
  public struct ServerSettings
  {
    /// <summary>
    /// The port to listen on.
    /// </summary>
    public ushort ListenPort;
    /// <summary>
    /// Number of additional ports we can try look beyond <see cref="ListenPort"/>.
    /// </summary>
    public ushort PortRange;
    /// <summary>
    /// The <see cref="ServerFlag"/> flags.
    /// </summary>
    public ServerFlag Flags;

    /// <summary>
    /// The default server settings.
    /// </summary>
    public static ServerSettings Default
    {
      get
      {
        ServerSettings settings = new ServerSettings();
        settings.ListenPort = 33500;
        settings.PortRange = 0;
        settings.Flags = ServerFlag.Collate | ServerFlag.NakedFrameMessage;
        return settings;
      }
    }
  }
}
