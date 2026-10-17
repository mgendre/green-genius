---
name: architecture-tests
description: "How to write an ArchUnitNET rule in GreenGenius.ArchitectureTests that enforces an ADR — file, naming, the ADR label, shared conventions, the red proof. Read before adding or changing an architecture test."
---

# Architecture tests

`GreenGenius.ArchitectureTests/` enforces the backend ADRs of `docs/adr/` on the compiled assemblies with ArchUnitNET
(`TngTech.ArchUnitNET.xUnit`). Read the ADR the rule enforces first (skill `adr`).

## What can be a rule

| The ADR rule is about                                                                                                      | Architecture test?                                                                                                                                                |
|----------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| namespaces, type names, dependencies, implemented interfaces, visibility, static, member types, return and parameter types | yes                                                                                                                                                               |
| a namespace cycle                                                                                                          | yes, a slice rule; slices keep each sub-namespace apart, so a cycle between whole features is checked by `FeatureCycleFinder.cs` (`ApiFeaturesNamespacesTest.cs`) |
| the body of a method (a call, a lambda, an attribute on a lambda parameter, a query)                                       | no: say so in the report                                                                                                                                          |
| frontend, JSON files, migrations, startup behaviour                                                                        | no                                                                                                                                                                |

## Steps

1. One test class per ADR topic, `GreenGenius.ArchitectureTests/<Topic>Test.cs`, namespace
   `GreenGenius.ArchitectureTests`. Add to the existing class of that ADR when there is one.
2. One `[Fact]` per rule, named `<Subject>_Should<Rule>`.
3. Check against `Architecture.Loaded` (`Architecture.cs`): never load the assemblies again.
4. Every `IArchRule` ends with `.As("ADR NNNN: <the rule in a few words>")`, so a failure names the ADR. A slice rule
   (`SliceRuleDefinition`) has no `.As` in ArchUnitNET 0.13.4: its test name and class say the ADR.
5. A predicate or a pattern used by two test classes lives once in `ArchitectureConventions.cs` (`IsStatic`, the feature
   namespace pattern, the public `Handle` methods of a handler). Look there before writing one.
6. Namespaces are matched with anchored regexes:
   `ResideInNamespaceMatching(@"^GreenGenius\.Api\.Features\.[^.]+\.Dtos$")`. `new Namespace("…")` matches no
   sub-namespace: never use it.
7. Prove the rule red: run it on a temporary violation (a type in the wrong namespace, a forbidden `using`), see it fail
   with the violating type named, remove the violation, run it green. A temporary file is named `TempProbe*.cs` and is
   gone from the final diff.
8. A rule the current code breaks is never weakened, filtered or skipped: stop and report the violating types.

## Reference

A slice rule, as in `AppNamespacesTest.cs`:

```csharp
using ArchUnitNET.Fluent.Slices;
using ArchUnitNET.xUnit;

namespace GreenGenius.ArchitectureTests;

public class AppNamespacesTest
{
    [Fact]
    public void AppNamespaces_ShouldHaveNoCycle()
    {
        var rule = SliceRuleDefinition
            .Slices()
            .Matching("GreenGenius.App.(*)")
            .Should()
            .BeFreeOfCycles();

        rule.Check(Architecture.Loaded);
    }
}
```

A rule with a condition ArchUnitNET has no method for, as in `OwnershipAndHandlerSignatureTest.cs`:

```csharp
[Fact]
public void EntityWithOwnerId_ShouldImplementIHasOwner()
{
    IArchRule rule = ArchRuleDefinition
        .Classes()
        .That().ResideInNamespaceMatching(@"^GreenGenius\.Common\.Data\.Entities$")
        .Should().FollowCustomCondition(
            ImplementsIHasOwnerWhenItHasOwnerId,
            "implement IHasOwner when it has a property named OwnerId",
            "has a property named OwnerId without implementing IHasOwner")
        .As("ADR 0019: a user-scoped entity with an OwnerId property implements IHasOwner");

    rule.Check(Architecture.Loaded);
}
```

Braces `{ }` on every `if`, `else`, `for`, `foreach` and `while`, also around a single line.

## Never

- An `IArchRule` without `.As("ADR …")`, or naming a superseded ADR.
- A second `ArchLoader` or a copy of a predicate of `ArchitectureConventions.cs`.
- Production code changed to make a rule pass, unless the task asks for it.
