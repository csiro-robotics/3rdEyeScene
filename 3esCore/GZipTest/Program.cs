using System;
using System.IO;
using System.Diagnostics;
using Tes.IO.Compression;

namespace Tes
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        Console.WriteLine("Missing file argument.");
        return;
      }
      Stopwatch timer = new Stopwatch();

      timer.Start();

      byte[] buffer = new byte[64 * 1024];
      int bytesRead = 0;
      using (FileStream readin = new FileStream(args[0], FileMode.Open))
      {
        using (GZipStream zip = new GZipStream(new FileStream(string.Format("{0}.gz", args[0]), FileMode.Create), CompressionMode.Compress))
        {
          while ((bytesRead = readin.Read(buffer, 0, buffer.Length)) > 0)
          {
            zip.Write(buffer, 0, bytesRead);
          }
        }
      }

      timer.Stop();
      Console.WriteLine("Elapsed: {0}", timer.Elapsed);
    }
  }
}
