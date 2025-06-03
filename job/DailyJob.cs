using Quartz;
using System;
using System.Threading.Tasks;

public class DailyJob : IJob
{
    private readonly DailyTaskService _dailyTaskService;

    public DailyJob(DailyTaskService dailyTaskService)
    {
        _dailyTaskService = dailyTaskService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"Job executada em: {DateTime.Now}");
        await _dailyTaskService.ExecuteDailyTask();
    }
}
