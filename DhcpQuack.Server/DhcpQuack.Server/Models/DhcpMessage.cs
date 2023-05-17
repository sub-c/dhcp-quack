using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace DhcpQuack.Server.Models;

internal sealed class DhcpMessage
{
  public DhcpOperation OperationCode { get; set; } = DhcpOperation.Unknown;
  public HardwareType HardwareType { get; set; } = HardwareType.Unknown;
  public ushort HardwareAddressLength { get; set; }
  public ushort Hops { get; set; }
  public int TransactionID { get; set; }
  public ushort SecondsElapsed { get; set; }
  public ushort Flags { get; set; }
  public IPAddress ClientIPAddress { get; set; } = IPAddress.None;
  public IPAddress YourIPAddress { get; set; } = IPAddress.None;
  public IPAddress ServerIPAddress { get; set; } = IPAddress.None;
  public IPAddress GatewayIPAddress { get; set; } = IPAddress.None;
  public PhysicalAddress ClientHardwareAddress { get; set; } = PhysicalAddress.None;
  public byte[] SName { get; set; } = Array.Empty<byte>();
  public byte[] File { get; set; } = Array.Empty<byte>();
  public byte[] Cookie { get; set; } = Array.Empty<byte>();
  public Dictionary<DhcpOptionCode, byte[]> Options { get; } = new Dictionary<DhcpOptionCode, byte[]>();
  public byte[] OptionBytes { get; set; } = Array.Empty<byte>();

  public DhcpMessageType MessageType => Options.Count > 0 && Options.TryGetValue(DhcpOptionCode.DhcpMessageType, out var data)
    ? (DhcpMessageType)data[0]
    : DhcpMessageType.Unknown;

  public string HostName => Options.Count > 0 && Options.TryGetValue(DhcpOptionCode.Hostname, out var data)
    ? Encoding.Default.GetString(data)
    : string.Empty;

  public DhcpMessage(UdpDhcpMessage udpMessage)
  {
    OperationCode = (DhcpOperation)udpMessage.Op;
    HardwareType = (HardwareType)udpMessage.HType;
    HardwareAddressLength = udpMessage.HLen;
    Hops = udpMessage.Hops;
    TransactionID = BitConverter.ToInt32(udpMessage.XID.Reverse().ToArray(), 0);
    SecondsElapsed = BitConverter.ToUInt16(udpMessage.Secs, 0);
    Flags = BitConverter.ToUInt16(udpMessage.Flags, 0);
    ClientIPAddress = new IPAddress(udpMessage.CIAddr);
    YourIPAddress = new IPAddress(udpMessage.YIAddr);
    ServerIPAddress = new IPAddress(udpMessage.SIAddr);
    GatewayIPAddress = new IPAddress(udpMessage.GIAddr);
    ClientHardwareAddress = new PhysicalAddress(udpMessage.CHAddr.Take(udpMessage.HLen).ToArray());
    File = udpMessage.File;
    Cookie = udpMessage.Cookie;
    Options = DhcpOptionParser.GetOptions(udpMessage.Options);
    OptionBytes = udpMessage.Options;
  }

  public UdpDhcpMessage ToUdpMessage()
  {
    var udpMessage = new UdpDhcpMessage
    {
      Op = (byte)OperationCode,
      HType = (byte)HardwareType,
      HLen = (byte)HardwareAddressLength,
      Hops = (byte)Hops,
      XID = BitConverter.GetBytes(TransactionID).Reverse().ToArray(),
      Secs = BitConverter.GetBytes(SecondsElapsed),
      Flags = BitConverter.GetBytes(Flags),
      CIAddr = ClientIPAddress.GetAddressBytes(),
      YIAddr = YourIPAddress.GetAddressBytes(),
      SIAddr = ServerIPAddress.GetAddressBytes(),
      GIAddr = GatewayIPAddress.GetAddressBytes(),
      CHAddr = ClientHardwareAddress.GetAddressBytes(),
      SName = SName,
      File = File,
      Cookie = Cookie,
      Options = OptionBytes
    };

    return udpMessage;
  }

  //public PhysicalAddress ClientHardwareAddress { get; set; } = PhysicalAddress.None;
  //public IPAddress ClientIPAddress { get; set; } = IPAddress.None;
  //public byte[] Cookie { get; set; } = Array.Empty<byte>();

  //public DhcpMessageType DhcpMessageType
  //{
  //  get
  //  {
  //    if (Options.Count == 0)
  //      return DhcpMessageType.Unknown;

  //    if (Options.ContainsKey(DhcpOptionCode.DhcpMessageType) == false)
  //      return DhcpMessageType.Unknown;

  //    var data = Options[DhcpOptionCode.DhcpMessageType][0];
  //    return (DhcpMessageType)data;
  //  }
  //}

  //public byte[] File { get; set; } = Array.Empty<byte>();
  //public ushort Flags { get; set; }
  //public IPAddress GatewayIPAddress { get; set; } = IPAddress.None;
  //public ushort HardwareAddressLength { get; set; }
  //public HardwareType HardwareType { get; set; }
  //public ushort Hops { get; set; }

  //public string HostName
  //{
  //  get
  //  {
  //    if (Options.Count == 0)
  //      return string.Empty;

  //    if (Options.ContainsKey(DhcpOptionCode.Hostname) == false)
  //      return string.Empty;

  //    var data = Options[DhcpOptionCode.Hostname];
  //    return Encoding.Default.GetString(data);
  //  }
  //}

  //public DhcpOperation OperationCode { get; set; }
  //public Dictionary<DhcpOptionCode, byte[]> Options { get; set; } = new();
  //public ushort SecondsElapsed { get; set; }
  //public IPAddress ServerIPAddress { get; set; } = IPAddress.None;
  //public int TransactionId { get; set; }
  //public IPAddress YourIPAddress { get; set; } = IPAddress.None;
}
