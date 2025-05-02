// Controllers/UploadController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartCowork.Services.Space.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("image")]
        [Authorize(Roles = "Admin")] // Seuls les administrateurs peuvent télécharger des images
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Début de la requête d'upload d'image");

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Tentative d'upload sans fichier");
                    return BadRequest("Aucun fichier n'a été envoyé");
                }

                // Vérifier le type de fichier
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning($"Type de fichier non autorisé: {file.ContentType}");
                    return BadRequest("Type de fichier non autorisé. Seuls JPEG, PNG et GIF sont acceptés");
                }

                _logger.LogInformation($"Fichier validé: {file.FileName}, taille: {file.Length} bytes, type: {file.ContentType}");

                // Créer un nom de fichier unique
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

                // Chemin où les images seront stockées
                var contentRootPath = _environment.ContentRootPath;
                var webRootPath = _environment.WebRootPath;

                _logger.LogInformation($"ContentRootPath: {contentRootPath}");
                _logger.LogInformation($"WebRootPath: {webRootPath}");

                // Si WebRootPath est null, utiliser un sous-dossier dans ContentRootPath
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(contentRootPath, "wwwroot");
                    Directory.CreateDirectory(webRootPath); // Créer le dossier s'il n'existe pas
                    _logger.LogInformation($"WebRootPath créé: {webRootPath}");
                }

                var uploadsFolder = Path.Combine(webRootPath, "uploads", "spaces");

                // Créer le dossier s'il n'existe pas
                if (!Directory.Exists(uploadsFolder))
                {
                    _logger.LogInformation($"Création du dossier: {uploadsFolder}");
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);
                _logger.LogInformation($"Chemin du fichier: {filePath}");

                // Sauvegarder le fichier
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    _logger.LogInformation("Fichier sauvegardé avec succès");
                }

                // Retourner l'URL de l'image
                var imageUrl = $"/uploads/spaces/{fileName}";
                _logger.LogInformation($"URL de l'image: {imageUrl}");

                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'upload de l'image");
                return StatusCode(500, new { message = "Une erreur s'est produite lors de l'upload de l'image", details = ex.Message });
            }
        }
    }
}