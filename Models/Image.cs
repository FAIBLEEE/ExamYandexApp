using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamYandexApp.Models
{
    [Table("images")]
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }
        
        [Required]
        [Column("file_name")]
        public string? FileName { get; set; }
        
        [Column("original_file_name")]
        public string? OriginalFileName { get; set; }
        
        [Column("content_type")]
        public string? ContentType { get; set; }
        
        [Column("file_size")]
        public long FileSize { get; set; }
        
        [Column("upload_date")]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        [Column("object_storage_key")]
        public string? ObjectStorageKey { get; set; }
    }
}