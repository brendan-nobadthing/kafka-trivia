namespace KafkaTriviaApi.Contracts;


public record NewGameRequested(
    string Name);

public record NewGameRequestFailed(
    string Name,
    string Reason);

