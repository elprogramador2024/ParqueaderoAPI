namespace ParqueaderoAPI.Services.Comunes
{
    public interface ITokenBlackListService
    {
        void AddToBlacklist(string token, DateTime fechaExpira);
        bool IsTokenBlacklisted(string token);
    }
}
