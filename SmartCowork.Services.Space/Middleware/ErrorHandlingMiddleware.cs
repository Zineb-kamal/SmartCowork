namespace SmartCowork.Services.Space.Middleware
{
    // Middleware/ErrorHandlingMiddleware.cs
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Une erreur s'est produite");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse();

            switch (ex)
            {
                case InvalidOperationException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Message = "Opération invalide";
                    break;
                case UnauthorizedAccessException:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    response.Message = "Non autorisé";
                    break;
                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.Message = "Une erreur interne s'est produite";
                    break;
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
