# Hello, World!

## Running the app in development

Use the following command: `aspire run` to initialize Aspire. Docker Containers should be automatically orchestrated and kicked off.

If you do not have the Aspire workload, use `dotnet workload install aspire` to obtain them.

By default, Aspire should support hot reload. If you get any client error, `cd` into `floe-web` and run `npm i`. Aspire will automatically initialize the Vite test server and will support HMR.

Happy devving!

## Blackbird.Floe.Api

In order to set up a signing-key for local development, run the following command:
`dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"`

The API project is also responsible for creating an OpenAPI spec upon build. This spec is then used by the `floe-web` project to automatically generate a fetch client using `heyapi`.

