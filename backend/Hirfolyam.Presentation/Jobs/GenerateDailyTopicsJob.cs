using Hirfolyam.Application.Features;
using MediatR;
using Quartz;

namespace Hirfolyam.Presentation.Jobs;

[DisallowConcurrentExecution]
public sealed class GenerateDailyTopicsJob(ISender sender) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return sender.Send(new GenerateDailyTopicsCommand(), context.CancellationToken);
    }
}
