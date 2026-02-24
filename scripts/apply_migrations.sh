#!/usr/bin/env bash
set -euo pipefail

: "${MYSQL_HOST?Need MYSQL_HOST}"
: "${MYSQL_DB?Need MYSQL_DB}"
: "${MYSQL_USER?Need MYSQL_USER}"
: "${MYSQL_PASSWORD?Need MYSQL_PASSWORD}"

MIGRATIONS_DIR="${1:-db/auth/migrations}"

echo "==> Using migrations folder: $MIGRATIONS_DIR"
echo "==> Target: $MYSQL_HOST / $MYSQL_DB / $MYSQL_USER"

mysql -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DB" <<'SQL'
CREATE TABLE IF NOT EXISTS schema_migrations (
  id INT NOT NULL AUTO_INCREMENT,
  filename VARCHAR(255) NOT NULL UNIQUE,
  applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id)
);
SQL

shopt -s nullglob
files=("$MIGRATIONS_DIR"/*.sql)

if [ ${#files[@]} -eq 0 ]; then
  echo "No .sql files found in $MIGRATIONS_DIR"
  exit 0
fi

for f in "${files[@]}"; do
  base="$(basename "$f")"
  applied=$(mysql -N -s -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DB" \
    -e "SELECT COUNT(*) FROM schema_migrations WHERE filename='${base}';")

  if [ "$applied" = "1" ]; then
    echo "SKIP: $base (already applied)"
    continue
  fi

  echo "APPLY: $base"
  mysql -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DB" < "$f"

  mysql -h "$MYSQL_HOST" -u "$MYSQL_USER" -p"$MYSQL_PASSWORD" "$MYSQL_DB" \
    -e "INSERT INTO schema_migrations(filename) VALUES('${base}');"

  echo "DONE: $base"
done

echo "All migrations applied."