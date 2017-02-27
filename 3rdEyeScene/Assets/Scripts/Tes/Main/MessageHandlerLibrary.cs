using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using Tes;
using Tes.Runtime;

namespace Tes.Main
{
  public class MessageHandlerLibrary
  {
    /// <summary>
    /// Load <see cref="MessageHandler"/> instances from assemblies in the <paramref name="pluginPath"/>
    /// directory.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="pluginPath">The path from which to load plugin assemblies.</param>
    /// <param name="plugins">The plugin manager used to load and keep references to the relevant assemblies.</param>
    public bool LoadPlugins(string pluginPath, PluginManager plugins, string exclude, object[] args)
    {
      if (!Directory.Exists(pluginPath))
      {
        return false;
      }

      DirectoryInfo dir = new DirectoryInfo(pluginPath);
      List<MessageHandler> handlers = new List<MessageHandler>();
      foreach (FileInfo file in dir.GetFiles("*.dll"))
      {
        if (!string.IsNullOrEmpty(exclude) && string.Compare(file.Name, exclude, true) == 0)
        {
          // Skip excluded file.
          continue;
        }

        plugins.LoadFrom<MessageHandler>(handlers, typeof(MessageHandler), file.FullName, args);
        if (handlers.Count > 0)
        {
          int registered = 0;
          foreach (MessageHandler handler in handlers)
          {
            if (Register(handler))
            {
              ++registered;
            }
            else
            {
              // TODO: log an error as routing ID is already used.
            }
          }
        }
        handlers.Clear();
      }

      return true;
    }
    
    public bool Register(MessageHandler handler)
    {
      if (handler == null)
      {
        return false;
      }

      if (_handlers.ContainsKey(handler.RoutingID))
      {
        return false;
      }

      _handlers.Add(handler.RoutingID, handler);
      return true;
    }

    public MessageHandler HandlerFor(int routingID)
    {
      if (_handlers.ContainsKey(routingID))
      {
        return _handlers[routingID];
      }
      return null;
    }

    public IEnumerable<MessageHandler> Handlers
    {
      get
      {
        foreach (var item in _handlers)
        {
          yield return item.Value;
        }
      }
    }

    private Dictionary<int, MessageHandler> _handlers = new Dictionary<int, MessageHandler>();
  }
}
