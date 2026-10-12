#!/usr/bin/env bash
set -euo pipefail

project="mgendre_green-genius"
api="https://sonarcloud.io/api"
scope="${1:+&pullRequest=$1}"

{
  curl -sf "$api/hotspots/search?projectKey=$project&status=TO_REVIEW&ps=500$scope"
  curl -sf "$api/issues/search?componentKeys=$project&resolved=false&ps=500$scope"
} | python3 -c '
import json, sys

decoder = json.JSONDecoder()
text = sys.stdin.read()
hotspots, position = decoder.raw_decode(text)
issues, _ = decoder.raw_decode(text[position:].lstrip())

severities = ["BLOCKER", "CRITICAL", "MAJOR", "MINOR", "INFO"]
types = ["HOTSPOT", "VULNERABILITY", "BUG", "CODE_SMELL"]
skipped = ("/shared/spartan/helm/", "/Migrations/", "api-client.generated.ts")

rows = []
for hotspot in hotspots["hotspots"]:
    rows.append(("BLOCKER", "HOTSPOT", hotspot["ruleKey"], hotspot["component"], hotspot.get("line"), hotspot["message"]))
for issue in issues["issues"]:
    rows.append((issue["severity"], issue["type"], issue["rule"], issue["component"], issue.get("line"), issue["message"]))

rows.sort(key=lambda row: (severities.index(row[0]), types.index(row[1]), row[3], row[4] or 0))
for severity, kind, rule, component, line, message in rows:
    path = component.split(":", 1)[1]
    action = "SKIP" if any(part in path for part in skipped) else "FIX "
    print(f"{action} {severity} {kind} {rule}  {path}:{line or 1}  {message}")
print(f"{len(rows)} open")
'
