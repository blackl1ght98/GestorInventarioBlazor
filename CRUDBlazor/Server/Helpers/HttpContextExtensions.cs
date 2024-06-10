using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Helpers
{
    public static class HttpContextExtensions
    {
        /*¿Que es un metodo de extension?
          Un método de extensión es un método estático especial en C# que, una vez definido, 
        puede ser llamado como si fuera un método de instancia en el tipo que se está extendiendo. 
        Esto permite añadir funcionalidades a tipos existentes sin tener que modificarlos, heredar de 
        ellos o reescribir el tipo. Aqui en este caso InsertarParametrosPaginacionRespuesta pasaria a formar parte
        de HttpContext y IQueryable
         */
        /*¿Que es <T>?
         * <T> es un marcador de posición para un tipo de parámetro en C#. Se utiliza en programación genérica 
         * donde T representa cualquier tipo. El tipo real se especifica cuando se utiliza el método o la clase. 
         * En tu código, InsertarParametrosPaginacionRespuesta<T> es un método genérico, lo que significa que 
         * puede trabajar con cualquier tipo T que sea IQueryable. El porque solo admite IQueryable es porque se
         * ha puesto asi IQueryable<T> la T que hemos puesto InsertarParametrosPaginacionRespuesta<T> en donde 
         * pongamos la otra T pues esa es la restriccion por ejemplo si en vez de IQueryable<T> se pusiese
         * List<T> solo aceptaria listas
         */
        /*¿Para que son usados los metodos de extension?
         Los métodos de extensión se utilizan para añadir funcionalidades a tipos existentes sin modificar el 
        código fuente de esos tipos, heredar de ellos o reescribir el tipo. Esto es útil cuando quieres añadir 
        métodos a un tipo que no controlas (por ejemplo, tipos en el marco .NET como puede ser String, List<T>, Dictionary, etc) o cuando quieres añadir métodos 
        que sólo tienen sentido en ciertos contextos específicos.
         */
        /*¿Que es IQueryable?
         Es una interfaz en .NET que se utiliza para implementar consultas de LINQ (Language Integrated Query) 
        contra bases de datos u otras fuentes de datos. Es una parte fundamental de Entity Framework, que es un 
        marco de trabajo de .NET para el acceso y la manipulación de datos.
        IQueryable<T> hereda de IEnumerable<T>, lo que significa que puedes usarlo para iterar sobre una 
        colección de objetos de tipo T
        IQueryable<T> también proporciona funcionalidades para crear y ejecutar consultas de forma dinámica.
         */
        /*¿Que hace HttpContext?
         * Es una clase en ASP.NET que encapsula toda la información específica de HTTP 
         * sobre una solicitud HTTP individual. Esto incluye detalles sobre la solicitud y 
         * la respuesta, los encabezados, las cookies, la sesión, y más.
         ¿Que hace this HttpContext?
        Cuando ves this HttpContext en el parámetro de un método, como en 
        InsertarParametrosPaginacionRespuesta<T>(this HttpContext context, ...), esto indica que el método es 
        un método de extensión.
        ¿Que es un metodo de extension?
        Un método de extensión es un método estático que se puede 
        llamar como si fuera un método de instancia en el tipo que se está extendiendo, 
        en este caso, HttpContext. La palabra clave this antes del tipo de parámetro indica que 
        el método es un método de extensión para ese tipo. Ejemplo:
        await HttpContext.InsertarParametrosPaginacionRespuesta(queryable, paginacion.CantidadAMostrar);
        ahora InsertarParametrosPaginacionRespuesta forma parte de HttpContext.
        Por lo tanto, this HttpContext context significa que el método InsertarParametrosPaginacionRespuesta<T> 
        es un método de extensión para la clase HttpContext. Esto permite que el método se llame en cualquier 
        instancia de HttpContext como si fuera un método definido en la clase HttpContext.
         */
        // Define un método de extensión estático y asíncrono llamado InsertarParametrosPaginacionRespuesta.
        // Este método es para el tipo HttpContext y puede trabajar con cualquier tipo T que sea IQueryable.
        public static async Task InsertarParametrosPaginacionRespuesta<T>(this HttpContext context, IQueryable<T> queryable, int cantidadRegistrosAMostrar)
        {
            // Comprueba si el contexto es nulo. Si es así, lanza una excepción.
            // Esto es para evitar errores más adelante si se intenta acceder a los métodos o propiedades de un objeto nulo.
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Usa el método CountAsync() para obtener el número total de elementos en queryable de forma asíncrona.
            // CountAsync() es un método de Entity Framework que cuenta el número de elementos en una secuencia de forma asíncrona.
            double conteo = await queryable.CountAsync();

            // Calcula el número total de páginas dividiendo el conteo por la cantidad de registros a mostrar y redondeando al entero más cercano hacia arriba.
            // Math.Ceiling() redondea un número decimal al entero más cercano hacia arriba.
            double totalPaginas = Math.Ceiling(conteo / cantidadRegistrosAMostrar);

            // Añade el número total de páginas como un encabezado de respuesta HTTP.
            // Esto permite que el cliente sepa cuántas páginas hay en total.
            context.Response.Headers.Add("totalPaginas", totalPaginas.ToString());
        }

    }
}
