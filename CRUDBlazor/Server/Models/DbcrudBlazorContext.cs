using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CRUDBlazor.Server.Models;

public partial class DbcrudBlazorContext : DbContext
{
    public DbcrudBlazorContext()
    {
    }

    public DbcrudBlazorContext(DbContextOptions<DbcrudBlazorContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<DetallePedido> DetallePedidos { get; set; }

    public virtual DbSet<ItemsDelCarrito> ItemsDelCarritos { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedore> Proveedores { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-2TL9C3O\\SQLEXPRESS;Initial Catalog=DBCrudBlazor;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Carrito__3214EC078850E6E0");

            entity.ToTable("Carrito");

            entity.Property(e => e.FechaCreacion).HasColumnType("datetime");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK__Carrito__Usuario__29221CFB");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PK__Departam__787A433DD548BDAA");

            entity.ToTable("Departamento");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DetalleP__3214EC07B8355AD5");

            entity.ToTable("DetallePedido");

            entity.HasOne(d => d.Pedido).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.PedidoId)
                .HasConstraintName("FK__DetallePe__Pedid__18EBB532");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetallePedidos)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK__DetallePe__Produ__19DFD96B");
        });

        modelBuilder.Entity<ItemsDelCarrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemsDel__3214EC075EEA68F4");

            entity.ToTable("ItemsDelCarrito");

            entity.HasOne(d => d.Carrito).WithMany(p => p.ItemsDelCarritos)
                .HasForeignKey(d => d.CarritoId)
                .HasConstraintName("FK__ItemsDelC__Carri__2BFE89A6");

            entity.HasOne(d => d.Producto).WithMany(p => p.ItemsDelCarritos)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK__ItemsDelC__Produ__2CF2ADDF");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pedidos__3214EC07421C452D");

            entity.Property(e => e.EstadoPedido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaPedido).HasColumnType("date");
            entity.Property(e => e.NumeroPedido)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_IdUsuario");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3214EC07294E6792");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Imagen).IsUnicode(false);
            entity.Property(e => e.NombreProducto)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK_IdProveedor");
        });

        modelBuilder.Entity<Proveedore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Proveedo__3214EC07C5A774F6");

            entity.Property(e => e.Contacto)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreProveedor)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07233713E8");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC07DC185A17");

            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EnlaceCambioPass).HasMaxLength(50);
            entity.Property(e => e.FechaEnlaceCambioPass).HasColumnType("datetime");
            entity.Property(e => e.FechaNacimiento).HasColumnType("date");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TemporaryPassword).IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdRol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
