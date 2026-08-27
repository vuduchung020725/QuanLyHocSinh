# Test Plan / Demo Checklist

1. Start the app
   - `dotnet run --project "QLPHONGHOC\QLPHONGHOC.csproj"`
   - Confirm console shows "Now listening on: http://localhost:5266"

2. Health checks
   - Visit `http://localhost:5266/api/health` → should return status UP
   - Visit `http://localhost:5266/api/health/db` → should return `canConnect: true` and counts

3. Login (web)
   - Open `http://localhost:5266/Account/Login`
   - Use `admin` / `123456` → should redirect to dashboard/Home

4. CRUD flows (sample)
   - Students: `HocSinh` → create, edit, delete, view details
   - Teachers: `GiaoVien` → create, edit, delete, view
   - Classes/Subjects/Scores: perform create/list operations

5. API login
   - `POST /api/Account/Login` with JSON `{"TenDangNhap":"admin","MatKhau":"123456"}` → returns token

6. Submission checklist
   - Include this repository zip and `README.md` and `TEST_PLAN.md` in submission.

If any step fails, check `appsettings.json` connection string and ensure SQL Server is reachable.
