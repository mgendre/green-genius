---
name: lookup
description: "Where to find an answer: in this project's code or in the documentation servers (angular-cli, spartan-ui, context7), and which server for which library. Read before answering a question about the project or the stack, and before a design choice that depends on what a library can do."
---

# Lookup

An answer comes from one of two sources. Decide which before any tool call, and never mix the two in one lookup.

| The question | Kind | Source |
|---|---|---|
| is about what this repository contains or does: where something is, how a feature works here, which file, handler, store, route or text, why a piece of code behaves as it does | **Project** | the code, with `glob`, `grep` and `read` |
| is about how a library, a framework or a tool works or should be used, whatever the project does | **Docs** | the documentation servers |
| needs both: "how do we do X here and is it the recommended way" | **Both** | the code first, then the docs |

"Our", "here", "this app", a file path or a name that exists only in this project (`GardenDto`, `gardens.store.ts`) mean **Project**. A library name, a component of a library or "how do I…" with no reference to the project mean **Docs**. When you cannot tell, ask which one in one sentence instead of guessing.

## Project

- `grep` or `glob` first to find the place, then `read` only the part that answers: `offset` and `limit` around the match, never a whole file of more than 200 lines, never the generated client or `node_modules`.
- Never use a documentation server for it.
- Every fact you state carries its `path:line`. What you did not read, you do not state.

## Docs

Pick the server from the subject. One subject, one server: never ask Angular's server about Spartan, never ask Spartan's server about Tailwind. Never read the project's code for it.

| The question is about | Server | Tool |
|---|---|---|
| Angular: components, signals, Signal Forms, routing, templates, `@angular/*` | `angular-cli` | `tools["angular-cli"].search_documentation`, or `tools["angular-cli"].get_best_practices` for "what is the right way to…" |
| Spartan UI: any `hlm` or `brn` component, dialog, button, field, input, card | `spartan-ui` | `tools["spartan-ui"].spartan_components_get` with the component name; `tools["spartan-ui"].spartan_components_list` when you do not know the name; `tools["spartan-ui"].spartan_docs_get` for installation and theming |
| Tailwind: classes, sizes, breakpoints, `@theme`, colours | `context7` | library `tailwindcss` |
| NgRx SignalStore | `context7` | library `@ngrx/signals` |
| ngx-translate | `context7` | library `ngx-translate` |
| ng-icons, Lucide icon names | `context7` | library `ng-icons` |
| .NET, ASP.NET Minimal API, EF Core, PostgreSQL | `context7` | library named in the question |
| Anything else | `context7` | library named in the question |

- These are not direct tools: call them inside the `execute` tool, as `return await tools["<server>"].<tool>({ ... })`, with the arguments the tool catalog of your instructions gives. A name the catalog does not show: `search({ namespace: "<server>" })` inside `execute`, never a guess.
- With `context7`, two calls: first resolve the library name to its id, then ask the docs of that id your question.
- Two subjects take one call per subject: "widen a Spartan dialog" is the Spartan dialog component first, then Tailwind for the width class.
- Look it up before answering, never from memory. Stop as soon as you have the answer.
- The lookup found nothing: say so in one sentence. Never invent a name, an option, an import or a path.

## Say where it comes from

Name the source with what you report: `project`, `docs (<server or library>)` or `project + docs`. When the project and the docs differ, say so plainly.
