using Confluent.Kafka;
using KafkaTriviaApi.KafkaProducer;
using KafkaTriviaApi.KafkaStreams;
using KafkaTriviaApi.Rest;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddSingleton(builder.Configuration.GetSection("producer").Get<ProducerConfig>()!);
builder.Services.AddKafkaProducers();

var kss = new KafkaStreamService();
builder.Services.AddSingleton(kss);
builder.Services.AddHostedService(_ => kss);
builder.Services.AddSingleton<KafkaInit>();


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add rest endpoints via extensions methods
app.AddEndpoints();

app.Services.GetService<KafkaInit>()!.InitTopics().GetAwaiter().GetResult();

app.Run();

