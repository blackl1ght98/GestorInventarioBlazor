using CRUDBlazor.Shared;

namespace CRUDBlazor.Server.Helpers
{
    public static class QueryableExtensions
    {
        /* 
   * Paginar es un método de extensión para cualquier tipo que implemente la interfaz IQueryable<T>, donde T puede ser cualquier tipo.
   * Este método sólo puede ser llamado en objetos que sean IQueryable<T>.
   * Por ejemplo, cuando haces var queryable = _context.Usuarios.Include(x=>x.IdRolNavigation).AsQueryable(), 
   * queryable es de tipo IQueryable<Usuario> y puedes llamar a queryable.Paginar(paginacion).
   */
        // Define un método de extensión estático llamado Paginar.
        // Este método es para cualquier tipo que implemente la interfaz IQueryable<T>, donde T puede ser cualquier tipo.
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, Paginacion paginacion)
        {
            // Calcula cuántos elementos se deben saltar en la secuencia de datos.
            // Esto se hace restando 1 a la página actual y multiplicando por la cantidad de registros a mostrar.
            // Por ejemplo, si estás en la página 2 y muestras 10 registros por página, debes saltar los primeros 10 registros.
            //como te has cambiado a la pagina 2 salta los 10 de la pagina 1
            int skip = (paginacion.Pagina - 1) * paginacion.CantidadAMostrar;

            // Usa el método Skip para saltar los primeros 'skip' elementos de la secuencia de datos.
            // Luego, usa el método Take para obtener los siguientes 'paginacion.CantidadAMostrar' elementos.
            // Esto te da los elementos que deben mostrarse en la página actual.
            //como te has cambiado a la pagina 2 salta los 10 de la pagina 1 y añade 10 registros de la pagina 2

            return queryable.Skip(skip).Take(paginacion.CantidadAMostrar);
        }

    }
}
