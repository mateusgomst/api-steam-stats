using System;
using System.Threading.Tasks;
using Quartz;

public class DailyJob : IJob
{
    private readonly DailyTaskService _dailyTaskService;

    public DailyJob(DailyTaskService dailyTaskService)
    {
        _dailyTaskService = dailyTaskService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine($"[INFO] Job iniciada em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

        try
        {
            await _dailyTaskService.ExecuteDailyTask();
            Console.WriteLine($"[INFO] Job finalizada com sucesso em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Job falhou em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"[CRITICAL] Erro: {ex.Message}");
        }
    }
}