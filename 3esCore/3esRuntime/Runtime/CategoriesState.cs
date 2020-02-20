using System;
using System.Collections.Generic;

namespace Tes.Runtime
{
  /// <summary>
  /// This class tracks the active state of all 65K categories addressable by <c>ushort</c>.
  /// </summary>
  /// <remarks>
  /// The state is shared with all handlers, but maintained by the categories handler. It is also up to the categories
  /// handler to hierarchical states.
  /// </remarks>
  public class CategoriesState
  {
    /// <summary>
    /// Check if <paramref name="category"/> is active.
    /// </summary>
    /// <param name="category">The category number to check.</param>
    /// <returns>True if active.</returns>
    public bool IsActive(ushort category)
    {
      // Resolve index and flag value.
      int index = category / 8;
      byte flag = (byte)(1 << (category % 8));
      return (_inactiveCategoryFlags[index] & flag) == 0;
    }

    /// <summary>
    /// Set the active state of <paramref name="category"/>
    /// </summary>
    /// <param name="category">The category to modify.</param>
    /// <param name="enable">True to enable, false to turn off.</param>
    public void SetActive(ushort category, bool active)
    {
      // Resolve index and flag value.
      int index = category / 8;
      byte flag = (byte)(1 << (category % 8));
      if (active)
      {
        _inactiveCategoryFlags[index] &= (byte)(~flag);
      }
      else
      {
        _inactiveCategoryFlags[index] |= flag;
      }
    }

    /// <summary>
    /// Reset the state to all categories active.
    /// </summary>
    public void Reset()
    {
      // How to meset in C#?
      for (int i = 0; i < _inactiveCategoryFlags.Length; ++i)
      {
        _inactiveCategoryFlags[i] = 0;
      }
    }

    /// <summary>
    /// A collection of bit flags indicating the active state of each category. A set bit indicates a disabled state.
    /// </summary>
    private byte[] _inactiveCategoryFlags = new byte[0xffff / 8];
  }
}