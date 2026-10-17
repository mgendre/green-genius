# 0011. Plant associations: one stored pair, one DTO

Date: 2026-10-16

## Status

Accepted

## Context

An association links two plants without a direction. Storing it twice, or shaping it differently per endpoint,
duplicated rows, DTOs and projections.

## Decision

- A pair is stored once: `PlantA` is the plant whose `Key` comes first in ordinal order. The table has a unique index on
  (`PlantAId`, `PlantBId`); queries look at both columns.
- An association carries an `Effect`, a list of `Mechanism` values, an `Evidence` and a `NoteFr`. No rule ties a
  mechanism to an effect.
- One `PlantAssociationDto` (`PlantA`, `PlantB` as `PlantSummaryDto`, `Effect`, `Mechanisms` sorted by name, `Evidence`,
  `NoteFr`) and one projection `PlantsExtensions.AsAssociationDto` serve both `GET /plants/{id}/associations` and
  `GET /gardens/{gardenId}/beds/{id}/associations`.
- The API returns both plants as stored; the client picks the other plant itself.

## Consequences

- No per-endpoint association DTO (`BedAssociationDto` is gone) and no `plantId`-dependent projection.
- The front end holds the "other plant" logic.
