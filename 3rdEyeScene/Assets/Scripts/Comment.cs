using UnityEngine;

/// <summary>
/// A script designed to simply attach a comment to an object.
/// </summary>
/// <remarks>
/// Need to prevent export to the built scene somehow. That is, make this an editor only
/// script, but allow attachment to any object.
/// 
/// Better yet, browse the asset store for an existing solution to the comment problem.
/// </remarks>
class Comment : MonoBehaviour
{
  public string Note = "";
}
