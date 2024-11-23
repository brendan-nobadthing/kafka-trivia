using MediatR;
using System.Text.Json;

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
        var apiQuestions =  JsonSerializer.Deserialize<IList<ApiQuestion>>(await response.Content.ReadAsStringAsync(cancellationToken));
        // TODO - push questions to state
    }
}



// these classes generasted from api spec by json2csharp
public class ApiQuestionText
{
    public string? text { get; set; }
}

public class ApiQuestion
{
    public string? category { get; set; }
    public string? id { get; set; }
    public string? correctAnswer { get; set; }
    public List<string> incorrectAnswers { get; set; } = new List<string>();
    public ApiQuestionText? question { get; set; }
    public List<string> tags { get; set; } = new List<string>();
    public string? type { get; set; }
    public string? difficulty { get; set; }
    public List<object> regions { get; set; } = new List<object>();
    public bool? isNiche { get; set; }
}


