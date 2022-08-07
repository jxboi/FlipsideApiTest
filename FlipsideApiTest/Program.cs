using Flurl;
using Flurl.Http;

var builder = WebApplication.CreateBuilder(args);

// Get Flipside API key store in environment variable
var flipsideApiKey = builder.Configuration["Flipside:ApiKey"];

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("QueryToken", async () =>
{
    var result = await GetApiTokenAsync();
    return result;
});

async Task<string?> GetApiTokenAsync()
{
    var result = await "https://node-api.flipsidecrypto.com"
        .AppendPathSegment("queries")
        .WithHeader("x-api-key", flipsideApiKey)
        .PostJsonAsync(new
        {
            sql = "SELECT nft_address, mint_price_eth, mint_price_usd FROM ethereum.core.ez_nft_mints LIMIT 2",
            ttlMinutes = 5
        }).ReceiveJson<QueryResult>();

    return result.Token;
}

app.MapGet("QueryResult", async () =>
{
    var token = await GetApiTokenAsync();

    var result = await "https://node-api.flipsidecrypto.com"
        .AppendPathSegment("queries")
        .AppendPathSegment(token)
        .WithHeader("x-api-key", flipsideApiKey)
        .GetJsonAsync();

    return result;
});

app.Run();

class QueryResult
{
    public string? Token { get; set; }
    public bool Cache { get; set; }
}