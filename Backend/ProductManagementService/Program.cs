using ProductManagementService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5002); // Lắng nghe trên tất cả các địa chỉ IP, cổng 5001
    // options.ListenLocalhost(5002); // Lắng nghe chỉ trên localhost, cổng 5001
});

// Add services to the container.
builder.Services.AddGrpc();

// Add Connection String
//builder.Services.AddDbContext<ProductManagementContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
