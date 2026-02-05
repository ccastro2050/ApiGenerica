// -----------------------------------------------------------------------------
// Archivo: RepositorioLecturaMysqlMariaDB.cs
// Ruta: ApiGenerica/Repositorios/RepositorioLecturaMysqlMariaDB.cs
// Propósito: Implementar IRepositorioLecturaTabla para MySQL/MariaDB
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using ApiGenerica.Repositorios.Abstracciones;
using ApiGenerica.Servicios.Abstracciones;
using ApiGenerica.Servicios.Utilidades;

namespace ApiGenerica.Repositorios
{
    /// <summary>
    /// Repositorio concreto para MySQL/MariaDB que implementa las operaciones
    /// de IRepositorioLecturaTabla.
    /// </summary>
    public sealed class RepositorioLecturaMysqlMariaDB : IRepositorioLecturaTabla
    {
        private readonly IProveedorConexion _proveedorConexion;

        public RepositorioLecturaMysqlMariaDB(IProveedorConexion proveedorConexion)
        {
            _proveedorConexion = proveedorConexion ?? throw new ArgumentNullException(nameof(proveedorConexion));
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerFilasAsync(
            string nombreTabla,
            string? esquema,
            int? limite)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            int limiteFinal = limite ?? 1000;
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            string sql = $"SELECT * FROM {esquemaFinal}`{nombreTabla}` LIMIT @limite";

            var filas = new List<Dictionary<string, object?>>();
            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@limite", limiteFinal);

            await using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            while (await lector.ReadAsync())
            {
                var fila = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < lector.FieldCount; i++)
                {
                    fila[lector.GetName(i)] = await lector.IsDBNullAsync(i) ? null : lector.GetValue(i);
                }
                filas.Add(fila);
            }

            return filas;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            string sql = $"SELECT * FROM {esquemaFinal}`{nombreTabla}` WHERE `{nombreClave}` = @valor";

            var filas = new List<Dictionary<string, object?>>();
            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@valor", valor);

            await using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                var fila = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < lector.FieldCount; i++)
                {
                    fila[lector.GetName(i)] = await lector.IsDBNullAsync(i) ? null : lector.GetValue(i);
                }
                filas.Add(fila);
            }

            return filas;
        }

        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (datos is null || datos.Count == 0)
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            // Encriptar campos si se especificaron
            if (!string.IsNullOrWhiteSpace(camposEncriptar))
            {
                foreach (var campo in camposEncriptar.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (datos.ContainsKey(campo))
                    {
                        var original = datos[campo]?.ToString() ?? string.Empty;
                        datos[campo] = EncriptacionBCrypt.Encriptar(original, costo: 10);
                    }
                }
            }

            var nombres = new List<string>();
            var parametros = new List<string>();
            var parametrosComando = new List<MySqlParameter>();

            foreach (var par in datos)
            {
                string nombreCol = par.Key;
                string nombreParam = $"p_{nombreCol}";
                nombres.Add($"`{nombreCol}`");
                parametros.Add($"@{nombreParam}");
                parametrosComando.Add(new MySqlParameter(nombreParam, par.Value ?? DBNull.Value));
            }

            string sql = $"INSERT INTO {esquemaFinal}`{nombreTabla}` ({string.Join(", ", nombres)}) VALUES ({string.Join(", ", parametros)})";

            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametrosComando.ToArray());

            int afectados = await comando.ExecuteNonQueryAsync();
            return afectados > 0;
        }

        public async Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));
            if (datos is null || datos.Count == 0)
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            // Encriptar campos si se especificaron
            if (!string.IsNullOrWhiteSpace(camposEncriptar))
            {
                foreach (var campo in camposEncriptar.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (datos.ContainsKey(campo))
                    {
                        var original = datos[campo]?.ToString() ?? string.Empty;
                        datos[campo] = EncriptacionBCrypt.Encriptar(original, costo: 10);
                    }
                }
            }

            var asignaciones = new List<string>();
            var parametrosComando = new List<MySqlParameter>();

            foreach (var par in datos)
            {
                string nombreCol = par.Key;
                string nombreParam = $"p_{nombreCol}";
                asignaciones.Add($"`{nombreCol}` = @{nombreParam}");
                parametrosComando.Add(new MySqlParameter(nombreParam, par.Value ?? DBNull.Value));
            }

            string sql = $"UPDATE {esquemaFinal}`{nombreTabla}` SET {string.Join(", ", asignaciones)} WHERE `{nombreClave}` = @valorClave";

            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddRange(parametrosComando.ToArray());
            comando.Parameters.AddWithValue("@valorClave", valorClave);

            return await comando.ExecuteNonQueryAsync();
        }

        public async Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("El nombre de la clave no puede estar vacío.", nameof(nombreClave));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            string sql = $"DELETE FROM {esquemaFinal}`{nombreTabla}` WHERE `{nombreClave}` = @valorClave";

            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@valorClave", valorClave);

            return await comando.ExecuteNonQueryAsync();
        }

        public async Task<string?> ObtenerHashContrasenaAsync(
            string nombreTabla,
            string? esquema,
            string campoUsuario,
            string campoContrasena,
            string valorUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "" : $"`{esquema}`.";

            string sql = $"SELECT `{campoContrasena}` FROM {esquemaFinal}`{nombreTabla}` WHERE `{campoUsuario}` = @usuario LIMIT 1";

            var cadena = _proveedorConexion.ObtenerCadenaConexion();

            await using var conexion = new MySqlConnection(cadena);
            await conexion.OpenAsync();

            await using var comando = new MySqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@usuario", valorUsuario);

            object? resultado = await comando.ExecuteScalarAsync();
            return resultado == null || resultado is DBNull ? null : Convert.ToString(resultado);
        }

        public async Task<Dictionary<string, object?>> ObtenerDiagnosticoConexionAsync()
        {
            string consultaDiagnostico = @"
                SELECT
                    DATABASE() AS nombre_base_datos,
                    SCHEMA() AS esquema_actual,
                    VERSION() AS version_servidor,
                    @@hostname AS nombre_servidor,
                    @@port AS puerto_servidor,
                    @@version_comment AS tipo_servidor,
                    USER() AS usuario_actual,
                    CONNECTION_ID() AS id_proceso_conexion";

            try
            {
                string cadenaConexion = _proveedorConexion.ObtenerCadenaConexion();

                await using var conexion = new MySqlConnection(cadenaConexion);
                await conexion.OpenAsync();

                await using var comando = new MySqlCommand(consultaDiagnostico, conexion);
                await using var lector = await comando.ExecuteReaderAsync();

                if (!await lector.ReadAsync())
                    throw new InvalidOperationException("No se pudo obtener información de diagnóstico.");

                string nombreBaseDatos = lector.GetString(0);
                string? esquemaActual = lector.IsDBNull(1) ? null : lector.GetString(1);
                string versionServidor = lector.GetString(2);
                string nombreServidor = lector.GetString(3);
                int puertoServidor = lector.GetInt32(4);
                string tipoServidor = lector.GetString(5);
                string usuarioActual = lector.GetString(6);
                ulong idProcesoConexion = lector.GetUInt64(7);

                await lector.CloseAsync();

                // Obtener uptime
                string consultaUptime = "SHOW STATUS LIKE 'Uptime'";
                await using var comandoUptime = new MySqlCommand(consultaUptime, conexion);
                await using var lectorUptime = await comandoUptime.ExecuteReaderAsync();

                long uptimeSegundos = 0;
                if (await lectorUptime.ReadAsync())
                {
                    uptimeSegundos = Convert.ToInt64(lectorUptime.GetString(1));
                }

                DateTime horaInicio = DateTime.UtcNow.AddSeconds(-uptimeSegundos);
                TimeSpan tiempoEncendido = TimeSpan.FromSeconds(uptimeSegundos);

                string proveedor = tipoServidor.Contains("MariaDB", StringComparison.OrdinalIgnoreCase)
                    ? "MariaDB"
                    : "MySQL";

                return new Dictionary<string, object?>
                {
                    ["proveedor"] = proveedor,
                    ["baseDatos"] = nombreBaseDatos,
                    ["esquema"] = esquemaActual ?? nombreBaseDatos,
                    ["version"] = versionServidor,
                    ["tipoServidor"] = tipoServidor,
                    ["servidor"] = nombreServidor,
                    ["puerto"] = puertoServidor,
                    ["horaInicio"] = horaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ["usuarioConectado"] = usuarioActual,
                    ["idProcesoConexion"] = idProcesoConexion,
                    ["tiempoEncendido"] = $"{(int)tiempoEncendido.TotalDays} días, {tiempoEncendido.Hours} horas"
                };
            }
            catch (MySqlException ex)
            {
                throw new InvalidOperationException($"Error MySQL al obtener diagnóstico: {ex.Message}", ex);
            }
        }
    }
}
