# Hello, World!

## Running the app in development

Use the following command: `aspire run` to initialize Aspire. Docker Containers should be automatically orchestrated and kicked off.

If you do not have the Aspire workload, use `dotnet workload install aspire` to obtain it.

By default, Aspire should support hot reload of the C# project. If you get any Node start-up errors, `cd` into `floe-web` and run `npm i`. Aspire will automatically initialize the Vite dev server and will support HMR.

Happy devving!

## Blackbird.Floe.Api

In order to set up a signing-key for local development, run the following command:
`dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"`

The API project is also responsible for creating an OpenAPI spec upon build. This spec is then used by the `floe-web` project to automatically generate a fetch client using `heyapi`. Use the command `npm run gen:api` from the `floe-web` directory to build the API client automagically from the most recently built OpenAPI spec.

