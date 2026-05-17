@echo off
setlocal

pushd "%~dp0"

echo Building and starting KrosDemo (api, client, db)...
docker compose up --build -d
if errorlevel 1 (
    echo.
    echo docker compose failed. Is Docker Desktop running?
    popd
    exit /b 1
)

echo.
echo Containers:
docker compose ps

echo.
echo  API:     http://localhost:5000
echo  Swagger: http://localhost:5000/swagger
echo  Client:  http://localhost:4200
echo  DB:      localhost,1433 (sa / Your_password123)
echo.
echo  Logs:  docker compose logs -f
echo  Stop:  docker compose down
echo.

popd
endlocal