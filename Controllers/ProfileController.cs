using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using UserProfileApp.Data;
using UserProfileApp.Models;

namespace UserProfileApp.Controllers
{
    /// <summary>
    /// Controller responsible for managing user profile-related actions like viewing, updating, and uploading profile pictures and files.
    /// </summary>
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\uploads");
        private readonly string _defaultProfilePicture = "/assets/default.jfif";
        private static int uploadStatusCount = 0;
        private static string userId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class.
        /// </summary>
        /// <param name="userManager">The user manager service for managing users.</param>
        /// <param name="context">The application database context.</param>
        public ProfileController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Displays the user profile.
        /// </summary>
        /// <returns>The profile view with the user's details and uploaded files.</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            // Retrieve user profile from the database
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            var userDirectory = Path.Combine(_uploadPath, userId);
            var profilePicturePath = Path.Combine(userDirectory, $"{userId}_profile.jpg");
            var profilePictureUrl = System.IO.File.Exists(profilePicturePath) ? $"/uploads/{userId}/{userId}_profile.jpg" : _defaultProfilePicture;

            var model = new UserProfile
            {
                UserId = userId,
                FirstName = userProfile?.FirstName,
                LastName = userProfile?.LastName,
                Address = userProfile?.Address,
                PhoneNumber = userProfile?.PhoneNumber,
                ProfilePicturePath = profilePictureUrl
            };

            // If directory exists, retrieve the files
            if (Directory.Exists(userDirectory))
            {
                var files = Directory.GetFiles(userDirectory)
                    .Select(filePath => new UserFile
                    {
                        FileName = Path.GetFileName(filePath),
                        FilePath = filePath
                    })
                    .ToList();

                model.Files = files;
            }

            return View(model);
        }

        /// <summary>
        /// Updates the user profile details.
        /// </summary>
        /// <param name="model">The user profile model with updated data.</param>
        /// <returns>Redirects to the profile view after updating the profile.</returns>
        [HttpPost]
        public async Task<IActionResult> Index(UserProfile model)
        {
            TempData["Submitted"] = string.Empty;

            var userDirectory = Path.Combine(_uploadPath, userId);
            model.UserId = userId;
            model.ProfilePicturePath = $"/uploads/{userId}/{userId}_profile.jpg";

            if (!ModelState.IsValid || uploadStatusCount < 2)
            {
                TempData["UploadStatus"] = uploadStatusCount < 2 ? "please upload the files" : null;
                return View(model);
            }

            var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);
            if (existingProfile != null)
            {
                // Update existing profile
                existingProfile.FirstName = model.FirstName;
                existingProfile.LastName = model.LastName;
                existingProfile.Address = model.Address;
                existingProfile.PhoneNumber = model.PhoneNumber;

                _context.UserProfiles.Update(existingProfile);
            }
            else
            {
                // Add new profile if it doesn't exist
                _context.UserProfiles.Add(model);
            }

            await _context.SaveChangesAsync();

            TempData["Submitted"] = "User Profile Successfully Updated!";
            return View(model);
        }

        /// <summary>
        /// Uploads a profile picture for the user.
        /// </summary>
        /// <param name="ProfilePicturePath">The profile picture file uploaded by the user.</param>
        /// <param name="model">The user profile model.</param>
        /// <returns>Redirects to the profile view after the upload.</returns>
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile ProfilePicturePath, UserProfile model)
        {
            if (ProfilePicturePath == null)
            {
                TempData["InvalidProfilePic"] = "Please select a profile picture to upload.";
                return RedirectToAction("Index");
            }

            var extension = Path.GetExtension(ProfilePicturePath.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".gif")
            {
                TempData["InvalidProfilePic"] = "Only image files (JPG, PNG, GIF) are allowed.";
                return RedirectToAction("Index");
            }

            var userDirectory = Path.Combine(_uploadPath, userId);
            var fileName = $"{userId}_profile.jpg";

            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            var filePath = Path.Combine(userDirectory, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePicturePath.CopyToAsync(stream);
            }

            uploadStatusCount++;
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Uploads a document file for the user.
        /// </summary>
        /// <param name="file">The document file uploaded by the user.</param>
        /// <param name="model">The user profile model.</param>
        /// <returns>Redirects to the profile view after the upload.</returns>
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, UserProfile model)
        {
            if (file == null)
            {
                TempData["InvalidFile"] = "Please select a file to upload.";
                return RedirectToAction("Index");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".docx" && extension != ".doc" && extension != ".pdf")
            {
                TempData["InvalidFile"] = "Only document files (DOCX, DOC, PDF) are allowed.";
                return RedirectToAction("Index");
            }

            var userDirectory = Path.Combine(_uploadPath, userId);
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            var filePath = Path.Combine(userDirectory, Path.GetFileName(file.FileName));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            uploadStatusCount++;
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Downloads a file for the user.
        /// </summary>
        /// <param name="fileName">The name of the file to download.</param>
        /// <returns>A file download response if the file exists, otherwise a 404 error.</returns>
        [HttpGet]
        public IActionResult DownloadFile(string fileName)
        {
            var userDirectory = Path.Combine(_uploadPath, userId);
            var filePath = Path.Combine(userDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Deletes a file for the user.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>Redirects to the profile view after deleting the file.</returns>
        [HttpGet]
        public IActionResult DeleteFile(string fileName)
        {
            var userDirectory = Path.Combine(_uploadPath, userId);
            var filePath = Path.Combine(userDirectory, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            return RedirectToAction("Index");
        }
    }
}
