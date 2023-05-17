namespace DhcpQuack.Server.Models;

public enum DhcpMessageType
{
  Unknown = 0x00,
  Discover,
  Offer,
  Request,
  Decline,
  Ack,
  Nak,
  Release,
  Inform,
  ForceRenew,
  LeaseQuery,
  LeaseUnassigned,
  LeaseUnknown,
  LeaseActive
}
