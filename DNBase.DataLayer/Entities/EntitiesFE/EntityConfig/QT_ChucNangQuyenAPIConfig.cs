using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_ChucNangQuyenAPIConfig : IEntityTypeConfiguration<QT_ChucNangQuyenAPI>
    {
        public void Configure(EntityTypeBuilder<QT_ChucNangQuyenAPI> builder)
        {
            builder.ToTable("QT_ChucNangQuyenAPI");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}