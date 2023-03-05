using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class CRUDConfig : IEntityTypeConfiguration<CRUD>
    {
        public void Configure(EntityTypeBuilder<CRUD> builder)
        {
            builder.ToTable("CRUD");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.Title).HasColumnType("nvarchar").HasMaxLength(250).IsRequired();
            builder.Property(x => x.Code).HasColumnType("nvarchar").HasMaxLength(50).IsRequired();
        }
    }
}