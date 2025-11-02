namespace AutoEdge.Services
{
    public interface IAIAssistantService
    {
        Task<string> GetReplyAsync(string userMessage);
    }
}
