using UnityEngine;
using System;

namespace Dialogs
{
  [Serializable]
  public struct IconInfo
  {
    public Sprite Icon;
    public FileItemType Type;
    public string SubType;
  }
}
