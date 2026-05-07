#!/usr/bin/env bash
# Lumen — WebGL batch-mode build wrapper.
#
# Discovers the latest 2022.3.x Unity install on macOS, then invokes
# WebGLBuilder.BuildWebGL in batch mode. Logs go to build-webgl.log
# in the repo root. Exits with Unity's exit code so CI can fail loud.
set -uo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_PATH="${REPO_ROOT}/unity"
LOG_FILE="${REPO_ROOT}/build-webgl.log"

# Discover latest Unity 2022.3.x installation.
UNITY_BIN=$(ls -d /Applications/Unity/Hub/Editor/2022.3.*/Unity.app/Contents/MacOS/Unity 2>/dev/null | sort -V | tail -n1)

if [[ -z "${UNITY_BIN}" || ! -x "${UNITY_BIN}" ]]; then
  echo "ERROR: No Unity 2022.3.x installation found under /Applications/Unity/Hub/Editor/" >&2
  echo "       Install Unity 2022.3 LTS via Unity Hub, then retry." >&2
  exit 127
fi

echo "Using Unity: ${UNITY_BIN}"
echo "Project:     ${PROJECT_PATH}"
echo "Log:         ${LOG_FILE}"
echo

"${UNITY_BIN}" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "${PROJECT_PATH}" \
  -buildTarget WebGL \
  -executeMethod WebGLBuilder.BuildWebGL \
  -logFile "${LOG_FILE}"

EC=$?
echo
echo "Unity exit code: ${EC}"
exit ${EC}
