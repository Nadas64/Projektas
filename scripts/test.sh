#!/bin/bash

dotnet tool restore
rm -rf backend.tests/coverage/raw
dotnet test

if [ "$1" = "report" ]; then
    dotnet reportgenerator -reports:"backend.tests/coverage/raw/*/coverage.cobertura.xml" -targetdir:backend.tests/coverage/report
    FILE="backend.tests/coverage/report/index.html"
    SCRIPT_DIR="$(dirname "$0")"
    TARGET_PATH="$SCRIPT_DIR/../$FILE"

    if [[ "$OSTYPE" == "darwin"* ]]; then
        open "$TARGET_PATH"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        xdg-open "$TARGET_PATH"
    elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
        start "" "$TARGET_PATH"
    fi
fi
