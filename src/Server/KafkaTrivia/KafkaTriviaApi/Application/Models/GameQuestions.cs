namespace KafkaTriviaApi.Application.Models;

public record GameQuestions(
    Guid GameId,
    IList<GameQuestion> Questions);

public record GameQuestion(
    int QuestionNumber,
    string QuestionText,
    IList<string> Answers,
    int CorrectAnswerIndex
    );