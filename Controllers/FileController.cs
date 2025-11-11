using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Table_Reservation.Models;
using System.Text;

public class FileController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FileController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }




    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPdf(IFormFile fileFr, IFormFile fileEn, IFormFile fileNl)
    {
        const long maxSizeBytes = 10 * 1024 * 1024; // 10MB
        bool IsPdf(IFormFile f) => f != null && f.ContentType == "application/pdf";
        bool SizeOk(IFormFile f) => f != null && f.Length > 0 && f.Length <= maxSizeBytes;

        if ((fileFr == null || fileFr.Length == 0) ||
            (fileEn == null || fileEn.Length == 0) ||
            (fileNl == null || fileNl.Length == 0))
        {
            return BadRequest("Veuillez sélectionner tous les fichiers PDF.");
        }

        if (!IsPdf(fileFr) || !IsPdf(fileEn) || !IsPdf(fileNl))
        {
            return BadRequest("Type de fichier invalide. Uniquement PDF.");
        }

        if (!SizeOk(fileFr) || !SizeOk(fileEn) || !SizeOk(fileNl))
        {
            return BadRequest("Fichier trop volumineux (max 10MB).");
        }

        try
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Upload des trois fichiers PDF avec des noms spécifiques
            await SavePdfFileNoCommit(fileFr, "menu_fr.pdf", uploadsFolder);
            await SavePdfFileNoCommit(fileEn, "menu_en.pdf", uploadsFolder);
            await SavePdfFileNoCommit(fileNl, "menu_nl.pdf", uploadsFolder);

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewPdf");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur UploadPdf: {ex}");
            return StatusCode(500, "Erreur serveur lors de l'upload des PDFs.");
        }
    }

    private async Task SavePdfFileNoCommit(IFormFile file, string fileName, string folderPath)
    {
        string filePath = Path.Combine(folderPath, fileName);

        // Vérifier si un fichier existe déjà et le supprimer avant d'uploader le nouveau
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        // Vérifier magic number PDF (%PDF) tolérant
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if (bytes.Length < 4 ||
            !(bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)) // %PDF
        {
            throw new InvalidDataException("Le fichier n'est pas un PDF valide.");
        }
        await System.IO.File.WriteAllBytesAsync(filePath, bytes);

        // Mettre à jour la base de données avec le nouveau fichier
        var existingPdf = _context.PdfFiles.FirstOrDefault(p => p.FileName == fileName);
        if (existingPdf != null)
        {
            existingPdf.FilePath = $"/uploads/{fileName}";
            _context.PdfFiles.Update(existingPdf);
        }
        else
        {
            _context.PdfFiles.Add(new PdfFileModel { FileName = fileName, FilePath = $"/uploads/{fileName}" });
        }
    }

    public IActionResult ViewPdf(string lang = "fr")
    {
        string filePath = $"/uploads/menu_{lang}.pdf";

        if (!System.IO.File.Exists(Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'))))
        {
            return Content("Aucun fichier PDF disponible pour cette langue.");
        }

        return View("ViewPdf", filePath);
    }
}
