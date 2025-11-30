using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamYandexApp.Models;
using ExamYandexApp.Data;
using ExamYandexApp.Services;

namespace ExamYandexApp.Controllers
{
    public class ImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IObjectStorageService _storageService;

        public ImagesController(ApplicationDbContext context, IObjectStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.Images
                .OrderByDescending(i => i.UploadDate)
                .ToListAsync();
            
            // Получаем URL для каждого изображения
            foreach (var image in images)
            {
                if (!string.IsNullOrEmpty(image.FileName))
                {
                    image.ObjectStorageKey = _storageService.GetFileUrl(image.FileName);
                }
            }

            return View(images);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("", "Please select an image file.");
                return View();
            }

            try
            {
                // Генерируем уникальное имя файла
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                
                // Загружаем файл в Object Storage
                var storageKey = await _storageService.UploadFileAsync(file, fileName);

                // Сохраняем метаданные в базу данных
                var image = new Image
                {
                    FileName = fileName,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    ObjectStorageKey = storageKey
                };

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Image uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View();
            }
        }
    }
}