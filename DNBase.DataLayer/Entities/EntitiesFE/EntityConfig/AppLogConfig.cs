using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class AppLogConfig : IEntityTypeConfiguration<AppLog>
    {
        public void Configure(EntityTypeBuilder<AppLog> builder)
        {
            builder.ToTable("AppLog");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
        }
    }
}