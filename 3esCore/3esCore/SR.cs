// Copyright (c) Hitcents
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Disable Xml comment warnings.
#pragma warning disable 1591
namespace Tes {
    /// <summary>
    /// NOTE: this is a hacked in replacement for the SR class
    ///     Unity games don't care about localized exception messages, so we just hacked these in the best we could
    /// </summary>
    internal class SR
    {
        public const string ArgumentOutOfRange_Enum = "Argument out of range";
        public const string CDCorrupt = "CD corrupt";
        public const string CannotBeEmpty = "Cannot be empty";
        public const string CannotReadFromDeflateStream = "Cannot read from deflate stream";
        public const string CannotWriteToDeflateStream = "Cannot write to deflate stream";
        public const string CentralDirectoryInvalid = "Central directory invalid: {0}";
        public const string CorruptedGZipHeader = "Corrupted gzip header";
        public const string CreateInReadMode = "Create in creat mode";
        public const string CreateModeCapabilities = "Create mode capabilities";
        public const string CreateModeCreateEntryWhileOpen = "Create mode create entry while open";
        public const string EOCDNotFound = "EOCD not found";
        public const string EntriesInCreateMode = "Entries in create mode";
        public const string EntryNameEncodingNotSupported = "Entry name encoding not supported";
        public const string FieldTooBigNumEntries = "Field too big num entries";
        public const string FieldTooBigOffsetToCD = "Field too big offset to CD";
        public const string FieldTooBigOffsetToZip64EOCD = "Field too big offset to zip 64 EOCD";
        public const string GenericInvalidData = "Invalid data";
        public const string InvalidArgumentOffsetCount = "Invalid argument offset count";
        public const string InvalidBeginCall = "Invalid begin call";
        public const string InvalidBlockLength = "Invalid block length";
        public const string InvalidCRC = "Invalid CRC";
        public const string InvalidEndCall = "Invalid end call";
        public const string InvalidHuffmanData = "Invalid Huffman data";
        public const string InvalidStreamSize = "Invalid stream size";
        public const string NotReadableStream = "Not a readable stream";
        public const string NotSupported = "Not supported";
        public const string NotWriteableStream = "Not a writeable stream";
        public const string NumEntriesWrong = "Number of entries wrong";
        public const string ObjectDisposed_StreamClosed = "Object disposed";
        public const string ReadModeCapabilities = "Read mode capabilities";
        public const string SplitSpanned = "Split spanned";
        public const string UnknownBlockType = "Unknown block type";
        public const string UnknownCompressionMode = "Unknown compression mode";
        public const string UnknownState = "Unknown state";
        public const string UpdateModeCapabilities = "Update mode capabilities";
        public const string Zip64EOCDNotWhereExpected = "Zip 64 EOCD not where expected";
        public const string EntryNamesTooLong = "Entry names too long";
        public const string LengthAfterWrite = "Length after write";
        public const string DeleteOpenEntry = "Delete open entry";
        public const string DeleteOnlyInUpdate = "Delete only in update";
        public const string LocalFileHeaderCorrupt = "Local file header corrupt";
        public const string CreateModeWriteOnceAndOneEntryAtATime = "Create mode write once and one entry at a time";
        public const string UpdateModeOneStream = "Update mode one stream";
        public const string UnsupportedCompressionMethod = "Unsupported compression method : {0}";
        public const string UnsupportedCompression = "Unsupported compression";
        public const string EntryTooLarge = "Entry too large";
        public const string DeletedEntry = "Deleted entry";
        public const string SeekingNotSupported = "Seeking not supported";
        public const string HiddenStreamName = "Hidden stream name";
        public const string ReadingNotSupported = "Reading not supported";
        public const string SetLengthRequiresSeekingAndWriting = "Set length requires seeking and writing";
        public const string ArgumentNeedNonNegative = "Argument need non negative";
        public const string OffsetLengthInvalid = "Offset length invalid";
        public const string ReadOnlyArchive = "Read only archive";
        public const string FrozenAfterWrite = "Frozen after write";
        public const string DateTimeOutOfRange = "Date/time out of range";
        public const string FieldTooBigUncompressedSize = "Field too big: uncompressed size";
        public const string FieldTooBigCompressedSize = "Field too big: compressed size";
        public const string FieldTooBigLocalHeaderOffset = "Field too big: local header offset";
        public const string FieldTooBigStartDiskNumber = "Field too big: start disk number";
        public const string WritingNotSupported = "Writing not supported";
        public const string UnexpectedEndOfStream = "Unexpected end of stream";
        public const string NotSupported_UnreadableStream = "Not supported/unreadable stream";
        public const string NotSupported_UnwritableStream = "Not supported/unwritable stream";
        public const string ZLibErrorDLLLoadError = "ZLib error: DLL load error";
        public const string ZLibErrorNotEnoughMemory = "ZLib error: not enough memory";
        public const string ZLibErrorVersionMismatch = "ZLib error: version mismatch";
        public const string ZLibErrorIncorrectInitParameters = "ZLib error: incorrect init parameters";
        public const string ZLibErrorUnexpected = "ZLib error: unexpected";
        public const string ZLibErrorInconsistentStream = "ZLib error: inconsistent stream";
        public const string ArgumentException_BufferNotFromPool = "Argument exception: buffer not from pool";

        private SR()
        {
        }

        internal static string GetString(string p)
        {
            //HACK: just return the string passed in, not doing localization
            return p;
        }

        internal static string Format(string a, params object[] args)
        {
            // HACK: No localisation
            return string.Format(a, args);
        }
    }
}
