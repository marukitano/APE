namespace ChaosMap_V4.Services;

public interface ISpaceApiProvider
{
    Task<string> LoadJsonAsync();
}
