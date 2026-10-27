# ZivSapNamer

ASP.NET Core (.NET 8) web service connecting to SAP through the official SAP
.NET Connector (NCo). `POST /api/sap/ping` authenticates to SAP and performs an
RFC ping. No arbitrary RFC functions are exposed.

## Prerequisites

- .NET 8 SDK.
- For a real SAP connection: a Windows x64 host, the .NET 8-compatible x64
  distribution of SAP NCo 3.1, and its prerequisites from
  [SAP's connector download page](https://support.sap.com/en/product/connectors/msnet.html).
  Obtain the connector using your authorized SAP account; it is proprietary and
  is not supplied through this repository or an unofficial NuGet package.
- A reachable SAP application server and a dedicated, least-privilege SAP RFC
  user. Have your SAP administrator configure network access and authorizations.

## Configure

Keep passwords and API keys out of `appsettings.json`. For local development,
use user secrets (stored outside the repository):

```sh
dotnet user-secrets set "Sap:ApplicationServerHost" "<sap-host>" --project src/ZivSapNamer
dotnet user-secrets set "Sap:SystemNumber" "00" --project src/ZivSapNamer
dotnet user-secrets set "Sap:Client" "100" --project src/ZivSapNamer
dotnet user-secrets set "Sap:User" "<rfc-user>" --project src/ZivSapNamer
dotnet user-secrets set "Sap:Password" "<sap-password>" --project src/ZivSapNamer
dotnet user-secrets set "ApiKey" "<random-api-key-of-at-least-32-characters>" --project src/ZivSapNamer
```

`Sap:Language` defaults to `EN`. System number and client must be exactly two
and three digits respectively, including leading zeroes. Configuration is
validated on startup.

In production, inject `Sap__ApplicationServerHost`, `Sap__SystemNumber`,
`Sap__Client`, `Sap__User`, `Sap__Password`, optional `Sap__Language`, and `ApiKey`
through your deployment's secret store/environment. User secrets are loaded
only in Development. Do not put real secrets in scripts, Git, or shell history.

## Build and run

The default build has no proprietary dependencies and can run on Linux, macOS,
or Windows for HTTP/configuration checks. It **does not connect to SAP**:
authenticated ping requests return HTTP 503 until the NCo-enabled build is used.

```sh
dotnet build src/ZivSapNamer
dotnet run --project src/ZivSapNamer --environment Development
```

For a real connection, place `sapnco.dll` and `sapnco_utils.dll` from the
**.NET 8 x64** connector distribution in `lib/sapnco/` (ignored by Git). Do not
use the .NET Framework assemblies. Install all native/runtime prerequisites
listed by SAP on the Windows host, matching the connector's architecture.

```sh
dotnet build src/ZivSapNamer -p:EnableSapNco=true
dotnet run --project src/ZivSapNamer -p:EnableSapNco=true --environment Development
dotnet publish src/ZivSapNamer -c Release -r win-x64 --self-contained false -p:EnableSapNco=true
```

Alternatively, pass `-p:SapNcoPath=<absolute-connector-directory>` along with
`-p:EnableSapNco=true`. Missing assemblies produce a clear build error. Referenced
assemblies are copied to the build/publish output.

## HTTP API

Use the URL printed by ASP.NET Core at startup. For example, with
`ASPNETCORE_URLS=http://localhost:5000`:

```sh
curl http://localhost:5000/health
curl -X POST http://localhost:5000/api/sap/ping -H "X-Api-Key: $SAP_API_KEY"
```

`SAP_API_KEY` here is a caller-side variable containing the same value configured
as `ApiKey` on the server.

- `GET /health`: HTTP 200, process liveness only; it does not contact SAP.
- `POST /api/sap/ping`: HTTP 200 `{"status":"connected"}` after a successful RFC
  ping; HTTP 401 for a missing/incorrect key; HTTP 503 for connector/SAP errors.
  Connection details and SAP exception messages are not returned or logged.

Local HTTP is for loopback development only. In production, require HTTPS at a
trusted reverse proxy, restrict network access to this service, and apply request
rate limits/timeouts there. API keys do not encrypt HTTP or the SAP connection;
coordinate SAP transport protection (for example SNC) with your SAP administrator
before transmitting production credentials over an untrusted network. This
initial implementation configures a direct application-server connection.

## Verification

There is no automated test framework in this repository. Build with
`dotnet build src/ZivSapNamer`, then check liveness, unauthorized requests, and
the default build's HTTP 503 response using the endpoints above. To verify the
actual RFC path, build with NCo on Windows and repeat the authenticated request
against a configured SAP system; this requires licensed binaries and SAP access.