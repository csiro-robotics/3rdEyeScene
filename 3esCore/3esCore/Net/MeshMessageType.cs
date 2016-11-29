using System;
using System.Collections.Generic;
using System.Text;

namespace Tes.Net
{
  /// <summary>
  /// Defines message types for transfering mesh data (vertices, etc).
  /// </summary>
  public enum MeshMessageType
  {
    /// <summary>
    /// Invalid/null message.
    /// </summary>
    Invalid,
    /// <summary>
    /// Destroy an existing mesh.
    /// </summary>
    Destroy,
    /// <summary>
    /// Create a new, empty mesh. Provides initial data.
    /// </summary>
    Create,
    /// <summary>
    /// Incoming vertex data (part).
    /// </summary>
    Vertex,
    /// <summary>
    /// Incoming index data (part).
    /// </summary>
    Index,
    /// <summary>
    /// Incoming vertex colour data (part).
    /// </summary>
    VertexColour,
    /// <summary>
    /// Incoming vertex normal data (part).
    /// </summary>
    Normal,
    /// <summary>
    /// Incoming vertex UV data (part).
    /// </summary>
    UV,
    /// <summary>
    /// Define the material for this mesh. Future extension: NYI.
    /// </summary>
    SetMaterial,
    /// <summary>
    /// Redefine the core aspects of the mesh. This invalidates the mesh
    /// requiring re-finalisation, but allows the creation parameters to
    /// be redefined. Component messages (vertex, index, colour, etc) can
    /// also be changed after this message, but before a second Finalise.
    /// </summary>
    Redefine,
    /// <summary>
    /// Finalise and build the mesh
    /// </summary>
    Finalise
  }
}
