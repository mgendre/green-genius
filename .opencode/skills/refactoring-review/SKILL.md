---
name: refactoring-review
description: "Propose refactorings on a diff or the whole repo — merge what overlaps (duplicates, sibling services or components doing the same job) and split what has grown too big (handlers, endpoint groups, stores, components with several responsibilities). Cross-checks SonarCloud. Returns concrete proposals, not findings."
---

# Refactoring review

Run it yourself, read-only, on the scope (branch diff, or every `GreenGenius.*` project for a periodic pass), against `AGENTS.md`, Sonar's duplicated-blocks and cognitive-complexity lists when available (https://sonarcloud.io/organizations/mgendre/projects), and the rules below. The user decides which proposals are built.

The review never edits code.

## Lens 1 — Merge

Signals:
- Identical or near-identical blocks of 5+ lines.
- Same shape, different names: two handlers with load → check → save; two components differing by one input.
- One rule written twice: a limit in the request record and in the form; an enum mirrored by hand; a route string typed in two places.
- Two classes sharing most of their dependencies and touching the same entities.

Ignore framework boilerplate, i18n files, migrations, generated code, two-line similarities.

## Lens 2 — Split

Signals:
- File over ~200 lines, method over ~30 lines, cognitive complexity over 15.
- A class with more than 5 injected dependencies, or private methods that form two clusters touching different entities.
- A store holding unrelated state slices, or a component with more than ~6 inputs or two unrelated outputs.
- An endpoint group mapping routes that belong to two features.
- A name with "And", "Manager", "Helper", "Utils".
- SCSS: two classes in `styles/` sharing most declarations, a component `.scss` file holding rules that belong in `styles/`, or a modifier chain that outgrew its base.

## Proposal rules

One proposal per case. It names the target files and new names, keeps the simplest mechanism (function before base class, `Common/` helper before new project), extracts as soon as a block appears twice, and gives each new unit one job and one name. Never propose a split that creates a unit with a single caller and no reason of its own to change.

## Report

Write in the user's language. Lead with the proposal, then the evidence.

```
## Proposals (ranked by impact)
1. Merge <A> and <B> into `<Name>` in <file> — they share <what>. Effort S | M | L.
   - <file>:<range>, <file>:<range>
2. Split `<X>` (<n> lines, <m> dependencies) into `<X1>` (<job>) and `<X2>` (<job>). Effort M.
   - <file>:<range>
## Sonar: duplication <x %> (gate < 3 %), complexity hot spots <file:line> | not available
## Left as is: <file> — <reason>
```
