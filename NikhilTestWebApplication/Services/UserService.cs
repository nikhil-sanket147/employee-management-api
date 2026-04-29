using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NikhilTestWebApplication.Data;
using NikhilTestWebApplication.Interfaces;
using NikhilTestWebApplication.Models;
using OfficeOpenXml;
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

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var emailList = new List<string>();
                var invalidEmails = new List<string>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

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
                        // STEP 1 — Validate Header HERE
                        // ================================

                        var header = worksheet.Cells[1, 1].Text;

                        if (header != "Official Email Address")
                        {
                            response.IsSuccess = false;
                            response.Message =
                                "Invalid column header. Expected 'Official Email Address'";
                            return response;
                        }

                        // ================================
                        // STEP 2 — Read Emails HERE
                        // ================================

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var email = worksheet.Cells[row, 1].Text;

                            if (string.IsNullOrWhiteSpace(email))
                                continue;

                            // ================================
                            // STEP 3 — Validate Email HERE
                            // ================================

                            if (!IsValidEmail(email))
                            {
                                invalidEmails.Add(email);
                                continue;
                            }

                            emailList.Add(email);
                        }

                        // ================================
                        // STEP 4 — Remove Duplicates HERE
                        // ================================

                        emailList = emailList.Distinct().ToList();

                        // ================================
                        // STEP 5 — Set Response HERE
                        // ================================

                        response.IsSuccess = true;
                        response.FileName = file.FileName;
                        response.UploadedOn = DateTime.Now;
                        response.Message =
                            $"Total Rows: {rowCount - 1}, " +
                            $"Valid Emails: {emailList.Count}, " +
                            $"Invalid Emails: {invalidEmails.Count}";
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
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
    }
}
