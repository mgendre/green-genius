# 0001. Record architecture decisions

Date: 2026-10-16

## Status

Accepted

## Context

Structural decisions were scattered across the convention files and the "Decided" lines of `.plans/` and `.tasks/`.
Agents and reviewers need one place to check code against and to stop reopening settled questions.

## Decision

- Every architecture decision is recorded as an ADR in `docs/adr/`, named `NNNN-<kebab-title>.md`, numbered from `0001`,
  in English, in the Michael Nygard format: Status, Context, Decision, Consequences.
- `docs/adr/README.md` indexes every ADR with its status.
- When an ADR and a convention file disagree, the ADR wins.
- An accepted ADR is never edited. A new ADR replaces it; the old one only changes its Status to `Superseded by NNNN`.

## Consequences

- A change that contradicts an accepted ADR needs a new ADR first.
- Convention files may restate an ADR for agents, but the ADR is the authority.
