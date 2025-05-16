using CommonGateLib.Models.Common;
using CommonGateLib.Models.Responses;

namespace CommonGateLib.Validation
{
    public interface IServerInstanceFluentValidator
    {
        ResponseIntegration Validate(ServerInstanceModel instanceModel);
    }
}
