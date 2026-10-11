#!/usr/bin/env bash
set -euo pipefail

number="${1:-$(gh pr view --json number --jq .number)}"

gh api graphql -F owner='{owner}' -F repo='{repo}' -F number="$number" -f query='
query($owner: String!, $repo: String!, $number: Int!) {
  repository(owner: $owner, name: $repo) {
    pullRequest(number: $number) {
      number
      commits(last: 1) { nodes { commit { committedDate } } }
      reviewThreads(first: 100) { nodes { isResolved isOutdated path line originalLine comments(first: 20) { nodes { author { login } authorAssociation body } } } }
      comments(last: 50) { nodes { author { login } authorAssociation body createdAt } }
      reviews(last: 30) { nodes { author { login } authorAssociation body submittedAt } }
    }
  }
}' | python3 -c '
import json, sys

trusted = ("OWNER", "MEMBER", "COLLABORATOR")
pull_request = json.load(sys.stdin)["data"]["repository"]["pullRequest"]
last_commit = pull_request["commits"]["nodes"][0]["commit"]["committedDate"]


def is_trusted(comment):
    return comment["authorAssociation"] in trusted


def flatten(text):
    line = " ".join(text.split())
    return line if len(line) <= 500 else line[:500] + " [cut]"


def author(comment):
    return (comment["author"] or {}).get("login", "ghost")


rows = []
resolved = older = untrusted = 0

for thread in pull_request["reviewThreads"]["nodes"]:
    comments = thread["comments"]["nodes"]
    if thread["isResolved"]:
        resolved += 1
        continue
    if not comments or not is_trusted(comments[0]):
        untrusted += 1
        continue
    body = " >> ".join(flatten(comment["body"]) for comment in comments if is_trusted(comment))
    path = thread["path"]
    line = thread["line"] or thread["originalLine"] or 1
    action = "MOVED" if thread["isOutdated"] else "FIX  "
    rows.append((path, line, f"{action} {path}:{line}  {author(comments[0])}  {flatten(body)}"))

general = [(comment, comment["createdAt"]) for comment in pull_request["comments"]["nodes"]]
general += [(review, review["submittedAt"]) for review in pull_request["reviews"]["nodes"] if review["body"].strip()]
for comment, written in general:
    if not is_trusted(comment):
        untrusted += 1
        continue
    if written is None or written < last_commit:
        older += 1
        continue
    body = flatten(comment["body"])
    rows.append(("~", 0, f"FIX   (general)  {author(comment)}  {body}"))

for _, _, text in sorted(rows, key=lambda row: (row[0], row[1])):
    print(text)
number = pull_request["number"]
print(f"PR {number}: {len(rows)} open, {resolved} resolved, {older} general older than the last commit, {untrusted} from other people ignored")
'
