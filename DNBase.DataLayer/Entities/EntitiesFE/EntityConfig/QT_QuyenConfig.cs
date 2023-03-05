using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_QuyenConfig : IEntityTypeConfiguration<QT_Quyen>
    {
        public void Configure(EntityTypeBuilder<QT_Quyen> builder)
        {
            builder.ToTable("QT_Quyen");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.TenQuyen).HasColumnType("nvarchar").HasMaxLength(250).IsRequired(false);
            builder.Property(x => x.MaQuyen).HasColumnType("varchar").HasMaxLength(100).IsRequired(false);
            builder.Property(x => x.MoTa).HasColumnType("nvarchar").HasMaxLength(500).IsRequired(false);
        }
    }
}