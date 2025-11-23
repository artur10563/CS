using LR6_GraphQL_ProductCatalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LR6_GraphQL_ProductCatalog.Api.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Translations> Translations { get; set; }
        public DbSet<Language> Languages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Language>().HasData(
                new Language { Id = 1, Code = "en", Name = "English" },
                new Language { Id = 2, Code = "uk", Name = "Ukrainian" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Sunflower Seed" },
                new Product { Id = 2, Name = "Pumpkin Seed" }
            );

            modelBuilder.Entity<Translations>().HasData(
                new Translations { Id = 1, ProductId = 1, LanguageId = 1, Text = "Sunflower Seed" },
                new Translations { Id = 2, ProductId = 1, LanguageId = 2, Text = "Насіння соняшника" },
                new Translations { Id = 3, ProductId = 2, LanguageId = 1, Text = "Pumpkin Seed" },
                new Translations { Id = 4, ProductId = 2, LanguageId = 2, Text = "Насіння гарбуза" }
            );

            modelBuilder.Entity<Translations>()
                .HasIndex(t => new { t.ProductId, t.LanguageId })
                .IsUnique();
        }
    }
}