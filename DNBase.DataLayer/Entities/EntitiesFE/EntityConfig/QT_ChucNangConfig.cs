using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_ChucNangConfig : IEntityTypeConfiguration<QT_ChucNang>
    {
        public void Configure(EntityTypeBuilder<QT_ChucNang> builder)
        {
            builder.ToTable("QT_ChucNang");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.TenChucNang).HasColumnType("nvarchar").HasMaxLength(200).IsRequired(false);
            builder.Property(x => x.MaChucNang).HasColumnType("varchar").HasMaxLength(100).IsRequired(false);
            builder.Property(x => x.MoTa).HasColumnType("nvarchar").HasMaxLength(500).IsRequired(false);
        }
    }
}