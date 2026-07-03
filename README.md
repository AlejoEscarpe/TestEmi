# EmiTest - Sistema de Gestión de Empleados

Solución técnica modular diseñada bajo los más altos estándares de ingeniería de software. El proyecto implementa una arquitectura desacoplada, segura, auditable y escalable utilizando **.NET 8** y **Entity Framework Core**.

---

## 🏗️ Arquitectura de la Solución (Clean Architecture)

El proyecto está estructurado siguiendo los principios de **Clean Architecture** (Arquitectura Limpia), lo que garantiza la separación de responsabilidades y permite que las reglas de negocio permanezcan independientes de frameworks, interfaces de usuario o proveedores de bases de datos.

La dependencia de los proyectos fluye estrictamente de afuera hacia adentro:

* **EmiTest.Domain (Núcleo):** Contiene el modelo conceptual de negocio. Incluye las entidades puras (`Employee`, `Department`, `Project`, `PositionHistory`) y las enumeraciones del sistema. No tiene dependencias de ninguna librería o framework externo.
* **EmiTest.Application (Lógica de Negocio):** Define los contratos de interfaces, los Data Transfer Objects (DTOs) para la transferencia segura de información y los servicios que orquestan los casos de uso (como el procesamiento y cálculo de beneficios de los empleados).
* **EmiTest.Infrastructure (Persistencia y Acceso a Datos):** Implementa el acceso a datos mediante EF Core. Contiene el `ApplicationDbContext` configurado detalladamente mediante Fluent API, la gestión de migraciones y las implementaciones concretas de los repositorios.
* **EmiTest.API (Presentación / Entrada):** Punto de entrada del sistema. Expone los endpoints mediante controladores REST, gestiona la inyección de dependencias global, implementa la seguridad JWT y configura el pipeline HTTP de ASP.NET Core.

---

## 🛠️ Patrones de Diseño Implementados

Para dar cumplimiento estricto a los requerimientos de la prueba, se aplicaron los siguientes patrones arquitectónicos y tácticos (**SOLID**):

1.  **Strategy Pattern (Sección 1):** Utilizado para aislar los diferentes algoritmos de cálculo de bono anual según el tipo de cargo (`Practicante`, `Junior`, `Senior`, etc.). Esto permite extender o modificar las reglas de bonos en el futuro sin alterar la entidad `Employee`, respetando el principio **Open/Closed (OCP)**.
2.  **Factory Pattern (Sección 1):** Encapsula la lógica de creación y resolución dinámica de las estrategias de bonos en la clase `BonusStrategyFactory` en tiempo de ejecución, evitando acoplamientos innecesarios en la capa de aplicación.
3.  **Repository Pattern (Sección 4):** Centraliza las operaciones de acceso a datos a través de una abstracción genérica `IRepository<T>` y una especializada `IEmployeeRepository`. Esto aísla la lógica de negocio de las consultas SQL o mutaciones directas de EF Core.
4.  **Chain of Responsibility / Middleware (Sección 2.3):** Implementado mediante `RequestLoggingMiddleware`. Captura de manera segura la metadata de cada petición HTTP entrante, incluyendo tiempos, métodos y el cuerpo (Body) de forma no destructiva utilizando buffereing de flujo, logueando la información antes de delegar el control al siguiente componente.

---

## 🔒 Seguridad y Autorización Basada en Roles (RBAC)

La API cuenta con protección contra accesos anónimos mediante **JSON Web Tokens (JWT)** firmados criptográficamente mediante HMAC-SHA256. 

Las políticas de autorización restringen los recursos de la siguiente manera (**Sección 3.2**):
* **Operaciones de Lectura (`GET`):** Accesibles por usuarios con rol **`Admin`** y **`User`**.
* **Operaciones de Escritura/Modificación (`POST`, `PUT`, `DELETE`):** Restringidas **exclusivamente** para usuarios con rol **`Admin`**. Cualquier intento de acceso por un usuario con rol común será interceptado y rechazado automáticamente con un código de estado estándar **`403 Forbidden`**.

---

## 🚀 Guía de Inicio Rápido (Quick Start)

### Requisitos Previos
* Visual Studio 2022 / 2026 con la carga de trabajo *Desarrollo de ASP.NET y web*.
* .NET 8 SDK o superior.
* SQL Server LocalDB (incluido por defecto en la instalación de Visual Studio).

### Paso 1: Configurar y Desplegar la Base de Datos
La persistencia local está configurada para ejecutarse sobre una instancia ligera de SQL Server LocalDB de forma transparente. Abre la **Consola del Administrador de Paquetes** (`Herramientas > Administrador de paquetes NuGet > Consola...`) y ejecuta el comando para aplicar las migraciones y estructurar el esquema relacional:

powershell
Update-Database -Project EmiTest.Infrastructure -StartupProject EmiTest.API

### Paso 2: Insertar Datos Maestros Iniciales
Dado que las tablas cuentan con reglas estrictas de integridad referencial (Llaves Foráneas), antes de registrar un empleado debes asegurar la existencia de al menos un departamento:

Abre el Explorador de objetos de SQL Server en Visual Studio.

Despliega (localdb)\MSSQLLocalDB > Bases de datos > EmiTestDb > Tablas.

Haz clic derecho en dbo.Departments y selecciona Ver datos.

Inserta una fila de prueba (ej. Id: 1, Name: "Desarrollo").

### Paso 3: Ejecución de la API
Establece el proyecto EmiTest.API como el proyecto de inicio de la solución.

Presiona F5 o haz clic en Iniciar depuración. Se desplegará automáticamente en tu navegador predeterminado el sitio de Swagger UI.

### 🧪 Escenarios de Prueba en Swagger
Para validar el flujo completo de la aplicación directamente desde la UI de Swagger, siga este flujo secuencial:

1. Autenticación y Generación de Tokens (Sección 3.1)
Despliega el controlador Auth y abre el endpoint POST /api/Auth/login.

Para simular Rol Admin: Envía en el cuerpo el JSON con el usuario "admin" (la contraseña puede ser cualquiera para efectos de la prueba):

JSON
{
  "username": "admin",
  "password": "PasswordAdmin123"
}
Ejecuta la petición y copia la cadena de caracteres del campo "token" de la respuesta.

2. Autorizar la Sesión en la UI
Sube a la parte superior derecha de Swagger y haz clic en el botón Authorize (icono de candado).

En el campo de texto ingresa la palabra Bearer seguida de un espacio y pega el token que acabas de copiar. Ejemplo:

Plaintext
Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Haz clic en Authorize y luego en Close.

3. Probar Operaciones del CRUD (Employees)
Creación (POST): Consuma POST /api/Employees utilizando el departmentId: 1. La API responderá con un código 201 Created, calculará el bono dinámicamente según la posición elegida e inicializará el historial de cargos de auditoría.

Lectura (GET): Consuma GET /api/Employees para ver el listado de registros insertados o filtre usando optimización LINQ en /api/Employees/department/{departmentId}.

4. Verificación de Restricciones (Probar el Rol User)
Vuelve a generar un token en POST /api/Auth/login pero esta vez cambia el usuario por cualquier otro nombre (ej. "username": "alejandro"). Esto le asignará automáticamente el rol User.

Actualiza el token en el botón Authorize de Swagger con el nuevo token de usuario común.

Intenta realizar un POST, PUT o DELETE sobre los empleados. Swagger reflejará inmediatamente una respuesta con código 403 Forbidden, comprobando el correcto funcionamiento de las políticas de seguridad.

Revise la terminal de consola activa de la aplicación para evidenciar cómo el Middleware de Logging registra en tiempo real las auditorías de estas solicitudes.