using UnityEngine;
using System.Collections;
using UnityEditor;
using Tes;
using Tes.Server;

[CustomEditor(typeof(TesServer))]
public class ManageTesViews : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    TesServer tes = (TesServer)target;
    if (GUILayout.Button("Add Views"))
    {
      tes.AttachViews();
    }
    if (GUILayout.Button("Remove Views"))
    {
      tes.RemoveViews();
    }
  }
}
