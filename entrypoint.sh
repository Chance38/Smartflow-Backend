#!/bin/sh
set -e

# Try to scaffold a migration named "auto-update". If there are no model changes
# or the command fails for any reason, ignore the error and continue. This
# prevents the container from exiting while still auto-creating a migration
# when necessary.
# The project paths match how the container mounts the repo (/src/...)

dotnet ef migrations add "auto-update" \
  --project /src/SmartFlowBackend.Infrastructure \
  --startup-project /src/SmartFlowBackend.Application/SmartFlowBackend.Application.csproj || true

# It can take a few seconds for the database to be ready, so we loop until the command succeeds
until dotnet ef database update \
  --project /src/SmartFlowBackend.Infrastructure \
  --startup-project /src/SmartFlowBackend.Application/SmartFlowBackend.Application.csproj; do
  >&2 echo "Postgres is unavailable - sleeping"
  sleep 1
done

>&2 echo "Postgres is up - executing command"
exec "$@"
