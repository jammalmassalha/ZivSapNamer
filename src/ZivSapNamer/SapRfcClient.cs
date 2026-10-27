using Microsoft.Extensions.Options;
#if SAP_NCO
using SAP.Middleware.Connector;
#endif

namespace ZivSapNamer;

public sealed class SapRfcClient(IOptions<SapOptions> options)
{
    public void Ping()
    {
        var settings = options.Value;
#if SAP_NCO
        var parameters = new RfcConfigParameters
        {
            [RfcConfigParameters.Name] = "ZivSapNamer",
            [RfcConfigParameters.AppServerHost] = settings.ApplicationServerHost,
            [RfcConfigParameters.SystemNumber] = settings.SystemNumber,
            [RfcConfigParameters.Client] = settings.Client,
            [RfcConfigParameters.User] = settings.User,
            [RfcConfigParameters.Password] = settings.Password,
            [RfcConfigParameters.Language] = settings.Language
        };
        RfcDestinationManager.GetDestination(parameters).Ping();
#else
        throw new InvalidOperationException(
            "SAP .NET Connector is not enabled. Build with EnableSapNco=true; see README.md.");
#endif
    }
}
