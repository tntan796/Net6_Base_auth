using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_VaiTro_NguoiSuDungConfig : IEntityTypeConfiguration<QT_VaiTro_NguoiSuDung>
    {
        public void Configure(EntityTypeBuilder<QT_VaiTro_NguoiSuDung> builder)
        {
            builder.ToTable("QT_VaiTro_NguoiSuDung");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}