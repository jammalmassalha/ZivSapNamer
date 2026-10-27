using System.Security.Cryptography;
using System.Text;
using ZivSapNamer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<SapOptions>()
    .BindConfiguration("Sap")
    .ValidateDataAnnotations()
    .ValidateOnStart();
var apiKey = builder.Configuration["ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < 32)
{
    throw new InvalidOperationException("Configure ApiKey with at least 32 characters using user secrets or an environment variable.");
}

builder.Services.AddSingleton<SapRfcClient>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapPost("/api/sap/ping", (HttpContext context, SapRfcClient sap, ILogger<Program> logger) =>
{
    var suppliedKey = context.Request.Headers["X-Api-Key"];
    if (suppliedKey.Count != 1 || suppliedKey[0]?.Length != apiKey.Length ||
        !CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(suppliedKey[0]!), Encoding.UTF8.GetBytes(apiKey)))
    {
        return Results.Unauthorized();
    }

    try
    {
        sap.Ping();
        return Results.Ok(new { status = "connected" });
    }
    catch (Exception exception)
    {
        // SAP exceptions may contain connection details; do not expose or log their messages.
        logger.LogWarning("SAP RFC ping failed ({ExceptionType}).", exception.GetType().Name);
        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "SAP RFC connection unavailable",
            detail: "Check the server's SAP connector installation and connection configuration.");
    }
});

app.Run();
