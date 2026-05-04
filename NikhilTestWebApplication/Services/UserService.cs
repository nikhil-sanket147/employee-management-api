using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikhilTestWebApplication.Data;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text.RegularExpressions;

namespace NikhilTestWebApplication.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Users.Where(u => u.IsActive).ToListAsync();
        }

        public async Task<User?> GetById(Guid id)
        {
            return await _context.Users.Where(u => u.Id == id && u.IsActive).FirstOrDefaultAsync();
        }

        public async Task<User> Add(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Update(User user)
        {
            var existing = await _context.Users.FindAsync(user.Id);
            if (existing == null) return null;

            existing.Username = user.Username;
            existing.Email = user.Email;
            existing.Password = user.Password;
            existing.Role = user.Role;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            if (!user.IsActive) return false;

            user.IsActive = false;
            user.IsArchieved = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RestoreUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            if(user.IsActive) return false;

            user.IsActive = true;
            user.IsArchieved = false;

            await _context.SaveChangesAsync();

            return true;

        }


        public async Task<UploadFileModel> UploadFile(UploadFile uploadFile)
        {
            var response = new UploadFileModel();

            try
            {
                if (uploadFile == null || uploadFile.File == null)
                {
                    response.IsSuccess = false;
                    response.Message = "No file uploaded";
                    return response;
                }

                var file = uploadFile.File;

                // ================================
                // Validate File Type
                // ================================

                var allowedExtensions = new[] { ".xlsx" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    response.IsSuccess = false;
                    response.Message = "Only .xlsx files are allowed";
                    return response;
                }

                // ================================
                // Validate File Size (5MB)
                // ================================

                if (file.Length > 5 * 1024 * 1024)
                {
                    response.IsSuccess = false;
                    response.Message = "File size exceeds 5MB";
                    return response;
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var userToInsert = new List<User>();
                var invalidRows = new List<int>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        if (worksheet.Dimension == null)
                        {
                            response.IsSuccess = false;
                            response.Message = "Excel sheet is empty";
                            return response;
                        }

                        int rowCount = worksheet.Dimension.Rows;

                        // ================================
                        // Validate Headers
                        // ================================

                        var usernameheader = worksheet.Cells[1, 1].Text.Trim();
                        var emailheader = worksheet.Cells[1, 2].Text.Trim();
                        var passwordheader = worksheet.Cells[1, 3].Text.Trim();
                        var roleheader = worksheet.Cells[1, 4].Text.Trim();

                        if (usernameheader != "Username" ||
                            emailheader != "Email" ||
                            passwordheader != "Password" ||
                            roleheader != "Role")
                        {
                            response.IsSuccess = false;
                            response.Message =
                                "Invalid Excel format. Expected columns: Username, Email, Password, Role";

                            return response;
                        }

                        // ================================
                        // Load existing emails once
                        // ================================

                        var existingEmails = await _context.Users
                            .Select(u => u.Email.ToLower())
                            .ToListAsync();

                        var excelEmails = new HashSet<string>();

                        // ================================
                        // Read Rows
                        // ================================

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var username = worksheet.Cells[row, 1].Text.Trim();
                            var email = worksheet.Cells[row, 2].Text.Trim().ToLower();
                            var password = worksheet.Cells[row, 3].Text.Trim();
                            var role = worksheet.Cells[row, 4].Text.Trim();

                            if (string.IsNullOrWhiteSpace(username) ||
                                string.IsNullOrWhiteSpace(email) ||
                                string.IsNullOrWhiteSpace(password))
                            {
                                invalidRows.Add(row);
                                continue;
                            }

                            if (!IsValidEmail(email))
                            {
                                invalidRows.Add(row);
                                continue;
                            }

                            // Check duplicate in DB

                            if (existingEmails.Contains(email))
                            {
                                invalidRows.Add(row);
                                continue;
                            }

                            // Check duplicate in Excel

                            if (!excelEmails.Add(email))
                            {
                                invalidRows.Add(row);
                                continue;
                            }

                            var user = new User
                            {
                                Id = Guid.NewGuid(),
                                Username = username,
                                Email = email,

                                // Password hashing
                                Password = BCrypt.Net.BCrypt.HashPassword(password),

                                Role = string.IsNullOrWhiteSpace(role)
                                    ? "User"
                                    : role,

                                IsActive = true,
                                IsArchieved = false
                            };

                            userToInsert.Add(user);
                        }

                        // ================================
                        // Bulk Insert
                        // ================================

                        if (userToInsert.Any())
                        {
                            await _context.Users.AddRangeAsync(userToInsert);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();

                        // ================================
                        // Response
                        // ================================

                        response.IsSuccess = true;
                        response.FileName = file.FileName;
                        response.UploadedOn = DateTime.Now;

                        response.Message =
                            $"Total Rows: {rowCount - 1}, " +
                            $"Inserted Users: {userToInsert.Count}, " +
                            $"Invalid Rows: {string.Join(", ", invalidRows)}";
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;

                response.Message =
                    ex.InnerException?.Message ?? ex.Message;
            }

            return response;
        }
        private bool IsValidEmail(string email)
        {
            var pattern =
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, pattern);
        }

        public async Task<PagedResponse<List<User>>> GetUsersAsync(PaginationParams pagination)
        {
            var query = _context.Users.AsQueryable();

            //filtering 
            if (!string.IsNullOrEmpty(pagination.Search))
            {
                query = query.Where(p => p.Username.Contains(pagination.Search));
            }

            var totalRecords = await query.CountAsync();

            var users = await query.OrderBy(u => u.Id).Skip((pagination.PageNumber - 1) * pagination.PageSize).
                Take(pagination.PageSize).ToListAsync();

            return new PagedResponse<List<User>>(
                    users,pagination.PageNumber, pagination.PageSize, totalRecords);
        }

        public async Task<byte[]> ExportUsers ()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var users = await _context.Users.Where(u => u.IsActive).ToListAsync();

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Users");

            //header
            worksheet.Cells[1,1].Value = "Username";
            worksheet.Cells[1,2].Value = "Email";
            worksheet.Cells[1,3].Value = "Role";

            using (var range = worksheet.Cells[1,1,1,3])
            {
                range.Style.Font.Bold = true;
            }

            //data
            int row = 2;

            foreach(var user in users)
            {
                worksheet.Cells[row, 1].Value = user.Username;
                worksheet.Cells[row, 2].Value = user.Email;
                worksheet.Cells[row, 3].Value = user.Role;
                worksheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                row++;
            }

            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}
