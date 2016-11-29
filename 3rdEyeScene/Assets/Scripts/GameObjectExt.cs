using UnityEngine;

public static class GameObjectExt
{
  public static GameObject FindChild(this GameObject obj, string name)
  {
    // Bredth first.
    for (int i = 0; i < obj.transform.childCount; ++i)
    {
      Transform child = obj.transform.GetChild(i);
      if (child.name == name)
      {
        return child.gameObject;
      }
    }

    // Recurse
    for (int i = 0; i < obj.transform.childCount; ++i)
    {
      Transform child = obj.transform.GetChild(i);
      GameObject descendent = FindChild(child.gameObject, name);
      if (descendent != null)
      {
        return descendent;
      }
    }
    
    return null;
  }
}
