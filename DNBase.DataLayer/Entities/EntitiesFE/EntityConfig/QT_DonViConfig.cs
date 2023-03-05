using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_DonViConfig : IEntityTypeConfiguration<QT_DonVi>
    {
        public void Configure(EntityTypeBuilder<QT_DonVi> builder)
        {
            builder.ToTable("QT_DonVi");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.TenDonVi).HasColumnType("nvarchar").HasMaxLength(250).IsRequired(false);
            builder.Property(x => x.MaDonVi).HasColumnType("varchar").HasMaxLength(100).IsRequired(false);
            builder.Property(x => x.GhiChu).HasColumnType("nvarchar").HasMaxLength(500).IsRequired(false);
        }
    }
}