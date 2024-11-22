using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class FetchQuestions: IRequest
{
    private Guid GameId { get; set; }
}

public class FetchQuestionsHandler(IHttpClientFactory httpClientFactory): IRequestHandler<FetchQuestions>
{
    public async Task Handle(FetchQuestions request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://the-trivia-api.com/v2/questions?limit=10");
        var apiQuestions =  JsonSerializer.Deserialize<IList<ApiQuestion>>(await response.Content.ReadAsStringAsync());
    }
}
