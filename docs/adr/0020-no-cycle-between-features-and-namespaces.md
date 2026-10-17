# 0020. No cycle between features and namespaces

Date: 2026-10-16

## Status

Accepted

Supersedes 0018

## Context

ADR 0018 forbade every namespace cycle, including inside a feature of the API. There, the feature root (`<Feature>Api`,
`<Feature>Extensions`) calls its `.Handlers`, and the handlers use the root's mapping: a cycle by design, which keeps a
feature in one folder. SonarCloud does not report it. ArchUnitNET slices cannot treat a feature and its sub-namespaces
as one node.

## Decision

- No dependency cycle between the namespaces of `GreenGenius.App` (`GreenGenius.App.(*)`) nor of
  `GreenGenius.Common.Data`.
- No dependency cycle between the features of the API: a feature, its root namespace and all its sub-namespaces, is one
  node of the graph. Inside one feature, the root and its `.Handlers` and `.Dtos` may depend on each other.
- A helper used by two namespaces lives in a namespace of its own (`GreenGenius.App.Json`), never in one of its callers.
- `GreenGenius.ArchitectureTests` enforces it: slice rules (`BeFreeOfCycles`) for App and Common.Data,
  `ApiFeaturesNamespacesTest` with `FeatureCycleFinder` for the features.

## Consequences

- A new dependency that closes a loop fails the build's tests, before SonarCloud sees it.
- A feature stays in one folder; a cross-feature dependency goes one way only.
