namespace DhcpQuack.Server.Models;
public enum DhcpOperation : byte
{
  Unknown = 0,
  BootRequest = 0x01,
  BootReply
}
