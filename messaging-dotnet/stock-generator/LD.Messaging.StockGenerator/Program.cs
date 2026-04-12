using LD.Messaging.StockGenerator;
using LD.Messaging.StockGenerator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<StockFileGenerator>();
builder.Services.AddHostedService<FileGeneratorService>();

await builder.Build().RunAsync();
