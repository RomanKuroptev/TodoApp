# TodoApp

## Prerequisites
- .NET 9 SDK (9.0.100)

## Commands
```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run --project TodoApp.Api
```

## Curl Examples
```bash
# Health check
curl http://localhost:5000/health

# List todos
curl http://localhost:5000/api/todos

# Filter todos by completion status
curl "http://localhost:5000/api/todos?isDone=true"

# Get a todo by id
curl http://localhost:5000/api/todos/1

# Create a todo
curl -X POST http://localhost:5000/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Draft agenda","dueDate":"2024-12-31"}'

# Update a todo
curl -X PUT http://localhost:5000/api/todos/1 \
  -H "Content-Type: application/json" \
  -d '{"title":"Draft final agenda","isDone":false,"dueDate":"2025-01-05"}'

# Patch a todo
curl -X PATCH http://localhost:5000/api/todos/1 \
  -H "Content-Type: application/json" \
  -d '{"isDone":true}'

# Delete a todo
curl -X DELETE http://localhost:5000/api/todos/1
```

## Swagger UI
Swagger is available at `http://localhost:5000/swagger` when running with `ASPNETCORE_ENVIRONMENT=Development` or when `SWAGGER__ENABLED=true`.
