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
        settings.Flags = ServerFlag.Collate;
        return settings;
      }
    }
  }
}
