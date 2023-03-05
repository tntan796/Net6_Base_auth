using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class CategoryItemConfig : IEntityTypeConfiguration<CategoryItem>
    {
        public void Configure(EntityTypeBuilder<CategoryItem> builder)
        {
            builder.ToTable("CategoryItems").HasOne<Category>(o => o.Category).WithMany().HasForeignKey(o => o.CategoryId).OnDelete(DeleteBehavior.NoAction);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.Name).HasColumnType("nvarchar").HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasColumnType("nvarchar").HasMaxLength(500);
            builder.Property(x => x.Code).HasColumnType("varchar").HasMaxLength(100).IsRequired();
        }
    }
}