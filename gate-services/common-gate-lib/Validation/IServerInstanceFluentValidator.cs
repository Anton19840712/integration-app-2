
using CommonGateLib.Models;
using CommonGateLib.Models.Response;

namespace CommonGateLib.Validation
{
    public interface IServerInstanceFluentValidator
    {
        ResponseIntegration Validate(ServerInstanceModel instanceModel);
    }
}
