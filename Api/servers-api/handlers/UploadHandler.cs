using servers_api.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Text;

namespace servers_api.Handlers
{
    /// <summary>
    /// Класс занимается валидацией результатов каждого из процесса настройки интеграции. Их пока насчитыаается 4. 
    /// Если все они произошли успешно - тогда результируется модель, которая сообщает клиенту интеграции, что вся интеграция 
    /// завершилась успешно.
    /// </summary>
    public class UploadHandler : IUploadHandler
    {
        public JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
        }

        public string GenerateResultMessage(
            ResponceIntegration queueCreationTask,
            ResponceIntegration senderConnectionTask,
            ResponceIntegration apiStatusTask,
            ResponceIntegration receiveTask)
        {
            var results = new List<ResponceIntegration> { queueCreationTask, senderConnectionTask, apiStatusTask, receiveTask };

            var successfulResults = results.Where(r => r.Result).ToList();
            var failedResults = results.Where(r => !r.Result).ToList();

            var resultMessage = new StringBuilder();

            if (successfulResults.Count == results.Count())
            {
                resultMessage.Append("Все процессы завершились успешно.");
            }
            else
            {
                resultMessage.Append("Некоторые процессы завершились с ошибками:\n");
                foreach (var failed in failedResults)
                {
                    resultMessage.AppendLine($"- {failed.Message}");
                }
            }

            return resultMessage.ToString();
        }
    }
}
