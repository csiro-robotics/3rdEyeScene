
/// <summary>
/// Interface for objects reflected in the 3rd Eye Scene.
/// </summary>
public interface TesView
{
  Tes.Shapes.Shape Shape { get; }
  UnityEngine.GameObject GameObject { get; }

  bool Dynamic { get; }
  /// <summary>
  /// Update the 3es view of the object.
  /// </summary>
  /// <returns>True if something has changed and an update message is required.</returns>
  bool UpdateView();
}
