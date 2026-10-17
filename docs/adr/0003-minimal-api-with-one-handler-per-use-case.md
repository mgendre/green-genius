# 0003. Minimal API with one handler per use case

Date: 2026-10-16

## Status

Accepted

## Context

Minimal API keeps endpoints close to the framework, but logic written inside route lambdas is hard to test and to read.
A controller or a mediator library would add a layer without a consumer.

## Decision

- Each feature has one static `<Feature>Api` exposing `With<Feature>Api(this IEndpointRouteBuilder)`, with one
  `MapGroup(RouteConstants.<Feature>)` carrying `.WithTags`, and routes `"/"` and `"/{id:guid}"`.
- A route lambda only binds its input and calls the handler. Every parameter carries `[FromRoute]`, `[FromBody]` or
  `[FromServices]`.
- One handler class per use case, in `GreenGenius.Api.Features.<Feature>.Handlers`, named `<UseCase>Handler`,
  implementing `IApiHandler`. Handlers are registered scoped by the assembly scan of `Infra.Hosting`, never by hand.
- A handler has one public `Handle` returning `TypedResults`; its signature lists only the success results
  (`Task<Ok<GardenDto>>`, `Task<Created<GardenDto>>`, `Task<NoContent>`).
- `Created` is `TypedResults.Created((string?)null, dto)`; a `Location` is set only when a `GET /{id}` already exists.

## Consequences

- No MediatR, no controllers, no manual DI registration of handlers.
- Error results are not part of the handler signature (ADR 0005).
- The handler naming and namespace rule is enforced by the architecture tests (ADR 0012).
