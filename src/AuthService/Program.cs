using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok("AuthService is running - and sanuk is testing - dulain is watching."));

app.MapPost("/login", (LoginRequest request) =>
{
    if (request.Username == "admin" && request.Password == "password")
    {
        return Results.Ok(new
        {
            token = "fake-jwt-token",
            message = "Login successful"
        });
    }

    return Results.Unauthorized();
});

app.MapGet("/db-check", async (IConfiguration config) =>
{
    var cs = config.GetConnectionString("AuthDb") ?? "";

    var safe = Regex.Replace(cs, "(Password=)([^;]*)", "$1***", RegexOptions.IgnoreCase);

    try
    {
        await using var conn = new MySqlConnection(cs);
        await conn.OpenAsync();

        await using var who = new MySqlCommand("SELECT USER(), CURRENT_USER(), @@hostname;", conn);
        await using var reader = await who.ExecuteReaderAsync();
        await reader.ReadAsync();

        var user = reader.GetString(0);        // USER()
        var currentUser = reader.GetString(1); // CURRENT_USER()
        var host = reader.GetString(2);        // @@hostname

        return Results.Ok(new
        {
            status = "DB Connected",
            usingConn = safe,
            user,
            currentUser,
            host
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "DB connection failed",
            detail: ex.Message + " | usingConn=" + safe,
            statusCode: 500
        );
    }
});

Console.WriteLine("AuthDb Conn = " + (builder.Configuration.GetConnectionString("AuthDb") ?? "<null>"));
app.Run();

record LoginRequest(string Username, string Password);