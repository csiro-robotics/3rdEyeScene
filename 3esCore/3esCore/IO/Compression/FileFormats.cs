// Disable Xml comment warnings.
#pragma warning disable 1591
namespace Tes.IO.Compression
{
    interface IFileFormatWriter {
        byte[] GetHeader();
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        byte[] GetFooter();
    }

    interface IFileFormatReader {
        bool ReadHeader(InputBuffer input);
        bool ReadFooter(InputBuffer input);
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        void Validate();
    }

}
