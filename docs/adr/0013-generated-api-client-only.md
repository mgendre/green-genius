# 0013. The generated API client is the only access to the backend

Date: 2026-10-16

## Status

Accepted

## Context

Handwritten models and HTTP calls in the front end drift from the backend contract without any compiler noticing.

## Decision

- The Debug build of `GreenGenius.App` writes `openapi/GreenGenius.App.json` and regenerates
  `GreenGenius.Frontend/src/app/api/api-client.generated.ts` with NSwag.
- Both generated files are never edited by hand.
- The front end calls the backend only through `Client` and the types of that file: no handwritten model, no
  `HttpClient` call. Base URL `/api`, proxied by `proxy.conf.json` in development.

## Consequences

- A backend contract change shows up as a TypeScript compile error in the front end.
- A backend change that alters the contract ships its regenerated client in the same change.
