using Confluent.Kafka;
using KafkaTriviaApi.GraphQL;
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

builder.Services.AddHttpClient();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddSingleton(builder.Configuration.GetSection("producer").Get<ProducerConfig>()!);
builder.Services.AddKafkaProducers();


builder.Services.AddSingleton<KafkaStreamService>();
builder.Services.AddHostedService(s => s.GetRequiredService<KafkaStreamService>());
builder.Services.AddSingleton<KafkaInit>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddSubscriptionType<Subscription>()
    .AddMutationType<Mutation>()
    .AddInMemorySubscriptions();

builder.Services.AddCors(c => c.AddDefaultPolicy(corsPolicyBuilder =>
{
    corsPolicyBuilder
        .SetIsOriginAllowed(o => new Uri(o).Host == "localhost")
        .AllowAnyMethod()
        .AllowAnyHeader();
}));


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
}

app.UseHttpsRedirection();

// Add rest endpoints via extensions methods
app.AddEndpoints();
app.UseWebSockets();
app.MapGraphQL();

app.Services.GetService<KafkaInit>()!.InitTopics().GetAwaiter().GetResult();

app.Run();

