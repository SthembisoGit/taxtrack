namespace TaxTrack.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string value);

    bool Verify(string hash, string value);
}
