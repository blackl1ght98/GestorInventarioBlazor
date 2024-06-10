using CRUDBlazor.Server.Application.DTOs;
using CRUDBlazor.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartamentoController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly ILogger<DepartamentoController> _logger;

        public DepartamentoController(DbcrudBlazorContext context, ILogger<DepartamentoController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetDepartamento()
        {
            try
            {
                var departamentos = await _context.Departamentos.ToListAsync();
                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los departamentos");
                return BadRequest("Error al obtener los departamentos");
            }
           
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetDepartamentoPorId(int id)
        {
            try
            {
                var departamentos = await _context.Departamentos.FirstOrDefaultAsync(x => x.IdDepartamento == id);
                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los departamentos por id");
                return BadRequest("Error al obtener el departamento por id, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador del sitio");
            }
            
        }
        [HttpPost]
        public async Task<IActionResult> PostDepartamento(DTODepartamento despacho)
        {
            try
            {
                var departamento = new Departamento()
                {
                    IdDepartamento = despacho.idDepartamento,
                    Nombre = despacho.nombre,
                };
                _context.Departamentos.Add(departamento);
                await _context.SaveChangesAsync();
                return Ok("Departamento guardado con exito");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el departamento");
                return BadRequest("Error al crear el departamento, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
            
        }
        [HttpPut]
        [Route("{idDepartamento}")]
        public async Task<IActionResult> UpdateDepatamento(DTODepartamento departamento)
        {
            try
            {
                var buscarDepatamento = await _context.Departamentos.FirstOrDefaultAsync(x => x.IdDepartamento == departamento.idDepartamento);
                if (buscarDepatamento == null)
                {
                    return BadRequest("Departamento no encontrado");

                }
                buscarDepatamento.Nombre = departamento.nombre;
                _context.Departamentos.Update(buscarDepatamento);
                await _context.SaveChangesAsync();
                return Ok("Depatamento actualizado con exito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el departamento");
                return BadRequest("Error al actualizar el producto intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
           
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDepartamento(int id)
        {
            try
            {
                var departamento = await _context.Departamentos.FirstOrDefaultAsync(x => x.IdDepartamento == id);
                if (departamento == null)
                {
                    return NotFound("Departamento no encontrado");
                }
                _context.Departamentos.Remove(departamento);
                await _context.SaveChangesAsync();
                return Ok("Departamento eliminado con exito");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el departamento");
                return BadRequest("Error al eliminar el departamento, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
           
        }
    }
}
