namespace WAConectorAPI.Models.ModelCliente
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class ModelCliente : DbContext
    {
        public ModelCliente()
            : base("name=ModelCliente")
        {
        }

        public virtual DbSet<Login> Login { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>()
                .Property(e => e.CodCliente)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.NomCliente)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.Usuario)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SAPUser)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SAPPass)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SQLUser)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.ServerSQL)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.ServerLicense)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SQLPass)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SQLType)
                .IsUnicode(false);

            modelBuilder.Entity<Login>()
                .Property(e => e.SQLBD)
                .IsUnicode(false);
        }
    }
}
