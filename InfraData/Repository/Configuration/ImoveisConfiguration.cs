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
    public class ImoveisConfiguration : IEntityTypeConfiguration<Imoveis>
    {
        public void Configure(EntityTypeBuilder<Imoveis> builder)
        {
            builder.ToTable("Imovel");

            builder
                .HasKey(o => o.Id)
                .HasName("pk_Imovel");

            builder
                .Property(o => o.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id")
                .HasColumnType("int");

            builder
                .Property(o => o.Tipo)
                .HasColumnName("Tipo")
                .HasColumnType("varchar")
                .HasMaxLength(100);


            builder
                .Property(o => o.Endereco)
                .HasColumnName("Endereco")
                .HasColumnType("varchar")
                .HasMaxLength(255);

            builder
                .Property(o => o.Status)
                .HasColumnName("Status")
                .HasColumnType("varchar")
                .HasMaxLength(20);

            builder
                .Property(o => o.ValorLocacao)
                .HasColumnName("ValorLocacao")
                .HasColumnType("decimal");

            //entidade base

            builder
                .Property(o => o.DataAtualizacao)
                .HasColumnName("data_cadastro")
                .HasColumnType("datetime2");

            builder
                .Property(o => o.DataCriacao)
                .HasColumnName("datacriacao")
                .HasColumnType("datetime2");

            builder
                .Property(o => o.ImagemBase64)
                .HasColumnName("ImagemBase64")
                .HasColumnType("varchar(max)");


            //RELACIONAMENTOS
            builder.HasMany(i => i.Alugueis)
                   .WithOne(a => a.Imovel)
                   .HasForeignKey(a => a.ImovelId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Aluguel_Imovel");

        }
    }

}
