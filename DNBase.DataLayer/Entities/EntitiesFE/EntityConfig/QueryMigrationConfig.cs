using DNBase.DataLayer.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DNBase.DataLayer.FE.Configs
{
    public class QueryMigrationConfig : IEntityTypeConfiguration<QueryMigration>
    {
        public void Configure(EntityTypeBuilder<QueryMigration> builder)
        {
            builder.ToTable("_QueryMigrations");
            builder.Property(x => x.Id).HasDefaultValueSql("NEWID()");
            builder.Property(x => x.MigrationDate).HasDefaultValueSql("GETDATE()");
        }
    }
}