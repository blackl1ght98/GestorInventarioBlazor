﻿using System;
using System.Collections.Generic;

namespace CRUDBlazor.Server.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string NombreProducto { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public string? Imagen { get; set; }

    public int Cantidad { get; set; }

    public double Precio { get; set; }

    public int? IdProveedor { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Proveedore? IdProveedorNavigation { get; set; }

    public virtual ICollection<ItemsDelCarrito> ItemsDelCarritos { get; set; } = new List<ItemsDelCarrito>();
}
