using Tes.IO;

namespace Tes.Net
{
  public interface IMessage
  {
    bool Write(PacketBuffer writer);
  }
}
