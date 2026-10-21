# Hello, World!

## Blackbird.Floe.Api

In order to set up a signing-key for local development, run the following command:
`dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)"`

The API project is also responsible for creating an OpenAPI spec upon build. This spec is then used by the `floe-web` project to automatically generate a fetch client using `heyapi`.