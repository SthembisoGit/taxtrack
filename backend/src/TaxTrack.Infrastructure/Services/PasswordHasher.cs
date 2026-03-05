using TaxTrack.Application.Interfaces;

namespace TaxTrack.Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string value) => BCrypt.Net.BCrypt.HashPassword(value);

    public bool Verify(string hash, string value) => BCrypt.Net.BCrypt.Verify(value, hash);
}
