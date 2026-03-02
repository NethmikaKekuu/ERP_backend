# InsightERP — Backend

A .NET microservices backend for the InsightERP platform, using an Ocelot API Gateway, MySQL via Docker, and a React + Vite frontend.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 9, ASP.NET Core Web API |
| API Gateway | Ocelot |
| Auth | JWT Bearer tokens |
| Database | MySQL 8 (Docker) |
| Frontend | React + Vite |
| Containerization | Docker / Docker Compose |
| CI/CD | GitHub Actions |
| Cloud | Azure Container Apps |

---

## Architecture

```
Frontend (Vite :5173)
        │
        ▼
API Gateway (:5000)   ← single entry point for all requests
        │
        ├── AuthService        (:5001)
        ├── CustomerService    (:5002)
        ├── OrderService       (:5003)
        ├── ProductService     (:5004)
        ├── ForecastService    (:5005)
        ├── PredictionService  (:5006)
        └── AnalyticsService   (:5007)
                │
                ▼
         MySQL DB (:3307)  ← via Docker
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Node.js](https://nodejs.org/) (for the frontend)

---

## 🚀 Starting the System

### Option 1 — Login / Auth only (minimal setup)

Use this when you only need the login page and auth to work.

**Step 1 — Start the database**
```powershell
# Run from the repo root
docker compose up -d
```

**Step 2 — Start the AuthService**
```powershell
dotnet run --project src/AuthService
# Runs on http://localhost:5001
```

**Step 3 — Start the API Gateway**
```powershell
dotnet run --project src/ApiGateway
# Runs on http://localhost:5000
```

**Step 4 — Start the frontend**
```powershell
# Run from the ERP_frontend folder
npm run dev
# Runs on http://localhost:5173
```

You can now log in with the pre-seeded test accounts:

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `manager` | `Manager@123` | Manager |
| `employee` | `Employee@123` | Employee |

---

### Option 2 — Full system (all microservices)

Use this when you need all services running at once.

**Step 1 — Start the database**
```powershell
docker compose up -d
```

**Step 2 — Start all microservices + gateway**
```powershell
# Run from the repo root
.\scripts\run-all-services.ps1
```
This opens each service in its own PowerShell window.

**Step 3 — Start the frontend**
```powershell
# Run from the ERP_frontend folder
npm run dev
```

---

## 🛑 Stopping the System

**Stop all microservices:**
```powershell
.\scripts\stop-all-services.ps1
```

**Stop the database:**
```powershell
docker compose down
```

> Use `docker compose down -v` to also **delete all database data** (destructive ⚠️).

---

## Service URLs

| Service | Local URL | Swagger |
|---|---|---|
| API Gateway | http://localhost:5000 | — |
| AuthService | http://localhost:5001 | http://localhost:5001/swagger |
| CustomerService | http://localhost:5002 | http://localhost:5002/swagger |
| OrderService | http://localhost:5003 | http://localhost:5003/swagger |
| ProductService | http://localhost:5004 | http://localhost:5004/swagger |
| ForecastService | http://localhost:5005 | http://localhost:5005/swagger |
| PredictionService | http://localhost:5006 | http://localhost:5006/swagger |
| AnalyticsService | http://localhost:5007 | http://localhost:5007/swagger |
| Frontend | http://localhost:5173 | — |

---

## Gateway Health Checks

```
http://localhost:5000/auth/health
http://localhost:5000/customer/health
http://localhost:5000/order/health
http://localhost:5000/product/health
http://localhost:5000/forecast/health
http://localhost:5000/prediction/health
http://localhost:5000/analytics/health
```

---

## CI/CD

GitHub Actions pipelines are configured in `.github/workflows/`. Pushes to the main branch trigger build and container publish to Azure Container Registry (ACR).
