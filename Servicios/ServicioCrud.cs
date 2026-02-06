// ServicioCrud.cs — Implementación del servicio que orquesta operaciones CRUD
// Ubicación: Servicios/ServicioCrud.cs
//
// Principios SOLID aplicados:
// - SRP: Orquesta operaciones, valida permisos, delega a repositorio
// - DIP: Depende de abstracciones (IRepositorioLecturaTabla, IPoliticaTablasProhibidas)
// - OCP: Abierto para extensión, cerrado para modificación

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiGenerica.Repositorios.Abstracciones;
using ApiGenerica.Servicios.Abstracciones;

namespace ApiGenerica.Servicios
{
    /// <summary>
    /// Servicio que orquesta las operaciones CRUD aplicando políticas de seguridad.
    /// 
    /// FLUJO DE UNA OPERACIÓN:
    /// 1. Recibe petición del controlador
    /// 2. Valida que la tabla esté permitida (IPoliticaTablasProhibidas)
    /// 3. Si está prohibida, lanza UnauthorizedAccessException
    /// 4. Si está permitida, delega al repositorio (IRepositorioLecturaTabla)
    /// 5. Retorna resultado al controlador
    /// </summary>
    public class ServicioCrud : IServicioCrud
    {
        private readonly IRepositorioLecturaTabla _repositorio;
        private readonly IPoliticaTablasProhibidas _politicaTablas;

        /// <summary>
        /// Constructor que recibe dependencias por inyección.
        /// </summary>
        public ServicioCrud(
            IRepositorioLecturaTabla repositorio,
            IPoliticaTablasProhibidas politicaTablas)
        {
            _repositorio = repositorio ?? throw new ArgumentNullException(
                nameof(repositorio), "IRepositorioLecturaTabla no puede ser null.");
            
            _politicaTablas = politicaTablas ?? throw new ArgumentNullException(
                nameof(politicaTablas), "IPoliticaTablasProhibidas no puede ser null.");
        }

        /// <summary>
        /// Valida que la tabla esté permitida. Lanza excepción si está prohibida.
        /// </summary>
        private void ValidarAccesoTabla(string nombreTabla)
        {
            if (!_politicaTablas.EsTablaPermitida(nombreTabla))
            {
                throw new UnauthorizedAccessException(
                    $"Acceso denegado: La tabla '{nombreTabla}' está restringida. " +
                    $"Contacte al administrador si necesita acceso.");
            }
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerTodosAsync(
            string nombreTabla,
            string? esquema,
            int? limite)
        {
            ValidarAccesoTabla(nombreTabla);
            return await _repositorio.ObtenerFilasAsync(nombreTabla, esquema, limite);
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor)
        {
            ValidarAccesoTabla(nombreTabla);
            return await _repositorio.ObtenerPorClaveAsync(nombreTabla, esquema, nombreClave, valor);
        }

        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            ValidarAccesoTabla(nombreTabla);
            return await _repositorio.CrearAsync(nombreTabla, esquema, datos, camposEncriptar);
        }

        public async Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            ValidarAccesoTabla(nombreTabla);
            return await _repositorio.ActualizarAsync(
                nombreTabla, esquema, nombreClave, valorClave, datos, camposEncriptar);
        }

        public async Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave)
        {
            ValidarAccesoTabla(nombreTabla);
            return await _repositorio.EliminarAsync(nombreTabla, esquema, nombreClave, valorClave);
        }

        public async Task<Dictionary<string, object?>> ObtenerDiagnosticoAsync()
        {
            return await _repositorio.ObtenerDiagnosticoConexionAsync();
        }
    }
}
