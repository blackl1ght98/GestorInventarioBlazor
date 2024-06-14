﻿using CRUDBlazor.Server.Helpers;
using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;
using CRUDBlazor.Shared;
using CRUDBlazor.Shared.Proveedores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CRUDBlazor.Server.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly DbcrudBlazorContext _context;
        private readonly ILogger<ProveedorController> _logger;
        private readonly IProveedorRepository _proveedorRepository;
        public ProveedorController(DbcrudBlazorContext context, ILogger<ProveedorController> logger, IProveedorRepository proveedor)
        {
            _context = context;
            _logger = logger;
            _proveedorRepository=proveedor;
        }
        [HttpGet]
        public async Task<ActionResult<List<Proveedore>>> GetAllProviders([FromQuery] Paginacion paginacion)
        {
            try
            {
                var provider =  _proveedorRepository.ObtenerTodosProveedores();
               // var provider = _context.Proveedores.AsQueryable();
                await HttpContext.InsertarParametrosPaginacionRespuesta(provider, paginacion.CantidadAMostrar);
                return await provider.Paginar(paginacion).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los proveedores");
                return BadRequest("Error al obtener los proveedores, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
           
        }
        [HttpPost]
        public async Task<IActionResult> CreateProvider(ProveedorViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                    var (success,errorMessage)= await _proveedorRepository.CrearProveedor(model);
                    if (success)
                    {
                        return Ok(success);
                    }
                    else
                    {
                        return BadRequest(errorMessage);
                    }
                }
                return Ok("Proveedor creado con exito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el proveedor");
                return BadRequest("Error al crear el pedido, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
           
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProveedorPorId(int id)
        {
            try
            {
                var proveedor= await _proveedorRepository.ObtenerProveedorId(id);
                //var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == id);
                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el proveedor por id");
                return BadRequest("Error al obtener el proveedor por id, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
           
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(string id)
        {
            try
            {
                //int idInt = int.Parse(id);

                //var proveedor = await _context.Proveedores.FirstOrDefaultAsync(x => x.Id == idInt);
                //if (proveedor == null)
                //{
                //    return NotFound("Proveedor no encontrado");
                //}
                //_context.Proveedores.Remove(proveedor);
                //await _context.SaveChangesAsync();
                //return Ok("Proveedor eliminado con exito");
                var (success,errorMessage)=await _proveedorRepository.EliminarProveedor(id);
                if (success)
                {
                    return Ok(success);
                }
                else
                {
                    return BadRequest(errorMessage);    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el proveedor");
                return BadRequest("Error al eliminar el proveedor intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
            
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarProveedor(ProveedorViewModel proveedor)
        {
            try
            {
                var (success, errorMessage) = await _proveedorRepository.EditarProveedor(proveedor);
                if (success)
                {
                    return Ok(success);
                }
                else
                {
                    return BadRequest(errorMessage);
                }
                //var existeProveedor = await _context.Proveedores.FindAsync(proveedor.id);
                //if (existeProveedor == null)
                //{
                //    return NotFound("El proveedor que intenta editar no existe");
                //}
                //existeProveedor.NombreProveedor = proveedor.nombreProveedor;
                //existeProveedor.Contacto = proveedor.contacto;
                //existeProveedor.Direccion = proveedor.direccion;
                //_context.Entry(existeProveedor).State = EntityState.Modified;
                //await _context.SaveChangesAsync();
                //return Ok("Proveedor editado con exito");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar el pedido");
                return BadRequest("Error al editar el proveedor, intentelo de nuevo mas tarde si el problema persiste contacte con el administrador");
            }
            
        }
    }
}
