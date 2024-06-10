using CRUDBlazor.Server.Interfaces.Infrastructure;
using CRUDBlazor.Server.Models;

namespace CRUDBlazor.Server.Infrastructure.Repositories
{
    public class NewStringGuid : INewStringGuid
    {
        //Llamamos a la base de datos para poder usarla

        private readonly DbcrudBlazorContext _context;
        //Creamos el contructor

        public NewStringGuid(DbcrudBlazorContext context)
        {
            _context = context;
        }
        //Agregamos el metodo que esta en la interfaz junto a  la tabla usuarios

        public async Task SaveNewStringGuid(Usuario operation)
        {
            //Actualizamos los datos directamente en la tabla usuarios

            _context.Usuarios.Update(operation);
            //Guardamos los cambios

            await _context.SaveChangesAsync();
        }
    }
}
