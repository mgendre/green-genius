# 0010. Reference data imported at startup from versioned JSON files

Date: 2026-10-16

## Status

Accepted

## Context

Plants and their associations are reference data shared by every user. They must be reviewable in the repository and
reach every environment without a manual step.

## Decision

- Plants live in `GreenGenius.App/Data/Plants/<key>.json`, one file per plant; associations in
  `GreenGenius.App/Data/Associations/*.json`. Both folders are copied to the build output with `CopyToOutputDirectory`
  `Always`.
- At startup, after `MigrateAsync`, `ImportDataAsync` runs `PlantImporter` then `PlantAssociationImporter`, unless the
  setting `PlantsImporterEnabled` is false (default true).
- A plant is matched by its `Key`, the file name; an existing plant is updated only when the file's `Version` is higher
  than the stored one.
- Associations are synchronised: each pair in the files is inserted or updated, a stored pair absent from the files is
  deleted.
- Each importer saves once. A file that cannot be read is logged and skipped; the rest of the import goes on.
- Integration tests check that the importers are called at startup, never the content of the data files.

## Consequences

- Changing reference data is a JSON change in a pull request; a changed plant file needs a higher `Version`.
- Startup time grows with the data set.
