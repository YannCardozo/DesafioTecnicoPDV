using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfraData.DAO;
using InfraData.Repository.Configuration;

namespace InfraData.Context
{
    public class Contexto : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        //string de conexao.
        //sql server monsterasp
        //string conexao_banco = "Server=db22815.databaseasp.net;Database=db22815;UserId=db22815;Password=nP#36yS+e%7M;Encrypt=False;MultipleActiveResultSets=True;";
        public Contexto(DbContextOptions<Contexto> options)
           : base(options)
        {
        }

        public DbSet<Imoveis> Imoveis { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema("Imobiliaria");

            builder.ApplyConfigurationsFromAssembly(typeof(Contexto).Assembly);
            base.OnModelCreating(builder);
        }
    }
}
