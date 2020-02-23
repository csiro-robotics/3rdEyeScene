using System;

namespace Tes.Runtime
{
  /// <summary>
  /// Error codes for 3ES.
  /// </summary>
  public enum ErrorCode : int
  {
    /// <summary>
    /// No error.
    /// </summary>
    OK = 0,

    /// <summary>
    /// Unspecified error.
    /// </summary>
    Unknown,

    /// <summary>
    /// Feature is unsupported or not implemented on the current platform.
    /// </summary>
    UnsupportedFeature,

    // General codes.

    /// <summary>
    /// The CRC comparison failed. No valid given (corrupted content).
    /// </summary>
    CrcFailure,
    /// <summary>
    /// The message routing ID did not match a known handler.
    /// The value is set to the unknown routing id.
    /// </summary>
    UnknownMessageHandler,
    /// <summary>
    /// A valid message code is present, but it is a null code.
    /// </summary>
    NullMessageCode,
    /// <summary>
    /// The message content ID is invalid. The value is set to the message ID.
    /// </summary>
    InvalidMessageID,
    /// <summary>
    /// The message content could not be successfully decoded.
    /// The value is set to the message content ID.
    /// </summary>
    MalformedMessage,
    /// <summary>
    /// Message object ID does not refere to a known and valid object.
    /// The value is set to the invalid ID.
    /// </summary>
    InvalidObjectID,
    /// <summary>
    /// Some array indexing is out of bounds.
    /// The value is set to the message content ID.
    /// </summary>
    IndexingOutOfRange,
    /// <summary>
    /// Message seems OK, but contains invalid data values.
    /// The value is set to the message content ID.
    /// </summary>
    InvalidContent,

    /// <summary>
    /// Not enough resources.
    /// </summary>
    InsufficientResources,

    // Serialisation codes

    /// <summary>
    /// The specified file name is invalid for the current operation. Includes permission failures.
    /// </summary>
    InvalidFileName,

    /// <summary>
    /// Failed to serialise; read or write.
    /// </summary>
    SerialisationFailure,

    // General shape codes.

    /// <summary>
    /// A shape with the given ID already exists.
    /// The value is set to the duplicate ID.
    /// </summary>
    DuplicateShape,

    // Mesh handler codes.

    /// <summary>
    /// Attempting to modify a mesh after it has been finalised.
    /// The value is set to the mesh ID.
    /// </summary>
    MeshAlreadyFinalised,

    /// <summary>
    /// The draw type of the mesh is invalid.
    /// The value is set to the mesh ID.
    /// </summary>
    MeshUnknownDrawType,

    /// <summary>
    /// User error codes start here.
    /// </summary>
    User = 1000
  }
}

