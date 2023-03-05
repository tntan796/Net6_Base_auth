using DNBase.DataLayer.EF.Entities;
using DNBase.DataLayer.FE.Configs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace DNBase.DataLayer.EF
{
    public class AppDbContext : AppDbContext<AppUser, AppRole>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    public class AppDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, Guid>
        where TUser : AppUser
        where TRole : AppRole
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<QueryMigration> QueryMigrations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryItem> CategoryItems { get; set; }
        public DbSet<Files> Files { get; set; }
        public DbSet<AppLog> AppLog { get; set; }
        public DbSet<CRUD> CRUDs { get; set; }
        public DbSet<QT_ChucNang> QT_ChucNang { get; set; }
        public DbSet<QT_ChucNangQuyenAPI> QT_ChucNangQuyenAPI { get; set; }
        public DbSet<QT_DanhSachAPI> QT_DanhSachAPI { get; set; }
        public DbSet<QT_DonVi> QT_DonVi { get; set; }
        public DbSet<QT_Quyen> QT_Quyen { get; set; }
        public DbSet<QT_VaiTro_NguoiSuDung> QT_VaiTro_NguoiSuDung { get; set; }
        public DbSet<QT_VaiTroQuyen> QT_VaiTroQuyen { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Configure using Fluent API
            modelBuilder.ApplyConfiguration(new AppUserConfig());
            modelBuilder.ApplyConfiguration(new AppRoleConfig());
            modelBuilder.ApplyConfiguration(new QueryMigrationConfig());
            modelBuilder.ApplyConfiguration(new CategoryConfig());
            modelBuilder.ApplyConfiguration(new CategoryItemConfig());
            modelBuilder.ApplyConfiguration(new FileConfig());
            modelBuilder.ApplyConfiguration(new AppLogConfig());
            modelBuilder.ApplyConfiguration(new CRUDConfig());
            modelBuilder.ApplyConfiguration(new QT_ChucNangConfig());
            modelBuilder.ApplyConfiguration(new QT_ChucNangQuyenAPIConfig());
            modelBuilder.ApplyConfiguration(new QT_DanhSachAPIConfig());
            modelBuilder.ApplyConfiguration(new QT_DonViConfig());
            modelBuilder.ApplyConfiguration(new QT_QuyenConfig());
            modelBuilder.ApplyConfiguration(new QT_VaiTro_NguoiSuDungConfig());
            modelBuilder.ApplyConfiguration(new QT_VaiTroQuyenConfig());

            modelBuilder.Ignore<IdentityUserLogin<Guid>>();
            modelBuilder.Ignore<IdentityUserClaim<Guid>>();
            modelBuilder.Ignore<IdentityUserRole<Guid>>();
            modelBuilder.Ignore<IdentityRoleClaim<Guid>>();
            modelBuilder.Ignore<IdentityUserToken<Guid>>();
        }
    }
}