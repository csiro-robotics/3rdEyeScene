using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tes;
using Tes.Runtime;
using UnityEngine;

namespace Tes.Main
{
  public class MessageHandlerLibrary
  {
    /// <summary>
    /// Converts a simple wildcard string into an equivalent regular expression string.
    /// </summary>
    /// <param name="wildcardString">The wildcard string to convert.</param>
    /// <returns>A RegEx compatible equivalent of the string.</returns>
    /// <remarks>
    /// This method replaces '*' in the input string with the RegEx equivalent '.*'. It also ensures that any regex
    /// reserved characters are correctly escaped.
    /// </remarks>
    public static string WildcardToRegExString(string wildcardString)
    {
      string specialCharacters = "?#+[]\\.{}^$()";
      StringBuilder regexString = new StringBuilder();
      foreach (char ch in wildcardString)
      {
        if (ch == '*')
        {
          regexString.Append(".");
        }
        else if (specialCharacters.Contains(ch.ToString()))
        {
          regexString.Append("\\");
        }
        regexString.Append(ch);
      }

      return regexString.ToString();
    }

    /// <summary>
    /// Load <see cref="MessageHandler"/> instances from assemblies in the <paramref name="pluginPath"/>
    /// directory.
    /// </summary>
    /// <returns><c>true</c> on success.</returns>
    /// <param name="pluginPath">The path from which to load plugin assemblies.</param>
    /// <param name="plugins">The plugin manager used to load and keep references to the relevant assemblies.</param>
    /// <param name="excludeFiles">Array of DLL names to exclude. Uses simple wildcat matching with case insensitive
    ///   compare. File name is used without its path.</param>
    public bool LoadPlugins(string pluginPath, PluginManager plugins, string[] excludeFiles, object[] args)
    {
      if (!Directory.Exists(pluginPath))
      {
        return false;
      }

      Regex[] excludeRex = new Regex[excludeFiles.Length];
      for(int i = 0; i < excludeFiles.Length; ++i)
      {
        if (!string.IsNullOrEmpty(excludeFiles[i]))
        {
          string rexStr = WildcardToRegExString(excludeFiles[i]);
          excludeRex[i] = new Regex(rexStr, RegexOptions.IgnoreCase);
        }
        else
        {
          excludeRex[i] = null;
        }
      }

      DirectoryInfo dir = new DirectoryInfo(pluginPath);
      List<MessageHandler> handlers = new List<MessageHandler>();
      foreach(FileInfo file in dir.GetFiles("*.dll"))
      {
        bool onExcludeList = false;
        foreach (Regex exclude in excludeRex)
        {
          if (exclude != null)
          {
            if (exclude.IsMatch(file.Name))
            {
              // Skip excluded file.
              onExcludeList = true;
              break;
            }
          }
        }

        if (onExcludeList)
        {
          continue;
        }

        try
        {
          Debug.Log($"Trying plugin load: {file.Name}");
          plugins.LoadFrom<MessageHandler>(handlers, typeof(MessageHandler), file.FullName, args);
        }
        catch (BadImageFormatException)
        {
          Debug.Log($"Skipping DLL {file.Name}: not a .NET assembly");
        }
        catch (Exception e)
        {
          Debug.LogError($"Failed to load DLL {file.Name}");
          Debug.LogException(e);
        }

        if (handlers.Count > 0)
        {
          Debug.Log($"Loaded {file.Name}: {handlers.Count} message handlers added.");
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
