namespace ParqueaderoAPI.Services.Comunes
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlackListService _tokenBlacklistService;

        public TokenValidationMiddleware(RequestDelegate next, ITokenBlackListService tokenBlacklistService)
        {
            _next = next;
            _tokenBlacklistService = tokenBlacklistService;
        }

        public async Task Invoke(HttpContext context)
        {
            var authorization = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authorization))
            {
                var token = authorization.Replace("Bearer ", "");

                if (_tokenBlacklistService.IsTokenBlacklisted(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token is blacklisted.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
