# 🗄️ InsightERP — Database Guide

**Author:** InsightERP DevOps  
**Last Updated:** March 2026  
**Database:** Azure SQL Database (`insighterp_db`) on server `insighterp-sqlserver.database.windows.net`

---

## 🧠 How the Database is Designed

InsightERP uses a **single Azure SQL Database** (`insighterp_db`) shared across all microservices. Instead of one database per service, we use **SQL schemas** to logically isolate each microservice's tables.

| Schema | Microservice | Tables |
|---|---|---|
| `auth` | AuthService | `auth.users`, `auth.roles`, `auth.user_roles` |
| *(future)* `customer` | CustomerService | `customer.customers`, ... |
| *(future)* `order` | OrderService | `order.orders`, ... |

**Why one database?** Azure SQL charges per database. For a university project, one database with schemas gives us the same logical isolation for free.

**Key rule:** Every table in the codebase must always be prefixed with its schema. For example:
```sql
-- ✅ Correct
SELECT * FROM auth.users;

-- ❌ Wrong — ambiguous, will break if another service has a `users` table
SELECT * FROM users;
```

---

## 📁 Folder Structure for Database Scripts

```
schemas/
  auth/
    migrations/
      001_init.sql        ← Creates auth schema + initial tables
      002_test_migration.sql
      003_add_something.sql   ← New migrations go here
  customer/               ← (future) CustomerService schema
    migrations/
      001_init.sql
```

All `.sql` files are **T-SQL** (Azure SQL dialect) — NOT MySQL.

---

## ⚙️ How Migrations Work (The `schema_migrations` Tracking Table)

When the CD pipeline runs, it executes `scripts/apply_sqlserver_migrations.sh`. This script:

1. Creates the schema (e.g. `auth`) if it doesn't already exist.
2. Creates a `schema_migrations` **tracking table** inside that schema (e.g. `auth.schema_migrations`).
3. Loops through every `.sql` file in order (`001_`, `002_`, etc.).
4. **Skips** files already recorded in `auth.schema_migrations`.
5. Runs new files and records them.

This means **data is NEVER deleted on deployment** — only new SQL files are applied.

---

## 🚀 How to Run Locally

### Prerequisites
- [Azure Data Studio](https://learn.microsoft.com/en-us/azure-data-studio/download-azure-data-studio) or [SSMS](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (to browse the Azure DB)
- `sqlcmd` installed — [download here](https://learn.microsoft.com/en-us/sql/tools/sqlcmd/sqlcmd-utility)
- The 4 Azure SQL credentials (ask the DevOps lead)

### Running migrations locally against Azure

```powershell
# Set the required environment variables first
$env:AZURE_SQL_SERVER   = "insighterp-sqlserver.database.windows.net"
$env:AZURE_SQL_DATABASE = "insighterp_db"
$env:AZURE_SQL_USER     = "your_username"
$env:AZURE_SQL_PASSWORD = "your_password"

# Run the auth schema migrations
bash ./scripts/apply_sqlserver_migrations.sh schemas/auth/migrations
```

> [!NOTE]
> You can also run the `.sql` files directly in Azure Data Studio by connecting to `insighterp-sqlserver.database.windows.net` and selecting `insighterp_db`.

---

## ☁️ How it Runs in Azure (CI/CD Pipeline)

Every time code is pushed to the `dev` branch, the CD pipeline (`.github/workflows/cd-dev.yml`) automatically:

1. Installs `mssql-tools18` (which provides `sqlcmd`) on the GitHub Actions runner.
2. Reads Azure SQL credentials from **GitHub Secrets**.
3. Calls `bash ./scripts/apply_sqlserver_migrations.sh schemas/auth/migrations`.
4. The script creates the `auth` schema and applies any new `.sql` files.

**GitHub Secrets used:**
| Secret | Purpose |
|---|---|
| `AZURE_SQL_SERVER` | `insighterp-sqlserver.database.windows.net` |
| `AZURE_SQL_DATABASE` | `insighterp_db` |
| `AZURE_SQL_USER` | SQL admin login username |
| `AZURE_SQL_PASSWORD` | SQL admin login password |
| `AUTH_DB_CONNECTION_STRING_AZURE` | Full ADO.NET connection string for the AuthService container app |

---

## ➕ How to Add a New Schema (New Microservice)

When a new microservice (e.g. `CustomerService`) needs a database, follow these steps:

**1. Create the folder:**
```
schemas/
  customer/
    migrations/
      001_init.sql
```

**2. Write the initial migration in T-SQL, scoped to the schema:**
```sql
-- schemas/customer/migrations/001_init.sql
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'customer')
BEGIN
    EXEC('CREATE SCHEMA [customer]');
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[customer].[customers]') AND type = N'U')
BEGIN
    CREATE TABLE customer.customers (
        id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        name NVARCHAR(255) NOT NULL,
        created_at DATETIME NOT NULL DEFAULT GETUTCDATE()
    );
END
```

**3. Add a new migration step to the CD pipeline** (`.github/workflows/cd-dev.yml`):
```yaml
- name: Apply DB migrations (CustomerService)
  ...
  run: |
    export PATH="$PATH:/opt/mssql-tools18/bin"
    bash ./scripts/apply_sqlserver_migrations.sh schemas/customer/migrations
```

**4. Add a `ConnectionStrings__CustomerDb` secret** to GitHub Secrets and configure the container app to receive it (same pattern as `authservice-dev`).

---

## ✏️ How to Make a Database Change (Adding Columns, Tables, etc.)

> [!IMPORTANT]
> **NEVER edit an existing migration file.** Once a file has been applied to the database, the system records it. Editing it will have NO effect on the next deployment — the script will simply skip it.

**ALWAYS create a new numbered `.sql` file:**

```
schemas/auth/migrations/
  001_init.sql             ✅ Already applied — DO NOT touch
  002_test_migration.sql   ✅ Already applied — DO NOT touch
  003_add_phone_number.sql ← ✅ NEW — this will be applied on next push
```

**Example new migration file:**
```sql
-- schemas/auth/migrations/003_add_phone_number.sql
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[auth].[users]') AND name = 'phone_number'
)
BEGIN
    ALTER TABLE auth.users ADD phone_number NVARCHAR(20) NULL;
END
```

Once you push this file to `dev`, the CD pipeline will detect it as a new migration and apply it automatically — without touching any existing data.

---

## 🔍 Verifying the Database (Azure Data Studio)

1. Connect to `insighterp-sqlserver.database.windows.net` using your SQL credentials.
2. Select the `insighterp_db` database.
3. Expand **Schemas** to see `auth`, `customer`, etc.
4. Expand **Tables** inside each schema to see the tables.
5. To verify which migrations have been applied, run:
```sql
SELECT * FROM auth.schema_migrations ORDER BY applied_at;
```

---

## 📋 Quick Reference Cheatsheet

| Task | How |
|---|---|
| Apply migrations locally | `bash ./scripts/apply_sqlserver_migrations.sh schemas/auth/migrations` |
| Make a DB change | Create `schemas/auth/migrations/00N_description.sql` with T-SQL |
| Add a new microservice schema | Create `schemas/newservice/migrations/001_init.sql` + update CD pipeline |
| Browse DB live | Azure Data Studio → connect to `insighterp-sqlserver.database.windows.net` |
| Check migration history | `SELECT * FROM auth.schema_migrations ORDER BY applied_at;` |
| Never do this | Edit an existing migration file |

---

*Documentation maintained by InsightERP DevOps — March 2026*
