# 0015. Spartan Helm components

Date: 2026-10-16

## Status

Accepted

## Context

UI components must behave the same everywhere and stay upgradable with Spartan UI.

## Decision

- UI components are Spartan UI.
- Helm components are generated with `npm run ui --prefix GreenGenius.Frontend -- --name=<component>` into
  `src/app/shared/spartan/helm/`, never written from scratch nor edited by hand.

## Consequences

- A missing Helm component is generated, not copied or rewritten.
- Their look is changed through tokens and classes, not in the generated code.
