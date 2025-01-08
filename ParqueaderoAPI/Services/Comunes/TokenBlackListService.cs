using Microsoft.Extensions.Caching.Memory;

namespace ParqueaderoAPI.Services.Comunes
{
    public class TokenBlackListService : ITokenBlackListService
    {
        private readonly IMemoryCache _cache;

        public TokenBlackListService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddToBlacklist(string token, DateTime fechaExpira)
        {
            var tokenTiempo = fechaExpira - DateTime.UtcNow;
            if (tokenTiempo <= TimeSpan.Zero) tokenTiempo = TimeSpan.Zero;

            _cache.Set(token, true, tokenTiempo);
        }

        public bool IsTokenBlacklisted(string token)
        {
            return _cache.TryGetValue(token, out _);
        }
    }
}
