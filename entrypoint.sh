#!/bin/sh
set -e

# It can take a few seconds for the database to be ready, so we loop until the command succeeds
until dotnet ef database update --project /src/SmartFlowBackend.Infrastructure --startup-project /src/SmartFlowBackend.Application/SmartFlowBackend.Application.csproj; do
>&2 echo "Postgres is unavailable - sleeping"
sleep 1
done

>&2 echo "Postgres is up - executing command"
exec "$@"
