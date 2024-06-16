﻿using System;
using System.Collections.Generic;

namespace CRUDBlazor.Server.Models;

public partial class DetalleHistorialProducto
{
    public int Id { get; set; }

    public int? HistorialProductoId { get; set; }

    public int? Cantidad { get; set; }

    public string? NombreProducto { get; set; }

    public string? Descripcion { get; set; }

    public decimal? Precio { get; set; }

    public virtual HistorialProducto? HistorialProducto { get; set; }
}
