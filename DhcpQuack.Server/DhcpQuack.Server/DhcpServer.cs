using DhcpQuack.Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DhcpQuack.Server;
internal sealed class DhcpServer
{
  private const int DHCP_CLIENT_PORT = 68;
  private const uint DHCP_OPTIONS_MAGIC_NUMBER = 1669485411;
  private const int DHCP_PACKET_OPTION_OFFSET = 240;
  private const int DHCP_SERVER_PORT = 67;

  private CancellationTokenSource _cancellationTokenSource = new();
  private Task _task = Task.CompletedTask;

  public void StartListening()
  {
    if (!_task.IsCompleted)
      throw new Exception("Cannot start listening, already listening.");

    _cancellationTokenSource = new();
    _task = Task.Run(RunDhcpServerAsync);
  }

  public void StopListening()
  {
    if (_task.IsCompleted)
      return;

    _cancellationTokenSource.Cancel();
    _task.Wait();
  }

  private byte[] GetResponseToDatagram(UdpReceiveResult receivedDatagram)
  {
    if (receivedDatagram.Buffer.Length == 0)
      return Array.Empty<byte>();

    var udpMessage = new UdpDhcpMessage(receivedDatagram.Buffer);
    var message = new DhcpMessage(udpMessage);

    return Array.Empty<byte>();
  }

  private async Task RunDhcpServerAsync()
  {
    try
    {
      using var client = new UdpClient(DHCP_SERVER_PORT);
      var token = _cancellationTokenSource.Token;

      while (!token.IsCancellationRequested)
      {
        var receivedDatagram = await client.ReceiveAsync(token);
        var responseBytes = GetResponseToDatagram(receivedDatagram);

        if (responseBytes.Length > 0)
          await client.SendAsync(responseBytes, token);
      }
    }
    catch (Exception exception)
    {
      Console.WriteLine($"ERROR: {exception.Message}");
    }
  }

  //private const int DHCP_CLIENT_PORT = 68;
  //private const uint DHCP_OPTIONS_MAGIC_NUMBER = 1669485411;
  //private const int DHCP_PACKET_OPTION_OFFSET = 240;
  //private const int DHCP_SERVER_PORT = 67;

  //private CancellationTokenSource _cancellationTokenSource = new();
  //private UdpClient _udpClient = new();

  //public DhcpServer()
  //{
  //}

  //public async Task StartListeningAsync(IPAddress ipAddress)
  //{
  //  _udpClient = new UdpClient(new IPEndPoint(ipAddress, DHCP_SERVER_PORT));
  //  _cancellationTokenSource = new CancellationTokenSource();
  //  await ReceiveAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
  //}

  //private async Task HandleDiscoverMessageAsync(DhcpMessage message)
  //{
  //  // get next lease
  //  var ipAddressOffered = IPAddress.Parse("192.168.1.222");

  //  // send offer
  //  message.OperationCode = DhcpOperation.BootReply;
  //  message.YourIPAddress = ipAddressOffered;
  //  message.ServerIPAddress = IPAddress.Parse("192.168.1.190");

  //  var optionsBuilder = new DhcpOptionBuilder();
  //  optionsBuilder.AddOption(DhcpOptionCode.DhcpMessageType, DhcpMessageType.Offer);
  //  optionsBuilder.AddOption(DhcpOptionCode.DhcpAddress, IPAddress.Parse("192.168.1.190"));
  //  optionsBuilder.AddOption(DhcpOptionCode.SubnetMask, IPAddress.Parse("255.255.255.0"));
  //  optionsBuilder.AddOption(DhcpOptionCode.Router, IPAddress.Parse("192.168.1.1"));
  //  optionsBuilder.AddOption(DhcpOptionCode.AddressTime, 120, true);

  //  // make packet
  //  var dhcpPacket = new DhcpPacket
  //  {
  //    Op = (byte)message.OperationCode,
  //    HType = (byte)message.HardwareType,
  //    Hops = (byte)message.Hops,
  //    XID = BitConverter.GetBytes(message.TransactionId).Reverse().ToArray(),
  //    Secs = BitConverter.GetBytes(message.SecondsElapsed),
  //    Flags = BitConverter.GetBytes(message.Flags),
  //    CIAddr = message.ClientIPAddress.GetAddressBytes(),
  //    YIAddr = message.YourIPAddress.GetAddressBytes(),
  //    SIAddr = message.ServerIPAddress.GetAddressBytes(),
  //    GIAddr = message.GatewayIPAddress.GetAddressBytes(),
  //    Options = optionsBuilder.GetBytes()
  //  };

  //  var chAddressArray = new byte[16];
  //  message.ClientHardwareAddress.GetAddressBytes().CopyTo(chAddressArray, 0);
  //  dhcpPacket.CHAddr = chAddressArray;
  //  dhcpPacket.HLen = (byte)message.ClientHardwareAddress.GetAddressBytes().Length;

  //  // send packet
  //  try
  //  {
  //    // make packet to send
  //    using var stream = new MemoryStream();
  //    using var writer = new BinaryWriter(stream);
      
  //    writer.Write(dhcpPacket.Op);
  //    writer.Write(dhcpPacket.HType);
  //    writer.Write(dhcpPacket.HLen);
  //    writer.Write(dhcpPacket.Hops);
  //    writer.Write(dhcpPacket.XID);

  //    var secsBytes = new byte[2];
  //    dhcpPacket.Secs.CopyTo(secsBytes, 0);
  //    writer.Write(secsBytes);

  //    var flagBytes = new byte[2];
  //    dhcpPacket.Flags.CopyTo(flagBytes, 0);
  //    writer.Write(flagBytes);

  //    writer.Write(dhcpPacket.CIAddr);
  //    writer.Write(dhcpPacket.YIAddr);
  //    writer.Write(dhcpPacket.SIAddr);
  //    writer.Write(dhcpPacket.GIAddr);
  //    writer.Write(dhcpPacket.CHAddr);

  //    var snameBytes = new byte[64];
  //    writer.Write(snameBytes);

  //    var fileBytes = new byte[128];
  //    writer.Write(fileBytes);

  //    writer.Write(BitConverter.GetBytes(DHCP_OPTIONS_MAGIC_NUMBER).Reverse().ToArray());
  //    writer.Write(dhcpPacket.Options);

  //    var responsePacket = stream.ToArray();

  //    // send packet
  //    using var client = new UdpClient();
  //    var endpoint = new IPEndPoint(IPAddress.Broadcast, DHCP_CLIENT_PORT);
  //    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
  //    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
  //    await client.SendAsync(responsePacket, responsePacket.Length, endpoint).ConfigureAwait(false);
  //  }
  //  catch (Exception exception)
  //  {

  //  }
  //}

  //private async Task HandleReceivedMessageAsync(DhcpMessage message)
  //{
  //  if (message.OperationCode != DhcpOperation.BootRequest)
  //    return;

  //  switch (message.DhcpMessageType)
  //  {
  //    case DhcpMessageType.Discover:
  //      await HandleDiscoverMessageAsync(message).ConfigureAwait(false);
  //      break;

  //    case DhcpMessageType.Request:
  //      await HandleRequestMessageAsync(message).ConfigureAwait(false);
  //      break;

  //    default:
  //      break;
  //  }
  //}

  //private async Task HandleReceivedUdpDataAsync(UdpReceiveResult result)
  //{
  //  using var stream = new MemoryStream(result.Buffer);
  //  using var reader = new BinaryReader(stream);
  //  var dhcpPacket = new DhcpPacket
  //  {
  //    Op = reader.ReadByte(),
  //    HType = reader.ReadByte(),
  //    HLen = reader.ReadByte(),
  //    Hops = reader.ReadByte(),
  //    XID = reader.ReadBytes(4),
  //    Secs = reader.ReadBytes(2),
  //    Flags = reader.ReadBytes(2),
  //    CIAddr = reader.ReadBytes(4),
  //    YIAddr = reader.ReadBytes(4),
  //    SIAddr = reader.ReadBytes(4),
  //    GIAddr = reader.ReadBytes(4),
  //    CHAddr = reader.ReadBytes(16),
  //    SName = reader.ReadBytes(64),
  //    File = reader.ReadBytes(128),
  //    Cookie = reader.ReadBytes(4),
  //    Options = reader.ReadBytes(result.Buffer.Length - DHCP_PACKET_OPTION_OFFSET)
  //  };

  //  var dhcpMessage = new DhcpMessage
  //  {
  //    OperationCode = (DhcpOperation)dhcpPacket.Op,
  //    HardwareType = (HardwareType)dhcpPacket.HType,
  //    HardwareAddressLength = dhcpPacket.HLen,
  //    Hops = dhcpPacket.Hops,
  //    TransactionId = BitConverter.ToInt32(dhcpPacket.XID.Reverse().ToArray(), 0),
  //    SecondsElapsed = BitConverter.ToUInt16(dhcpPacket.Secs, 0),
  //    Flags = BitConverter.ToUInt16(dhcpPacket.Flags, 0),
  //    ClientIPAddress = new IPAddress(dhcpPacket.CIAddr),
  //    YourIPAddress = new IPAddress(dhcpPacket.YIAddr),
  //    ServerIPAddress = new IPAddress(dhcpPacket.SIAddr),
  //    GatewayIPAddress = new IPAddress(dhcpPacket.GIAddr),
  //    ClientHardwareAddress = new PhysicalAddress(dhcpPacket.CHAddr.Take(dhcpPacket.HLen).ToArray()),
  //    File = dhcpPacket.File,
  //    Cookie = dhcpPacket.Cookie,
  //    Options = DhcpOptionParser.GetOptions(dhcpPacket.Options)
  //  };

  //  // fill out dhcpMessage.Options
  //  //var endByteIndex = Array.IndexOf(dhcpPacket.Options, (byte)DhcpOptionCode.End);
  //  //var trimmedArray = new byte[endByteIndex + 1];
  //  //Array.Copy(dhcpPacket.Options, 0, trimmedArray, 0, trimmedArray.Length);
  //  //for (var index = 0; index < trimmedArray.Length; ++index)
  //  //{
  //  //  var opCode = (DhcpOptionCode)trimmedArray[index];
  //  //  if (opCode == DhcpOptionCode.End)
  //  //    break;
  //  //  var length = trimmedArray[++index];
  //  //  var data = new byte[length];
  //  //  Array.Copy(trimmedArray, ++index, data, 0, length);
  //  //  index += length;
  //  //  dhcpMessage.Options.Add(opCode, data);
  //  //}

  //  await HandleReceivedMessageAsync(dhcpMessage).ConfigureAwait(false);
  //}

  //private async Task HandleRequestMessageAsync(DhcpMessage message)
  //{

  //}

  //private async Task ReceiveAsync(CancellationToken token)
  //{
  //  while (!token.IsCancellationRequested)
  //  {
  //    try
  //    {
  //      var result = await _udpClient.ReceiveAsync(token).ConfigureAwait(false);

  //      if (result.Buffer.Length > 0)
  //        await HandleReceivedUdpDataAsync(result).ConfigureAwait(false);
  //    }
  //    catch (ObjectDisposedException)
  //    {
  //      break;
  //    }
  //  }
  //}
}
