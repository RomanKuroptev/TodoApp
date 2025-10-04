namespace TodoApp.Api.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
            .WithName("HealthCheck")
            .WithDescription("Simple health check endpoint")
            .WithTags("Health")
            .Produces(StatusCodes.Status200OK);
    }
}
