var builder = WebApplication.CreateBuilder(args);

// Register YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Enable YARP middleware
app.MapReverseProxy();

app.Run();