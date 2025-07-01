using HackerspaceFinder.Model;

namespace HackerspaceFinder.Services;

public interface IHackerspaceService
{
    Task<List<Hackerspace>> LoadHackerspacesAsync();
}
