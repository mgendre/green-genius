# 0021. Shared happy path in the test constructor

Date: 2026-10-17

## Status

Accepted

## Context

Unit tests with an in-memory database repeated the same arrangement in each test: the same bed, the same link, the same
ids. The repetition buried what each test is about, and a change of the data meant editing every test
(`BedsExtensionsTest`).

## Decision

- Data that two tests of a class arrange identically, the happy path, is built once in the constructor of the test
  class.
  xUnit builds a new instance per test, so each test still starts from its own empty database.
- The constructor arranges through its own context with the synchronous `SaveChanges()`, and keeps the ids and names the
  tests assert as `readonly` fields.
- A test arranges only what differs from the happy path (a foreign owner, a missing link) or what it checks; the call of
  the class under test still gets a new context, as before.
- Data used by a single test stays in that test.

## Consequences

- A test reads as its difference from the happy path.
- A test that needs an empty database arranges nothing in the constructor: such a class has no shared happy path.
- The rule applies to `GreenGenius.Api.Tests` only; integration tests already arrange through `Persist<Entity>Async`.
