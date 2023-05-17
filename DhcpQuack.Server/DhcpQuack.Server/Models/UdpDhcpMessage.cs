namespace DhcpQuack.Server.Models;

internal sealed class UdpDhcpMessage
{
  private const int OPTION_OFFSET = 240;
  private const uint OPTIONS_MAGIC_NUMBER = 1669485411;

  public byte Op { get; set; }
  public byte HType { get; set; }
  public byte HLen { get; set; }
  public byte Hops { get; set; }
  public byte[] XID { get; set; } = Array.Empty<byte>();
  public byte[] Secs { get; set; } = Array.Empty<byte>();
  public byte[] Flags { get; set; } = Array.Empty<byte>();
  public byte[] CIAddr { get; set; } = Array.Empty<byte>();
  public byte[] YIAddr { get; set; } = Array.Empty<byte>();
  public byte[] SIAddr { get; set; } = Array.Empty<byte>();
  public byte[] GIAddr { get; set; } = Array.Empty<byte>();
  public byte[] CHAddr { get; set; } = Array.Empty<byte>();
  public byte[] SName { get; set; } = Array.Empty<byte>();
  public byte[] File { get; set; } = Array.Empty<byte>();
  public byte[] Cookie { get; set; } = Array.Empty<byte>();
  public byte[] Options { get; set; } = Array.Empty<byte>();

  public UdpDhcpMessage()
  {
  }

  public UdpDhcpMessage(byte[] data)
  {
    if (data.Length < OPTION_OFFSET)
      throw new Exception($"Malformed UDP DHCP message, length is too small: {data.Length}");

    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);

    Op = reader.ReadByte();
    HType = reader.ReadByte();
    HLen = reader.ReadByte();
    Hops = reader.ReadByte();
    XID = reader.ReadBytes(4);
    Secs = reader.ReadBytes(2);
    Flags = reader.ReadBytes(2);
    CIAddr = reader.ReadBytes(4);
    YIAddr = reader.ReadBytes(4);
    SIAddr = reader.ReadBytes(4);
    GIAddr = reader.ReadBytes(4);
    CHAddr = reader.ReadBytes(16);
    SName = reader.ReadBytes(64);
    File = reader.ReadBytes(128);
    Cookie = reader.ReadBytes(4);
    Options = reader.ReadBytes(data.Length - OPTION_OFFSET);
  }

  public byte[] ToByteArray()
  {
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);

    writer.Write(Op);
    writer.Write(HType);
    writer.Write(HLen);
    writer.Write(Hops);
    writer.Write(XID);
    writer.Write(Secs);
    writer.Write(Flags);
    writer.Write(CIAddr);
    writer.Write(YIAddr);
    writer.Write(SIAddr);
    writer.Write(GIAddr);
    writer.Write(CHAddr);
    writer.Write(SName);
    writer.Write(File);
    writer.Write(BitConverter.GetBytes(OPTIONS_MAGIC_NUMBER).Reverse().ToArray());
    writer.Write(Options);

    return stream.ToArray();
  }
}
