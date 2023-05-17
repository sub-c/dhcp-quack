namespace DhcpQuack.Server;

internal static class Program
{
  public static void Main(string[] args)
  {
    var dhcpServer = new DhcpServer();
    dhcpServer.StartListening();

    Console.WriteLine("Press enter to stop listening.");
    Console.ReadLine();

    dhcpServer.StopListening();
  }
}
