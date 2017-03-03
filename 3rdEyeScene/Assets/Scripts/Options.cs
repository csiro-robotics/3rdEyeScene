using System;
using System.Collections.Generic;
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

  /// <summary>
  /// Constructor. Populates members based on command line arguments.
  /// </summary>
  public Options()
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
