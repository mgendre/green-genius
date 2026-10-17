# Architecture Decision Records

Michael Nygard format (Status, Context, Decision, Consequences), one file per decision, `NNNN-<kebab-title>.md`; ADR
0001 sets the rules.

| ADR                                                           | Title                                                        | Status             |
|---------------------------------------------------------------|--------------------------------------------------------------|--------------------|
| [0001](0001-record-architecture-decisions.md)                 | Record architecture decisions                                | Accepted           |
| [0002](0002-layered-solution-and-dependency-direction.md)     | Layered solution and dependency direction                    | Accepted           |
| [0003](0003-minimal-api-with-one-handler-per-use-case.md)     | Minimal API with one handler per use case                    | Accepted           |
| [0004](0004-entities-never-exposed-dtos-projected.md)         | Entities never exposed, DTOs projected in the database       | Accepted           |
| [0005](0005-not-found-through-data-not-found-exception.md)    | Not found through DataNotFoundException                      | Accepted           |
| [0006](0006-ownership-through-query-filter.md)                | Ownership through IHasOwner and a query filter               | Superseded by 0019 |
| [0007](0007-migrations-generated-and-applied-at-startup.md)   | Migrations generated, never edited, applied at startup       | Accepted           |
| [0008](0008-no-cancellation-token-and-no-validation-layer.md) | No CancellationToken in handlers, no input validation layer  | Accepted           |
| [0009](0009-enums-stored-as-text.md)                          | Enums stored as text by one EF convention                    | Accepted           |
| [0010](0010-reference-data-imported-from-json-at-startup.md)  | Reference data imported at startup from versioned JSON files | Accepted           |
| [0011](0011-plant-associations-one-stored-pair-one-dto.md)    | Plant associations: one stored pair, one DTO                 | Accepted           |
| [0012](0012-architecture-tests-with-archunitnet.md)           | Architecture tests with ArchUnitNET                          | Accepted           |
| [0013](0013-generated-api-client-only.md)                     | The generated API client is the only access to the backend   | Accepted           |
| [0014](0014-signalstore-for-state.md)                         | NgRx SignalStore for state                                   | Accepted           |
| [0015](0015-spartan-helm-components.md)                       | Spartan Helm components                                      | Accepted           |
| [0016](0016-tailwind-design-tokens.md)                        | Tailwind design tokens                                       | Accepted           |
| [0017](0017-signal-forms.md)                                  | Signal Forms only                                            | Accepted           |
| [0018](0018-no-namespace-cycles.md)                           | No namespace cycles                                          | Superseded by 0020 |
| [0019](0019-owner-query-filters-in-the-db-context.md)         | Owner query filters in the DbContext                         | Accepted           |
| [0020](0020-no-cycle-between-features-and-namespaces.md)      | No cycle between features and namespaces                     | Accepted           |
| [0021](0021-shared-happy-path-in-the-test-constructor.md)     | Shared happy path in the test constructor                    | Accepted           |
