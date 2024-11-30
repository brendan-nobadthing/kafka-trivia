using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class AnswerQuestion: IRequest
{
    public Guid GameId { get; set; }
    public Guid ParticipantId { get; set; }
    public int QuestionNumber { get; set; }
    public int AnswerIndex { get; set; }
}

public class AnswerQuestionHandler(IMessageSender<AnswerQuestion> sender) : IRequestHandler<AnswerQuestion>
{
    public async Task Handle(AnswerQuestion request, CancellationToken cancellationToken)
    {
        await sender.Send(request.GameId.ToString(), request, cancellationToken);
    }
}

