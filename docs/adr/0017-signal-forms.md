# 0017. Signal Forms only

Date: 2026-10-16

## Status

Accepted

## Context

Forms must share one model, one validation style and one way to submit.

## Decision

- Forms use Angular Signal Forms (`@angular/forms/signals`) only.
- No reactive or template-driven forms from `@angular/forms`.

## Consequences

- One form pattern to copy for every new form.
