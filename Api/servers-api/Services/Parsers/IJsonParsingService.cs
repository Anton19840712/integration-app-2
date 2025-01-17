using System.Text.Json;
using servers_api.Models;

namespace servers_api.Services.Parsers
{
    public interface IJsonParsingService
    {
        CombinedModel ParseJson(JsonElement jsonBody);
    }
}