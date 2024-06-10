using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CRUDBlazor.Shared.Pedidos
{
    public enum EstadoPedido
    {
        [Description("Pendiente de pago")]
        PedienteDePago,
        Pagado,
        [Description("En Proceso")]
        EnProceso,
        Enviado,
        Recibido
    }
    public static class ObtenerValorEnumeracion
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }


    }
}
