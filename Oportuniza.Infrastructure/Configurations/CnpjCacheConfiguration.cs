using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Oportuniza.Domain.Models;

public class CnpjCacheConfiguration : IEntityTypeConfiguration<CNPJCache>
{
    public void Configure(EntityTypeBuilder<CNPJCache> builder)
    {
        builder.ToTable("CnpjCache");

        builder.HasKey(x => x.Cnpj);

        builder.Property(x => x.Cnpj)
            .HasColumnType("char(14)")
            .IsRequired();

        builder.Property(x => x.Situacao)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AtualizadoEm)
            .IsRequired();
    }
}
