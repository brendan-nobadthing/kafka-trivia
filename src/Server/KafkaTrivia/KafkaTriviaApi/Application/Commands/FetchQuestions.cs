using MediatR;
using System.Text.Json;
using KafkaTriviaApi.Application.Models;

namespace KafkaTriviaApi.Application.Commands;

public class FetchQuestions: IRequest<GameQuestions>
{
    public Guid GameId { get; set; }
}

public class FetchQuestionsHandler(IHttpClientFactory httpClientFactory): IRequestHandler<FetchQuestions, GameQuestions>
{

    public async Task<GameQuestions> Handle(FetchQuestions request, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync("https://the-trivia-api.com/v2/questions?limit=10");
        var apiQuestions =  JsonSerializer.Deserialize<IList<ApiQuestion>>(await response.Content.ReadAsStringAsync(cancellationToken));
        
        Random rnd = new Random();
        int questionNumber = 0;
        IList<GameQuestion> gameQuestions = apiQuestions.Select(q =>
        {
            int correctAnswerIndex = rnd.Next(3);
            var answers = q.incorrectAnswers;
            answers.Insert(correctAnswerIndex, q.correctAnswer!);
            return new GameQuestion(
                ++questionNumber,
                q!.question!.text!,
                answers,
                correctAnswerIndex
            );
        }).ToList();
        return new GameQuestions(GameId: request.GameId, Questions: gameQuestions);
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


