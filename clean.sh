#!/usr/bin/env sh
set -eu

SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)

for project in ClientPlugin DedicatedPlugin TorchPlugin; do
    rm -rf "$SCRIPT_DIR/$project/bin" "$SCRIPT_DIR/$project/obj"
done
