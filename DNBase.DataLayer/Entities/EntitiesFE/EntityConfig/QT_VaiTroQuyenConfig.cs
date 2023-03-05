using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QT_VaiTroQuyenConfig : IEntityTypeConfiguration<QT_VaiTroQuyen>
    {
        public void Configure(EntityTypeBuilder<QT_VaiTroQuyen> builder)
        {
            builder.ToTable("QT_VaiTroQuyen");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}