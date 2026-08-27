# QLPHONGHOC — School Management Demo

This repository is a minimal school management system built with ASP.NET Core MVC and EF Core.

Quick start (Windows):

1. Ensure SQL Server (e.g. LocalDB or SQLEXPRESS) is running and accessible.
2. Update connection string in `appsettings.json` if needed (DefaultConnection).
3. From repo root, build and run:

```powershell
dotnet restore
dotnet build
dotnet run --project "QLPHONGHOC\QLPHONGHOC.csproj"
```

The app listens by default at `http://localhost:5266` (see console output).

Demo accounts (seeded automatically on startup if missing):
- admin / 123456 (ADMIN)
- teacher / 123456 (TEACHER)
- student / 123456 (STUDENT)

Health endpoints:
- `GET /api/health` — basic app health
- `GET /api/health/db` — DB connectivity and simple counts

Notes:
- Passwords are stored hashed; the seeder will hash any plaintext passwords it finds.
- If you need to reseed, stop the app, modify the database as needed, then restart.
