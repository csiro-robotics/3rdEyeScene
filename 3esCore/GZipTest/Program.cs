using System;
using System.IO;
using System.Diagnostics;
using Tes.IO.Compression;

namespace Tes
{
  class Program
  {
    static void Usage()
    {
			Console.WriteLine("Usage:");
			Console.WriteLine("GZipTest c|d <file>");
			Console.WriteLine("Use c to compress <file>, d to decompress a GZip <file>");
    }


    static void Main(string[] args)
    {
      if (args.Length < 2)
      {
        Console.WriteLine("Missing argument.");
        Usage();
        return;
      }
      Stopwatch timer = new Stopwatch();

      timer.Start();

      string command = args[0];
      string fileName = args[1];
      string outFileName = (args.Length > 2) ? args[2] : null;
      byte[] buffer = new byte[64 * 1024];
      int bytesRead = 0;
      if (string.Compare(command, "c") == 0)
      {
        if (outFileName == null)
        {
          outFileName = string.Format("{0}.gz", fileName);
        }

        using (FileStream readin = new FileStream(fileName, FileMode.Open))
        {
          using (GZipStream zip = new GZipStream(new FileStream(outFileName, FileMode.Create), CompressionMode.Compress))
          {
            while ((bytesRead = readin.Read(buffer, 0, buffer.Length)) > 0)
            {
              zip.Write(buffer, 0, bytesRead);
            }
          }
        }
      }
      else if (string.Compare(command, "d") == 0)
      {
        if (outFileName == null)
        {
          outFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
        }

				using (GZipStream zip = new GZipStream(new FileStream(fileName, FileMode.Open), CompressionMode.Decompress))
				{
					using (FileStream outfile = new FileStream(outFileName, FileMode.Create))
					{
						while ((bytesRead = zip.Read(buffer, 0, buffer.Length)) > 0)
						{
              outfile.Write(buffer, 0, bytesRead);
						}
					}
				}
			}
      else
      {
        Console.WriteLine("Unknown command");
        Usage();
        return;
      }

      timer.Stop();
      Console.WriteLine("Elapsed: {0}", timer.Elapsed);
    }
  }
}
