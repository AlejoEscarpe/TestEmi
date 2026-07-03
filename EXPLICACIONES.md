# Explicaciones del proyecto

## 1) Diseño de dominio y cálculo de bonus

- Clase Employee: contiene Id, Name, CurrentPosition (enum PositionType), Salary, referencia a Department, PositionHistories y Projects. La representación como entidad es simple y explícita para facilitar mapeo EF.
- PositionHistory: registro inmutable (salvo EndDate) que guarda la posición, fecha de inicio y fin; se enlaza con Employee para mantener el historial.
- Cálculo de bonus: se implementa el patrón Strategy. Ventajas:
  - Abierto a extensión (OCP): añadir nuevos tipos de cálculo no obliga a tocar Employee.
  - Testable: cada estrategia se puede unit-testear aisladamente.
  - Implementación actual: Regular 10%, Manager (y variantes SeniorManager/Director) 20%.

Decisión técnica: CurrentPosition usa un enum (mejor que int) y la fábrica (BonusStrategyFactory) mapea tipos a estrategias.

## 2) API REST y middleware

- Endpoints CRUD expuestos en EmployeesController:
  - GET /api/employees
  - GET /api/employees/{id}
  - POST /api/employees
  - PUT /api/employees/{id}
  - DELETE /api/employees/{id}
  - GET /api/employees/department/{departmentId}
- El controlador aplica [Authorize] globalmente y protege mutaciones con [Authorize(Roles = "Admin")].
- Middleware: RequestLoggingMiddleware captura método, ruta, query string y cuerpo de la petición. Se registra en Program.cs con UseMiddleware.

Buenas prácticas aplicadas en middleware:
- Habilitar EnableBuffering para leer el body sin consumirlo.
- No modificar la petición para que otros middlewares/controllers puedan leerla.

## 3) Autenticación y autorización (JWT y roles)

- AuthController genera JWT firmado con una clave simétrica (configurada en appsettings). El token incluye ClaimTypes.Name y ClaimTypes.Role.
- Roles definidos: Admin y User. Política aplicada: Admin puede crear/editar/borrar; User puede listar.

## 4) Diseño de base de datos y EF Core

- Esquema principal:
  - Employees(Id, Name, CurrentPosition, Salary, DepartmentId)
  - Departments(Id, Name)
  - Projects(Id, Name, Description)
  - PositionHistories(Id, EmployeeId, Position, PositionType, StartDate, EndDate)
  - EmployeeProjects (tabla many-to-many implícita por EF Core)
- ApplicationDbContext configura relaciones y tipos (decimal(18,2) para Salary, FK, cascade rules). Projects <-> Employees es muchos-a-muchos.

Query solicitada (LINQ usando EF Core):

	// Obtener empleados de un departamento que tengan al menos un proyecto
	var result = await _context.Employees
		.Include(e => e.Department)
		.Include(e => e.Projects)
		.Where(e => e.DepartmentId == departmentId && e.Projects.Any())
		.ToListAsync();

## 5) Rendimiento y optimización

Problemas comunes y soluciones prácticas:
- Consultas N+1: usar Include o proyecciones con joins y revisar el SQL generado.
- Queries pesadas: indexar columnas usadas en WHERE/JOIN, revisar el plan de ejecución en SQL Server (Query Store/Profiler).
- Lecturas frecuentes: aplicar caching (MemoryCache, Distributed Cache) con invalidación correcta.
- IO/espera: usar operaciones asíncronas (async/await) y connection pooling.

Herramientas de profiling recomendadas:
- Visual Studio Profiler, dotnet-trace, dotnet-counters, SQL Server Profiler / Query Store.

Cómo perfilar una consulta lenta:
1. Reproducir la consulta con logs para obtener el SQL generado (usar ToQueryString() o logging de EF Core).
2. Ejecutar el SQL en SQL Server Management Studio y revisar el plan de ejecución.
3. Añadir índices, reescribir la consulta o crear proyecciones si procede.
4. Volver a medir con herramientas de profiling y pruebas de carga.

## Patrones, principios y arquitectura

- Arquitectura por capas: Domain, Application, Infrastructure, API — enfoque similar a Clean/Onion Architecture. Esto facilita separación de responsabilidades, testabilidad y evolución.
- Patrones usados:
  - Repository: abstracción de acceso a datos (Repository<T>, IEmployeeRepository).
  - Strategy + Factory: cálculo de bonos (extensible y testeable).
  - DTOs y Services: separación de lógica de negocio vs transporte.
- SOLID: las capas y la inyección de dependencias promocionan SRP, OCP, DIP y testabilidad.

