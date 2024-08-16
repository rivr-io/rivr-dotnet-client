# Rivr .NET Client

[![nuget](https://img.shields.io/nuget/v/rivr.svg)](https://www.nuget.org/packages/rivr/) [![nuget](https://img.shields.io/nuget/dt/rivr.svg)](https://www.nuget.org/packages/rivr/) [![Build and tests](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/rivr-io/rivr-dotnet-client/actions/workflows/build-and-publish.yml)

## Security

The industry-standard [OAuth2](https://oauth.net/2/) security protocol is used for authorization.

### System level

The system level authorization utilizes a simplified version of OAuth2 and uses `grant_type = client_credentials`, which uses `clientId` and `clientSecret` to fetch an `access_token`. It is valid for 60 minutes and it is recommended to use a caching mechanism. In this example implementation, a file-based caching is used, but if you have multiple server instances, you might need to switch to something like Redis.

### Merchant level

The merchant level authorization is a variant of OAuth2 which allows the system to act on behalf of a merchant. It is very similar to the system level authorization, but uses our own `grant_type = merchant_credentials`. With this, the `clientId` and the `clientSecret` along with a `merchantId` are used to fetch an `access_token` that includes the `merchantId`. This allows for the system to perform actions on behalf of the authorized merchant.

### Which should I use?

The system level authorization is used when the system needs to perform tasks as itself, and the merchant level authorization is used when the system needs to perform tasks on behalf of a merchant.

An example of an response from the authentication endpoints:

```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJjbGllbnQtaWQiOiI3OWZlZTMwMi1lZDkyLTRjMDctYmM0Mi01OTg2YmZjY2JiYmIiLCJzY29wZSI6ImhlYWx0aDpyZWFkIG9yZGVyczp3cml0ZSBvcmRlcnM6cmVhZCBkZXZpY2VzOnJlYWQgcGxhdGZvcm1zIG1lcmNoYW50czpyZWFkIiwibWVyY2hhbnQtaWQiOiI3NWVhODEzZC00Zjc4LTQxMDItOGJiNy1jMmIxNWU3MDQ5ZDgiLCJuYmYiOjE3MTM4NTM4MDcsImV4cCI6MTcxMzg1NzQwNywiaWF0IjoxNzEzODUzODA3fQ.uBjABuvnhZ9PUusKEJVvIZ5rrLCJmUztBNgVPuSqLHc",
  "token_type": "Bearer",
  "expires_in": 3599,
  "scope": "health:read orders:write orders:read devices:read platforms merchants:read"
}
```

**Tip:** Use the site https://www.jwt.io if you wish to inspect the contents of the `access_token`.

## API Actions
