using Microsoft.EntityFrameworkCore;
using ExamYandexApp.Models;

namespace ExamYandexApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Настройка для PostgreSQL
            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("images");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.FileName).HasColumnName("file_name");
                entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name");
                entity.Property(e => e.ContentType).HasColumnName("content_type");
                entity.Property(e => e.FileSize).HasColumnName("file_size");
                entity.Property(e => e.UploadDate)
                    .HasColumnName("upload_date")
                    .HasDefaultValueSql("NOW()");
                entity.Property(e => e.ObjectStorageKey).HasColumnName("object_storage_key");
            });
        }
    }
}