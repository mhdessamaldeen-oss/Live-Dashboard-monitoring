namespace Application.Interfaces;

public interface IReportGenerationJob
{
    Task GenerateAsync(int reportId);
}
