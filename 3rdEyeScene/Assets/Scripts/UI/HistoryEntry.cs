using UnityEngine;
using System.Net;

namespace UI
{
  public class HistoryEntry : MonoBehaviour
  {
    public ConnectionPanel Connections { get; set; }
    
    public string Host
    {
      get
      {
        var addressEntry = GetInput("Host");
        if (addressEntry != null)
        {
          return addressEntry.text;
        }
        return "";
      }
      
      set
      {
        var addressEntry = GetInput("Host");
        if (addressEntry != null)
        {
          addressEntry.text = value;
        }
      }
    }
    
    public int Port
    {
      get
      {
        var portEntry = GetInput("Port");
        int port = 0;
        if (portEntry != null)
        {
          int.TryParse(portEntry.text, out port);
        }
        return port;
      }
      
      set
      {
        var portEntry = GetInput("Port");
        if (portEntry != null)
        {
          portEntry.text = value.ToString();
        }
      }
    }

    public IPEndPoint Connection
    {
      get
      {
        try
        {
          return new IPEndPoint(IPAddress.Parse(Host), Port);
        }
        catch (System.Exception)
        {
        }
        return null;
      }
      
      set
      {
        Host = value.Address.ToString();
        Port = value.Port;
      }
    }
    
    private UnityEngine.UI.Text GetInput(string objectName)
    {
      foreach (var text in gameObject.GetComponentsInChildren<UnityEngine.UI.Text>())
      {
        if (text.name == objectName)
        {
          return text;
        }
      }
      return null;
    }
    
    public void OnClick()
    {
      if (Connections != null)
      {
        IPEndPoint endPoint = Connection;
        if (endPoint != null)
        {
          Connections.Connect(endPoint);
        }
      }
    }
  }
}