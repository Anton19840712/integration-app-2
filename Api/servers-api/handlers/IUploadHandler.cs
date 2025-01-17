using System.Text.Json;
using servers_api.Models;

namespace servers_api.Handlers
{
    public interface IUploadHandler
    {
        string GenerateResultMessage(
            ResponceIntegration queueCreationTask,
            ResponceIntegration senderConnectionTask,
            ResponceIntegration pushTask,
            ResponceIntegration receiveTask);
        JsonSerializerOptions GetJsonSerializerOptions();
    }
}