using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using Tes.Main;

namespace UI
{
  public class ConnectionPanel : MonoBehaviour
  {
    public static string LastConnectionHostKey { get { return "last-connection-host"; } }
    public static string LastConnectionPortKey { get { return "last-connection-port"; } }
    public static string ConnectionHostHistoryKey { get { return "history-connection-host"; } }
    public static string ConnectionPortHistoryKey { get { return "history-connection-port"; } }
    public static string HistorySizeKey { get { return "connection-history-size"; } }

    public HistoryEntry HistoryViewEntryUI;
    public ScrollRect HistoryContentUI;
    private RectTransform _historyContentTransform = null;
    public RectTransform HistoryContentTransform { get { return _historyContentTransform; } }
    public TesComponent Controller;
    [SerializeField]
    private InputField _hostEntry = null;
    [SerializeField]
    private InputField _portEntry = null;
    [SerializeField]
    private Button _connectButton = null;
    [SerializeField]
    private Button _disconnectButton = null;

    [SerializeField]
    private int _historySize = 64;
    public int HistorySize
    {
      get { return _historySize; }
      set
      {
        _historySize = value;
        PlayerPrefs.SetInt(HistorySizeKey, _historySize);
      }
    }

    public bool AutoReconnect
    {
      get
      {
        foreach (UnityEngine.UI.Toggle toggle in GetComponentsInChildren<UnityEngine.UI.Toggle>())
        {
          if (toggle.name == "AutoReconnectToggle")
          {
            return toggle.isOn;
          }
        }
        return false;
      }

      set
      {
        foreach (UnityEngine.UI.Toggle toggle in GetComponentsInChildren<UnityEngine.UI.Toggle>())
        {
          if (toggle.name == "AutoReconnectToggle")
          {
            toggle.isOn = value;
            return;
          }
        }
      }
    }

    public string CurrentHost
    {
      get
      {
        string host = string.Empty;
        foreach (UnityEngine.UI.InputField input in GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
          if (input.name == "HostInput")
          {
            host = input.text;
            break;
          }
        }

        if (host.Length > 0)
        {
          return host;
        }
        return "127.0.0.1";
      }

      set
      {
        foreach (UnityEngine.UI.InputField input in GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
          if (input.name == "HostInput")
          {
            input.text = value;
            break;
          }
        }
      }
    }

    public int CurrentPort
    {
      get
      {
        int port = 0;
        foreach (UnityEngine.UI.InputField input in GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
          if (input.name == "PortInput")
          {
            int.TryParse(input.text, out port);
            break;
          }
        }

        if (port != 0)
        {
          return port;
        }
        return Tes.Server.ServerSettings.Default.ListenPort;
      }

      set
      {
        foreach (UnityEngine.UI.InputField input in GetComponentsInChildren<UnityEngine.UI.InputField>())
        {
          if (input.name == "PortInput")
          {
            input.text = value.ToString();
            break;
          }
        }
      }
    }

    public IPEndPoint LastConnection
    {
      get
      {
        string host = PlayerPrefs.GetString(LastConnectionHostKey);
        int port = PlayerPrefs.GetInt(LastConnectionPortKey);
        if (host != null && port > 0)
        {
          return new IPEndPoint(IPAddress.Parse(host), port);
        }
        return new IPEndPoint(IPAddress.Loopback, Tes.Server.ServerSettings.Default.ListenPort);
      }

      set
      {
        IPEndPoint previousLast = LastConnection;
        if (value.Equals(previousLast))
        {
          // No change.
          return;
        }
        PlayerPrefs.SetString(LastConnectionHostKey, value.Address.ToString());
        PlayerPrefs.SetInt(LastConnectionPortKey, value.Port);
        if (previousLast != null)
        {
          // Add to the history.
          Add(previousLast);
        }
      }
    }

    public IEnumerable<IPEndPoint> History
    {
      get
      {
        string[] hosts = PlayerPrefsX.GetStringArray(ConnectionHostHistoryKey);
        int[] ports = PlayerPrefsX.GetIntArray(ConnectionPortHistoryKey);
        int limit = System.Math.Min(hosts.Length, ports.Length);
        for (int i = 0; i < limit; ++i)
        {
          yield return new IPEndPoint(IPAddress.Parse(hosts[i]), ports[i]);
        }
      }
    }

    public void Add(IPEndPoint endPoint)
    {
      if (endPoint == null)
      {
        return;
      }

      List<IPEndPoint> historyList = new List<IPEndPoint>();
      IEnumerator<IPEndPoint> iter = History.GetEnumerator();
      while (iter.MoveNext())
      {
        if (!endPoint.Equals(iter.Current))
        {
          historyList.Add(iter.Current);
        }
      }

      historyList.Insert(0, endPoint);
      // Limit the history size.
      if (historyList.Count > _historySize)
      {
        historyList.RemoveAt(historyList.Count - 1);
      }

      // Now save the updated history.
      string[] hosts = new string[historyList.Count];
      int[] ports = new int[historyList.Count];
      int i = 0;
      foreach (IPEndPoint addr in historyList)
      {
        hosts[i] = addr.Address.ToString();
        ports[i] = addr.Port;
        ++i;
      }

      PlayerPrefsX.SetStringArray(ConnectionHostHistoryKey, hosts);
      PlayerPrefsX.SetIntArray(ConnectionPortHistoryKey, ports);

      // Update the scroll view to match.
      UpdateView();
    }

    public void Connect()
    {
      try
      {
        IPEndPoint connection = new IPEndPoint(IPAddress.Parse(CurrentHost), CurrentPort);
        Connect(connection);
      }
      catch (System.Exception e)
      {
        Tes.Logging.Log.Exception(e);
        // Close();
      }
    }

    public void Connect(IPEndPoint endPoint)
    {
      Connect(endPoint, AutoReconnect);
    }

    public void Connect(IPEndPoint endPoint, bool autoReconnect)
    {
      LastConnection = endPoint;
      Add(endPoint);
      if (Controller != null)
      {
        Controller.Connect(endPoint, autoReconnect);
      }

      Close();
    }

    public void Disconnect()
    {
      if (Controller != null)
      {
        Controller.Disconnect();
      }
      Close();
    }

    /// <summary>
    /// Clear the history view and cached history.
    /// </summary>
    public void ClearHistory()
    {
      // Clear cached history.
      PlayerPrefsX.SetStringArray(ConnectionHostHistoryKey, new string[0]);
      PlayerPrefsX.SetIntArray(ConnectionPortHistoryKey, new int[0]);
      // Clear UI.
      if (HistoryContentUI != null)
      {
        // Remove in reverse order to guarantee iteration termination.
        for (int i = HistoryContentUI.content.childCount - 1; i >= 0; --i)
        {
          GameObject entry = HistoryContentUI.content.GetChild(i).gameObject;
          entry.transform.SetParent(null, false);
          GameObject.Destroy(entry.gameObject);
        }
      }
    }

    /// <summary>
    /// Respond to mode changes in the router and update the UI accordingly.
    /// </summary>
    /// <param name="mode">The new router mode to reflect.</param>
    public void OnControllerModeChange(RouterMode mode)
    {
      if (mode == RouterMode.Connected || mode == RouterMode.Connecting)
      {
        _connectButton.gameObject.SetActive(false);
        _disconnectButton.gameObject.SetActive(true);
        _hostEntry.enabled = false;
        _portEntry.enabled = false;
        // Reflect the current connection in the host/port items.
        if (Controller != null)
        {
          IPEndPoint connection = Controller.Connection;
          if (connection != null)
          {
            _hostEntry.text = connection.Address.ToString();
            _portEntry.text = connection.Port.ToString();
          }
        }
      }
      else
      {
        _connectButton.gameObject.SetActive(true);
        _disconnectButton.gameObject.SetActive(false);
        _hostEntry.enabled = true;
        _portEntry.enabled = true;
      }
    }

    void Close()
    {
      ToolsManager tools = transform.parent.GetComponent<ToolsManager>();
      if (tools != null)
      {
        tools.SetActivePanel(null);
      }
    }

    void Start()
    {
      _historySize = PlayerPrefs.GetInt(HistorySizeKey, _historySize);
      _historyContentTransform = (HistoryContentUI != null) ? HistoryContentUI.content.GetComponent<RectTransform>() : null;
      UpdateView();
    }

    private void UpdateView()
    {
      if (HistoryViewEntryUI == null || HistoryContentTransform == null)
      {
        return;
      }

      // Update existing UI objects to match the history order.
      // Add new items when we run out of existing items.
      // Update the layout at the end if required.
      int nextViewChildIndex = (HistoryContentUI.content.childCount > 0) ? 0 : -1;
      bool updateLayout = false;
      foreach (IPEndPoint connection in History)
      {
        GameObject entryObj = null;
        HistoryEntry entry = null;
        RectTransform entryTransform = null;
        // Try get an existing view entry for this item.
        if (nextViewChildIndex >= 0)
        {
          while (entryObj == null && nextViewChildIndex < HistoryContentUI.content.childCount)
          {
            entryObj = HistoryContentUI.content.GetChild(nextViewChildIndex).gameObject;
            entry = entryObj.GetComponent<HistoryEntry>();
            entryTransform = entryObj.GetComponent<RectTransform>();
            if (entry == null || entryTransform == null)
            {
              // Invalid/unexpected UI object. Remove and delete it.
              entryObj.transform.parent = null;
              GameObject.Destroy(entryObj);
            }
            else
            {
              ++nextViewChildIndex;
            }
          }

          if (nextViewChildIndex >= HistoryContentUI.content.childCount)
          {
            nextViewChildIndex = -1;
          }
        }
        else
        {
          updateLayout = true;
          entryObj = GameObject.Instantiate(HistoryViewEntryUI.gameObject);
          entry = entryObj.GetComponent<HistoryEntry>();
          entryTransform = entryObj.GetComponent<RectTransform>();
        }

        if (entryObj == null || entry == null || entryTransform == null)
        {
          if (entryObj) { GameObject.Destroy(entryObj); }
          continue;
        }

        entry.Connection = connection;
        entry.Connections = this;
        entryTransform.SetParent(HistoryContentTransform, false);
      }

      if (updateLayout)
      {
        HistoryContentUI.LayoutContentV();
      }
    }
  }
}
