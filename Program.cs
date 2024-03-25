using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.WebHost.UseUrls("http://localhost:5280");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.MapPost("/payments/pix", (TransferStatus dto) =>
{
  Console.WriteLine($"Processing payment from {dto.Origin.User.CPF} to {dto.Destiny.Key.Value}");
  var timeToWait = GenerateRandomTime();
  Console.WriteLine($"This operation will return in {timeToWait} ms");
  Thread.Sleep(timeToWait);

  return Results.Ok();
});

app.MapPatch("/payments/pix", (HttpContext context, TransferStatusDTO dto) =>
{
  try
  {
    var authorization = context.Request.Headers.Authorization;
    var token = authorization.FirstOrDefault()?.Split(" ")[1];
    string filePath = GenerateLogFile(token);
    WriteLogFile(filePath, dto);
  }
  catch
  {
    Console.WriteLine($"Cannot log payment status id {dto.Id} due to invalid token");
  }

  Console.WriteLine($"Processing payment status id {dto.Id} to {dto.Status}");
  return Results.NoContent();
});

app.MapPost("/concilliation/status", (HttpContext context, ConcilliationOutputDTO dto) =>
{
  try
  {
    var authorization = context.Request.Headers.Authorization;
    var token = authorization.FirstOrDefault()?.Split(" ")[1];
    GenerateConcilliationFile(token, dto);
  }
  catch
  {
    Console.WriteLine($"Cannot check concilliation result due to invalid token");
  }
});

static void GenerateConcilliationFile(string token, ConcilliationOutputDTO dto)
{
  DateTime today = DateTime.Today;
  string tokenPath = $"./Concilliation/{token}/{today.ToString("dd-MM-yyyy")}";
  if (!Directory.Exists(tokenPath))
  {
    Directory.CreateDirectory(tokenPath);
  }

  string filePath = $"{tokenPath}/result-{DateTime.Now.ToString("HH:mm:ss")}.json";
  if (!File.Exists(filePath))
  {
    File.Open(filePath, FileMode.Create).Close();
  }

  using TextWriter file = new StreamWriter(filePath, true);
  string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
  file.WriteLine(json);
  Console.WriteLine($"Concilliation results at: {filePath}");
}

static string GenerateLogFile(string token)
{
  DateTime today = DateTime.Today;
  string tokenPath = $"./Logs/{token}";
  if (!Directory.Exists(tokenPath))
  {
    Directory.CreateDirectory(tokenPath);
  }

  string filePath = $"{tokenPath}/transactions-{today.ToString("dd-MM-yyyy")}.json";
  if (!File.Exists(filePath))
  {
    File.Open(filePath, FileMode.Create).Close();
  }
  return filePath;
}

static void WriteLogFile(string filePath, TransferStatusDTO dto)
{
  using TextWriter file = new StreamWriter(filePath, true);
  if (!File.Exists(filePath))
    return;

  TransferStatusDTO content = new() { Id = dto.Id, Status = dto.Status };
  string json = JsonSerializer.Serialize(content);
  file.WriteLine(json);
}

static int GenerateRandomTime()
{
  Random random = new();
  int lowPercentage = 5; // 5% das reqs são lentas
  int percentageChoice = random.Next(1, 101);
  if (percentageChoice <= lowPercentage) return random.Next(60000, 90000); // TODO: you can change
  else return random.Next(100, 500);
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.Run();