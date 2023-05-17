using DhcpQuack.Server.Models;

namespace DhcpQuack.Server;

public static class DhcpOptionParser
{
  public static Dictionary<DhcpOptionCode, byte[]> GetOptions(byte[] options)
  {
    var result = new Dictionary<DhcpOptionCode, byte[]>();
    options = TrimEmptyData(options);
    result = ParseOptions(options);

    return result;
  }

  private static byte[] TrimEmptyData(byte[] options)
  {
    var endByteIndex = Array.IndexOf(options, (byte)DhcpOptionCode.End);
    byte[] trimmedArray = new byte[endByteIndex + 1];

    Array.Copy(options, 0, trimmedArray, 0, endByteIndex + 1);

    return trimmedArray;
  }

  private static Dictionary<DhcpOptionCode, byte[]> ParseOptions(byte[] options)
  {
    int index = 0;
    int dataLength = options.Length;
    var result = new Dictionary<DhcpOptionCode, byte[]>();

    while (index < dataLength)
    {
      var opCode = (DhcpOptionCode)options[index];

      if (opCode == DhcpOptionCode.End) break;

      var length = options[++index];

      byte[] data = new byte[length];
      Array.Copy(options, ++index, data, 0, length);

      index += length;

      result.Add(opCode, data);
    }

    return result;
  }
}
