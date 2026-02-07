# ApiGenerica - API REST Generica Multi-Base de Datos

API REST generica para operaciones CRUD sobre cualquier tabla de base de datos. Soporta multiples motores de base de datos con una sola configuracion.

---

## Tabla de Contenidos

- [Caracteristicas](#caracteristicas)
- [Arquitectura](#arquitectura)
- [Requisitos](#requisitos)
- [Instalacion](#instalacion)
- [Configuracion](#configuracion)
- [Bases de Datos Soportadas](#bases-de-datos-soportadas)
- [Endpoints](#endpoints)
- [Autenticacion JWT](#autenticacion-jwt)
- [Ejemplos de Uso](#ejemplos-de-uso)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Principios SOLID](#principios-solid)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)

---

## Caracteristicas

- **CRUD Generico**: Operaciones Create, Read, Update, Delete sobre cualquier tabla
- **Multi-Base de Datos**: SQL Server, PostgreSQL, MySQL, MariaDB
- **Autenticacion JWT**: Tokens seguros con expiracion configurable
- **Swagger UI**: Documentacion interactiva de la API
- **Consultas Parametrizadas**: Ejecucion segura de SQL con parametros
- **Stored Procedures**: Ejecucion dinamica de procedimientos almacenados
- **Introspeccion de BD**: Consultar estructura de tablas y base de datos
- **Encriptacion BCrypt**: Hash seguro de contrasenas
- **CORS Configurado**: Listo para consumir desde frontend
- **Arquitectura Limpia**: Separacion de responsabilidades (Controllers, Services, Repositories)

---

## Arquitectura

```
+-------------------------------------------------------------+
|                        CONTROLLERS                          |
|  EntidadesController | ConsultasController | Autenticacion  |
|  DiagnosticoController | EstructurasController | Procedimientos |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                         SERVICIOS                           |
|         IServicioCrud          |      IServicioConsultas    |
|              |                 |              |             |
|         ServicioCrud           |      ServicioConsultas     |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                       REPOSITORIOS                          |
|  +-------------+  +-------------+  +---------------------+  |
|  |  SQL Server |  |  PostgreSQL |  |  MySQL / MariaDB    |  |
|  +-------------+  +-------------+  +---------------------+  |
+-------------------------------------------------------------+
                              |
                              v
+-------------------------------------------------------------+
|                      BASE DE DATOS                          |
+-------------------------------------------------------------+
```

---

## Requisitos

| Requisito | Version |
|-----------|---------|
| .NET SDK | 9.0 o superior |
| Visual Studio / VS Code | 2022 / Ultima version |
| Base de datos | SQL Server, PostgreSQL, MySQL o MariaDB |

---

## Instalacion

### 1. Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd ApiGenerica
```

### 2. Restaurar paquetes NuGet

```bash
dotnet restore
```

### 3. Compilar el proyecto

```bash
dotnet build
```

### 4. Ejecutar la API

```bash
dotnet run
```

### 5. Abrir Swagger

Navegar a: `https://localhost:5001/swagger` o `http://localhost:5000/swagger`

---

## Configuracion

### Archivo appsettings.json

```json
{
  "Jwt": {
    "Key": "MiClaveSecretaMuyLargaDeAlMenos32Caracteres!",
    "Issuer": "MiApp",
    "Audience": "MiAppUsers",
    "DuracionMinutos": 60
  },
  "TablasProhibidas": [],
  "ConnectionStrings": {
    "SqlServer": "Server=MI_SERVIDOR;Database=mi_bd;Integrated Security=True;TrustServerCertificate=True;",
    "LocalDb": "Server=(localdb)\\MSSQLLocalDB;Database=mi_bd;Integrated Security=True;TrustServerCertificate=True;",
    "Postgres": "Host=localhost;Port=5432;Database=mi_bd;Username=postgres;Password=postgres;",
    "MySQL": "Server=localhost;Port=3306;Database=mi_bd;User=root;Password=mysql;CharSet=utf8mb4;",
    "MariaDB": "Server=localhost;Port=3306;Database=mi_bd;User=root;Password=;"
  },
  "DatabaseProvider": "SqlServer"
}
```

### Cambiar de base de datos

Solo modifica el valor de `DatabaseProvider`:

| Valor | Base de datos |
|-------|---------------|
| `SqlServer` | Microsoft SQL Server |
| `LocalDb` | SQL Server LocalDB (desarrollo) |
| `Postgres` | PostgreSQL |
| `MySQL` | MySQL |
| `MariaDB` | MariaDB |

---

## Bases de Datos Soportadas

| Base de Datos | Paquete NuGet | Puerto Default |
|---------------|---------------|----------------|
| SQL Server | Microsoft.Data.SqlClient | 1433 |
| SQL Server LocalDB | Microsoft.Data.SqlClient | - |
| PostgreSQL | Npgsql | 5432 |
| MySQL | MySqlConnector | 3306 |
| MariaDB | MySqlConnector | 3306 |

---

## Endpoints

### EntidadesController - CRUD Generico

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| GET | `/api/{tabla}` | Obtener todos los registros | Si |
| GET | `/api/{tabla}/{clave}/{valor}` | Obtener por clave | Si |
| POST | `/api/{tabla}` | Crear registro | Si |
| PUT | `/api/{tabla}/{clave}/{valor}` | Actualizar registro | Si |
| DELETE | `/api/{tabla}/{clave}/{valor}` | Eliminar registro | Si |

### ConsultasController - SQL Parametrizado

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| POST | `/api/consultas/ejecutar` | Ejecutar consulta SQL | Si |
| POST | `/api/consultas/validar` | Validar consulta SQL | Si |

### AutenticacionController - JWT

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| POST | `/api/autenticacion/login` | Iniciar sesion | No |

### DiagnosticoController - Estado del Sistema

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| GET | `/api/diagnostico/salud` | Verificar estado de la API | No |
| GET | `/api/diagnostico/conexion` | Verificar conexion a BD | No |

### EstructurasController - Introspeccion

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| GET | `/api/estructuras/{tabla}/modelo` | Estructura de una tabla | No |
| GET | `/api/estructuras/basedatos` | Estructura completa de la BD | No |

### ProcedimientosController - Stored Procedures

| Metodo | Ruta | Descripcion | Auth |
|--------|------|-------------|------|
| POST | `/api/procedimientos/ejecutarsp` | Ejecutar procedimiento almacenado | Si |

---

## Autenticacion JWT

### 1. Obtener token

```http
POST /api/autenticacion/login
Content-Type: application/json

{
  "tabla": "usuarios",
  "campoUsuario": "email",
  "campoContrasena": "password",
  "usuario": "admin@ejemplo.com",
  "contrasena": "miPassword123"
}
```

### 2. Respuesta exitosa

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expira": "2024-01-15T12:00:00Z",
  "usuario": "admin@ejemplo.com"
}
```

### 3. Usar token en peticiones

```http
GET /api/productos
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Ejemplos de Uso

### Obtener todos los productos

```http
GET /api/productos?limite=100
Authorization: Bearer {token}
```

### Obtener producto por ID

```http
GET /api/productos/id/42
Authorization: Bearer {token}
```

### Crear un producto

```http
POST /api/productos
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Laptop HP",
  "precio": 1500.00,
  "stock": 25
}
```

### Actualizar un producto

```http
PUT /api/productos/id/42
Authorization: Bearer {token}
Content-Type: application/json

{
  "precio": 1399.99,
  "stock": 30
}
```

### Eliminar un producto

```http
DELETE /api/productos/id/42
Authorization: Bearer {token}
```

### Ejecutar consulta SQL

```http
POST /api/consultas/ejecutar
Authorization: Bearer {token}
Content-Type: application/json

{
  "consultaSQL": "SELECT * FROM productos WHERE precio > @precio",
  "parametros": {
    "precio": 100.00
  }
}
```

### Ejecutar procedimiento almacenado

```http
POST /api/procedimientos/ejecutarsp
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombreSP": "sp_obtener_ventas_mes",
  "mes": 12,
  "anio": 2024
}
```

---

## Estructura del Proyecto

```
ApiGenerica/
|-- Controllers/
|   |-- AutenticacionController.cs    # Login y JWT
|   |-- ConsultasController.cs        # SQL parametrizado
|   |-- DiagnosticoController.cs      # Estado del sistema
|   |-- EntidadesController.cs        # CRUD generico
|   |-- EstructurasController.cs      # Introspeccion BD
|   +-- ProcedimientosController.cs   # Stored procedures
|
|-- Servicios/
|   |-- Abstracciones/
|   |   |-- IServicioCrud.cs          # Contrato CRUD
|   |   |-- IServicioConsultas.cs     # Contrato consultas
|   |   |-- IProveedorConexion.cs     # Contrato conexion
|   |   +-- IPoliticaTablasProhibidas.cs
|   |-- Conexion/
|   |   +-- ProveedorConexion.cs      # Proveedor de conexion
|   |-- Politicas/
|   |   +-- PoliticaTablasProhibidasDesdeJson.cs
|   |-- Utilidades/
|   |   +-- EncriptacionBCrypt.cs     # Hash de contrasenas
|   |-- ServicioCrud.cs               # Logica CRUD
|   +-- ServicioConsultas.cs          # Logica consultas
|
|-- Repositorios/
|   |-- Abstracciones/
|   |   |-- IRepositorioLecturaTabla.cs
|   |   +-- IRepositorioConsultas.cs
|   |-- RepositorioLecturaSqlServer.cs
|   |-- RepositorioLecturaPostgreSQL.cs
|   |-- RepositorioLecturaMysqlMariaDB.cs
|   |-- RepositorioConsultasSqlServer.cs
|   |-- RepositorioConsultasPostgreSQL.cs
|   +-- RepositorioConsultasMysqlMariaDB.cs
|
|-- Modelos/
|   +-- ConfiguracionJwt.cs           # Configuracion JWT
|
|-- Properties/
|   +-- launchSettings.json           # Puertos y perfiles
|
|-- appsettings.json                  # Configuracion produccion
|-- appsettings.Development.json      # Configuracion desarrollo
|-- Program.cs                        # Punto de entrada
|-- ApiGenerica.csproj                # Definicion del proyecto
+-- README.md                         # Este archivo
```

---

## Principios SOLID Aplicados

| Principio | Aplicacion |
|-----------|------------|
| **S** - Single Responsibility | Cada clase tiene una sola responsabilidad (Controller -> coordina, Service -> logica, Repository -> datos) |
| **O** - Open/Closed | Agregar nueva BD sin modificar codigo existente (solo nuevo repositorio) |
| **L** - Liskov Substitution | Cualquier repositorio puede sustituir a otro que implemente la misma interfaz |
| **I** - Interface Segregation | Interfaces especificas (IRepositorioLecturaTabla, IRepositorioConsultas) |
| **D** - Dependency Inversion | Controllers dependen de interfaces, no de implementaciones concretas |

---

## Tecnologias Utilizadas

| Tecnologia | Version | Proposito |
|------------|---------|-----------|
| .NET | 9.0 | Framework principal |
| ASP.NET Core | 9.0 | Framework web |
| Dapper | 2.1.66 | Micro ORM |
| BCrypt.Net-Next | 4.0.3 | Hash de contrasenas |
| Swashbuckle | 9.0.4 | Swagger UI |
| Microsoft.Data.SqlClient | 6.1.1 | Conexion SQL Server |
| Npgsql | 9.0.3 | Conexion PostgreSQL |
| MySqlConnector | 2.4.0 | Conexion MySQL/MariaDB |

---

## Paquetes NuGet

```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  <PackageReference Include="Dapper" Version="2.1.66" />
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.10" />
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.6" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.1" />
  <PackageReference Include="MySqlConnector" Version="2.4.0" />
  <PackageReference Include="Npgsql" Version="9.0.3" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.4" />
</ItemGroup>
```

---

## Probar la API

1. Ejecutar: `dotnet run`
2. Abrir: `http://localhost:5000/swagger`
3. Probar endpoint de diagnostico: `GET /api/diagnostico/salud`
4. Hacer login para obtener token
5. Usar token en endpoints protegidos

---

## Licencia

Este proyecto es de uso educativo.

---

## Autor

Creado como tutorial paso a paso para aprender a construir APIs genericas con .NET 9.
