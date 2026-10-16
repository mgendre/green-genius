#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

tasksDir=".tasks"
plansDir=".plans"
lockFile="$tasksDir/.lock"
pollSeconds="${POLL_SECONDS:-30}"
runPid=""
declare -A setAsidePlans=()

mkdir -p "$tasksDir" "$plansDir"

exec 9>"$lockFile"
if ! flock -n 9; then
    echo "Another dispatch.sh is already running: stop it first."
    exit 1
fi

cleanUp() {
    if [[ -n "$runPid" ]]; then
        kill -- "-$runPid" 2>/dev/null || true
        wait "$runPid" 2>/dev/null || true
    fi
    rm -f "$lockFile"
}

trap cleanUp EXIT
trap 'exit 130' INT TERM

log() {
    echo "[$(date '+%F %T')] $*"
}

queuedFiles() {
    find "$1" -maxdepth 1 -name '[0-9][0-9][0-9]_*.md' -printf '%f\n' | LC_ALL=C sort
}

runDispatcher() {
    local title="$1"
    local message="$2"
    local startedAt=$SECONDS
    local exitCode=0

    log "Start $title"
    setsid opencode run --standalone --agent dispatcher --title "$title" "$message" &
    runPid=$!
    wait "$runPid" || exitCode=$?
    runPid=""
    log "End $title: exit $exitCode, $((SECONDS - startedAt))s"
}

nextTask() {
    queuedFiles "$tasksDir" | head -n 1
}

isBlocked() {
    grep -q '^## .*\[!\]' "$1"
}

nextPlan() {
    local name path
    while read -r name; do
        path="$plansDir/$name"
        if isBlocked "$path"; then
            continue
        fi
        if [[ "${setAsidePlans[$path]:-}" == "$(sha256sum < "$path")" ]]; then
            continue
        fi
        echo "$name"
        return
    done < <(queuedFiles "$plansDir")
}

runTask() {
    local name="$1"
    runDispatcher "task $name" "$(cat "$tasksDir/$name")"
    mv "$tasksDir/$name" "$tasksDir/done_$name"
}

runPlan() {
    local name="$1"
    local path="$plansDir/$name"
    local shaBefore
    shaBefore="$(sha256sum < "$path")"

    runDispatcher "plan $name" "Continue $path."

    if [[ ! -f "$path" ]]; then
        return
    fi
    if isBlocked "$path" || [[ "$(sha256sum < "$path")" == "$shaBefore" ]]; then
        setAsidePlans[$path]="$(sha256sum < "$path")"
        log "Set aside $path: blocked or no progress"
    fi
}

waiting=false
while true; do
    task="$(nextTask)"
    if [[ -n "$task" ]]; then
        waiting=false
        runTask "$task"
        continue
    fi

    plan="$(nextPlan)"
    if [[ -n "$plan" ]]; then
        waiting=false
        runPlan "$plan"
        continue
    fi

    if [[ "$waiting" == false ]]; then
        log "Waiting for work…"
        waiting=true
    fi
    sleep "$pollSeconds"
done
