using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraData.DAO;

namespace InfraData.Repository.Configuration
{
    public class AluguelConfiguration : IEntityTypeConfiguration<Aluguel>
    {
        public void Configure(EntityTypeBuilder<Aluguel> builder)
        {
            builder.ToTable("Aluguel");

            builder
                .HasKey(o => o.Id)
                .HasName("pk_Aluguel");

            builder
                .Property(o => o.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasColumnType("int");

            builder.Property(o => o.ImovelId)
               .HasColumnName("imovel_id")
               .HasColumnType("int");

            builder.Property(o => o.UsuarioId)
                   .HasColumnName("usuario_id")
                   .HasColumnType("int");

            builder
                .Property(o => o.ValorLocacao)
                .HasColumnName("ValorLocacao")
                .HasColumnType("decimal");

            builder
                .Property(o => o.DataInicio)
                .HasColumnName("DataInicio")
                .HasColumnType("datetime2");

            builder
                .Property(o => o.DataTermino)
                .HasColumnName("DataTermino")
                .HasColumnType("datetime2");

            //entidade base

            builder
                .Property(o => o.DataAtualizacao)
                .HasColumnName("data_cadastro")
                .HasColumnType("datetime2");

            builder
                .Property(o => o.DataCriacao)
                .HasColumnName("datacriacao")
                .HasColumnType("datetime2");


            //
            builder.HasOne(a => a.Imovel)
                   .WithMany(i => i.Alugueis)
                   .HasForeignKey(a => a.ImovelId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Aluguel_Imovel");

            builder.HasOne(a => a.Usuario)
                   .WithMany(u => u.Alugueis)
                   .HasForeignKey(a => a.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Aluguel_Usuario");
        }
    }
}
