using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ApplicationDbContext _context;

        public SubjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetAllSubjectsAsync(bool includeDeleted = false)
        {
            var query = _context.Subjects
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.Documents)
                        .ThenInclude(d => d.Uploader)
                .Include(s => s.Documents)
                    .ThenInclude(d => d.Uploader)
                .Include(s => s.Lecturer)
                .AsQueryable();
                
            if (!includeDeleted)
            {
                query = query.Where(s => !s.IsDeleted);
            }
            
            return await query.AsSplitQuery().ToListAsync();
        }

        public async Task<Subject?> GetSubjectByIdAsync(int id)
        {
            return await _context.Subjects
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.Documents)
                        .ThenInclude(d => d.Uploader)
                .Include(s => s.Documents)
                    .ThenInclude(d => d.Uploader)
                .Include(s => s.Lecturer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Chapter>> GetChaptersBySubjectIdAsync(int subjectId)
        {
            return await _context.Chapters
                .Include(c => c.Documents)
                .Where(c => c.SubjectId == subjectId)
                .OrderBy(c => c.OrderIndex)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subject>> GetSubjectsByLecturerIdAsync(int lecturerId)
        {
            return await _context.Subjects
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.Documents)
                        .ThenInclude(d => d.Uploader)
                .Include(s => s.Documents)
                    .ThenInclude(d => d.Uploader)
                .Where(s => s.LecturerId == lecturerId && !s.IsDeleted)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task AddSubjectAsync(Subject subject)
        {
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubjectAsync(Subject subject)
        {
            _context.Subjects.Update(subject);
            await _context.SaveChangesAsync();
        }

        public async Task AddChapterAsync(Chapter chapter)
        {
            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateChapterAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
        }

        public async Task<Chapter?> GetChapterByIdAsync(int id)
        {
            return await _context.Chapters.FindAsync(id);
        }
    }
}
