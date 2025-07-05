using InfraData.DAO;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraData.Repository.Configuration
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuario");

            builder.Property(u => u.Nome)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(u => u.CPF)
                   .HasMaxLength(11)
                   .IsRequired();
        }
    }
}
