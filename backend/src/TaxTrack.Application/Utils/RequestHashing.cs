using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TaxTrack.Application.Utils;

public static class RequestHashing
{
    public static string ComputeHash(object value)
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(bytes);
    }
}
