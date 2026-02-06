// IServicioCrud.cs — Interfaz para el servicio de operaciones CRUD
// Ubicación: Servicios/Abstracciones/IServicioCrud.cs
//
// Principios SOLID aplicados:
// - SRP: Define operaciones CRUD genéricas
// - DIP: Permite que controladores dependan de abstracción, no implementación

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiGenerica.Servicios.Abstracciones
{
    /// <summary>
    /// Contrato para el servicio que orquesta operaciones CRUD.
    /// 
    /// Este servicio actúa como intermediario entre el Controlador y el Repositorio:
    /// - Valida que la tabla esté permitida (política de seguridad)
    /// - Delega las operaciones al repositorio correspondiente
    /// - Maneja excepciones y las traduce a respuestas apropiadas
    /// </summary>
    public interface IServicioCrud
    {
        /// <summary>
        /// Obtiene todas las filas de una tabla.
        /// </summary>
        Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerTodosAsync(
            string nombreTabla,
            string? esquema,
            int? limite
        );

        /// <summary>
        /// Obtiene filas filtradas por una clave.
        /// </summary>
        Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor
        );

        /// <summary>
        /// Crea un nuevo registro.
        /// </summary>
        Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        );

        /// <summary>
        /// Actualiza un registro existente.
        /// </summary>
        Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null
        );

        /// <summary>
        /// Elimina un registro.
        /// </summary>
        Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave
        );

        /// <summary>
        /// Obtiene información de diagnóstico de la conexión.
        /// </summary>
        Task<Dictionary<string, object?>> ObtenerDiagnosticoAsync();
    }
}
