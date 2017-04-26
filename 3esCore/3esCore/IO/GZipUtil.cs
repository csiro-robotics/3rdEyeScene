using System.IO;

namespace Tes.IO
{
	/// <summary>
	/// GZip utility methods.
	/// </summary>
	public static class GZipUtil
	{
		/// <summary>
		/// First GZip marker byte.
		/// </summary>
		public const byte GZipID1 = 0x1F;
		/// <summary>
		/// Second GZip marker byte.
		/// </summary>
		public const byte GZipID2 = 0x8B;

		/// <summary>
		/// Check whether <paramref name="stream"/> contains a GZip header.
		/// This is not the same as checking for the <see cref="T:GZipStream"/> class.
		/// </summary>
		/// <param name="stream">The stream to check. Must support relative seeking.</param>
		/// <returns>True if a GZip header is found at the current position of <paramref name="stream"/>.</returns>
		public static bool IsGZipStream(Stream stream)
		{
			// Look for the first marker byte.
			int markerByte = stream.ReadByte();
			if (markerByte == -1)
			{
				// End of stream reached.
				return false;
			}
			if (markerByte != GZipID1)
			{
				// No match with the first marker byte. Rewind the stream.
				Seek(stream, -1);
				return false;
			}
			// Look for the second marker byte.
			markerByte = stream.ReadByte();
			if (markerByte == -1)
			{
				// End of stream reached. Rewind the first byte.
				Seek(stream, -1);
				return false;
			}
			// Rewind the stream to before the marker regardless of success.
			Seek(stream, -2);
			if (markerByte != GZipID2)
			{
				// No match with the second marker byte. Rewind the stream.
				return false;
			}
			// Matched both marker bytes.
			return true;
		}

		/// <summary>
		/// A quick helper to seek and flush <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">The stream to seek and flush.</param>
		/// <param name="offset">The seek offset, from the current position</param>
		private static void Seek(Stream stream, int offset)
		{
			stream.Seek(offset, SeekOrigin.Current);
			stream.Flush();
		}
	}
}
