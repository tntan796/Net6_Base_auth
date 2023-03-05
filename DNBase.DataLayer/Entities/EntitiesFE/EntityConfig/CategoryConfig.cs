using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.Name).HasColumnType("nvarchar").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnType("nvarchar").HasMaxLength(500);
            builder.Property(x => x.Code).HasColumnType("varchar").HasMaxLength(100).IsRequired();
        }
    }
}