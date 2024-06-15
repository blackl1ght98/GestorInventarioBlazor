using CRUDBlazor.Shared.Productos;
using CRUDBlazor.Shared.Proveedores;
using CRUDBlazor.Shared;

namespace CRUDBlazor.Client.Services.Producto
{
    public class ApiProperties
    {
        /*Debido a la arquitectura de Blazor, el servidor y el cliente están separados, lo que significa que debemos manejar los datos de una
  * manera que ambos puedan entender. Cuando consultas algo desde el cliente al servidor, debes asegurarte de que los datos estén formateados
  * de manera que puedan ser mostrados correctamente.
  * Cuando recibes datos del servidor, estos vienen en formato JSON. En JSON, todas las propiedades comienzan con una letra minúscula.
  * Por lo tanto, si en tu modelo ProductosViewModel nombras una propiedad como "Nombre", no será reconocida. Debes nombrarla como "nombre"
  *  para que sea detectada correctamente. La razón de esto es la convención de nomenclatura de las propiedades en los archivos JSON.
 */
        public ProductosViewModel producto = new ProductosViewModel();
        // Crea un array de productos para poder mostrarlos en la interfaz de usuario.
        public ProductosViewModel[] productos;
        //Crea un array inicializado en 0
        public ProveedorViewModel[] proveedores = new ProveedorViewModel[0];
        // Configuración para agregar la paginación. Esto permite al usuario navegar a través de las páginas de productos.
        public Paginacion paginacion = new Paginacion();
        // Crea un diccionario que almacena las cantidades de cada producto que se añade al carrito. El número inicial es 1 porque esta es 
        // la cantidad predeterminada.
        public Dictionary<int, int> cantidades = new Dictionary<int, int>();
        // Configuración para agregar la paginación. Estas variables mantienen un seguimiento de la página actual, el total de páginas.
        public int paginaActual = 1;
        public int paginasTotales;
        //Mostrar el mensaje de error
        public bool showAlert = false;
        public string alertMessage = "";
        //Parametros de busqueda
        public string buscar;
        public string ordenarPorPrecio;
        public string ordenarPorPrecioAnterior;
        public int idProveedor;
        public int idProveedorAnterior;
    }
}
