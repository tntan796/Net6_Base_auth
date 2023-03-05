using System;

namespace DNBase.DataLayer.EF.Entities
{
    public class QueryMigration : Entity<Guid>
    {
        public string Name { get; set; }
        public DateTime MigrationDate { get; set; }
    }
}