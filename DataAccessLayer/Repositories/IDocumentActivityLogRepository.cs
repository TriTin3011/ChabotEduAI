using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace DataAccessLayer.Repositories
{
    public interface IDocumentActivityLogRepository
    {
        Task AddLogAsync(DocumentActivityLog log);
        Task<IEnumerable<DocumentActivityLog>> GetLogsBySubjectIdAsync(int subjectId);
    }
}
