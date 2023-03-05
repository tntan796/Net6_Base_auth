using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_DanhSachAPIConfig : IEntityTypeConfiguration<QT_DanhSachAPI>
    {
        public void Configure(EntityTypeBuilder<QT_DanhSachAPI> builder)
        {
            builder.ToTable("QT_DanhSachAPI");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}