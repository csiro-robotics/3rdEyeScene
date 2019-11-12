using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// Parses command line arguments into a dictionary of arguments and values.
/// </summary>
/// <remarks>
/// This class parses arguments of the forms:
///
/// <list type="bullet">
/// <item><c>--&lt;name&gt;=&lt;value&gt;</c> into the <see cref="Values"/> dictionary
///   of arg/value pairs. Leading '-' are dropped from the argument name.</item>
/// <item><c>--&lt;arg&gt;</c> into the <see cref="Opt"/> array of strings. Leading '-' are dropped from the argument name.</item>
/// <item>Other arguments are put into <see cref="Anonymous"/>.</item>
/// </list>
/// </remarks>
public class Options
{
  /// <summary>
  /// Enumeration of the possible application execution modes.
  /// </summary>
  public enum RunMode
  {
    /// <summary>
    /// Normal interactive execution mode.
    /// </summary>
    Normal,
    /// <summary>
    /// Show version number and exit.
    /// </summary>
    Version,
    /// <summary>
    /// Show usage/help.
    /// </summary>
    Help,
    /// <summary>
    /// Immediately run the specified file in playback mode.
    /// </summary>
    Play,
    /// <summary>
    /// Run the in test mode.
    /// </summary>
    Test
  };

  private static Options _current = null;
  /// <summary>
  /// Static Options instance, initialised in <see cref="Startup"/>
  /// </summary>
  public static Options Current
  {
    get
    {
      if (_current == null)
      {
        _current = new Options();
      }
      return _current;
    }
  }

  /// <summary>
  /// IP end point to automatically connect to.
  /// </summary>
  public IPEndPoint Connection = null;

  /// <summary>
  /// The desired execution mode for the application.
  /// </summary>
  public RunMode Mode = RunMode.Normal;

  /// <summary>
  /// Set to true if parsing of command line options has failed and the application should quit.
  /// </summary>
  public bool ParseFailure = false;
  /// <summary>
  ///  Persist after RunMode.Test?
  /// </summary>
  public bool Persist = false;

  /// <summary>
  /// Parsed command line arguments.
  /// </summary>
  public Dictionary<string, string> Values = new Dictionary<string, string>();
  /// <summary>
  /// Parsed command line options.
  /// </summary>
  public List<string> Opt = new List<string>();
  /// <summary>
  /// Anonymous arguments.
  /// </summary>
  public List<string> Anonymous = new List<string>();

  public bool ChangeMode(RunMode newMode)
  {
    if (Mode == RunMode.Normal)
    {
      Mode = newMode;
      Debug.Log($"Change mode {Mode}");
      return true;
    }
    return false;
  }

  /// <summary>
  /// Constructor. Populates members based on command line arguments.
  /// </summary>
  private Options()
  {
    string[] args = Environment.GetCommandLineArgs();

    string key, value;
    for (int i = 1; i < args.Length; ++i)
    {
      if (args[i].StartsWith("-"))
      {
        if (args[i].IndexOf("=") >= 0)
        {
          Split(args[i], out key, out value);
          Values.Add(key, value);
        }
        else
        {
          Split(args[i], out key, out value);
          Opt.Add(key);
        }
      }
      else
      {
        Anonymous.Add(args[i]);
      }
    }

    Parse();
  }

  // Note: Console.Write doesn't work.
  public static void ShowUsage()
  {
    Debug.LogWarning("Command line arguments:");
    Debug.LogWarning("--help,-h\n\tShow this message.");
    Debug.LogWarning("--connect=<end-point>\n\tOn start, try connect to the specified IPv4 end point.");
    // Debug.LogWarning("--4\n\tInterpret '--connect' end point as an IPv4 address.");
    // Debug.LogWarning("--6\n\tInterpret '--connect' end point as an IPv6 address.");
    Debug.LogWarning("--test\n\tRun in unit test mode.");
  }

  void Parse()
  {
    if (Opt.Contains("help") || Opt.Contains("h"))
    {
      ChangeMode(RunMode.Help);
    }
    if (Opt.Contains("version"))
    {
      ChangeMode(RunMode.Version);
    }
    if (Values.ContainsKey("test") || Opt.Contains("test"))
    {
      ChangeMode(RunMode.Test);
    }
    if (Values.ContainsKey("play"))
    {
      ChangeMode(RunMode.Play);
    }

    Persist = Opt.Contains("persist");

    // Check for auto connect on start.
    if (Values.ContainsKey("connect"))
    {
      // Split the IPv4 string.
      string[] parts = Values["connect"].Split(':');
      IPAddress address = null;
      int port = 0;
      string additionalError = "";
      bool connectionStringOk = true;

      if (parts.Length > 2)
      {
        additionalError = "unexpected number of components";
        connectionStringOk = false;
      }

      if (parts.Length >= 1)
      {
        try
        {
          address = IPAddress.Parse(parts[0]);
        }
        catch (Exception)
        {
          additionalError = "invalid address part";
          connectionStringOk = false;
        }
      }
      if (parts.Length >= 2)
      {
        if (!int.TryParse(parts[1], out port))
        {
          additionalError = "invalid port";
          connectionStringOk = false;
        }
      }
      else
      {
        port = Tes.Server.ServerSettings.Default.ListenPort;
      }

      if (connectionStringOk)
      {
        try
        {
          IPEndPoint endPoint = new IPEndPoint(address, port);
          Connection = endPoint;
        }
        catch (Exception)
        {
          additionalError = "invalid IP end point";
          connectionStringOk = false;
        }
      }

      if (!connectionStringOk)
      {
        Console.Error.WriteLine($"Invalid IPv4 string: {Values["connect"]}");
        if (!string.IsNullOrEmpty(additionalError))
        {
          Console.Error.WriteLine(additionalError);
        }
        ParseFailure = true;
      }
    }
  }

  /// <summary>
  /// Splits a command line argument into it's key/value pair.
  /// </summary>
  /// <param name="argIn">The input to split.</param>
  /// <param name="key">The key part.</param>
  /// <param name="value">The value part.</param>
  public static void Split(string argIn, out string key, out string value)
  {
    while (argIn.StartsWith("-"))
    {
      argIn = argIn.Substring(1);
    }

    int valueStart = argIn.IndexOf("=");
    if (valueStart < 0)
    {
      key = argIn;
      value = "";
      return;
    }

    key = argIn.Substring(0, valueStart);
    value = argIn.Substring(valueStart + 1);
  }
}
