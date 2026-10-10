using Hirfolyam.Application.Features;
using MediatR;
using Quartz;

namespace Hirfolyam.Presentation.Jobs;

[DisallowConcurrentExecution]
public sealed class ImportArticlesJob(ISender sender) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        return sender.Send(new ImportArticlesCommand(), context.CancellationToken);
    }
}
