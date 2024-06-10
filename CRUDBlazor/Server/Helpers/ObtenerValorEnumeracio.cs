using System.ComponentModel;
using System.Reflection;

namespace CRUDBlazor.Server.Helpers
{
    public static class ObtenerValorEnumeracio
    {
        //Metodo de extension para enumeraciones

        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }


    }
}
