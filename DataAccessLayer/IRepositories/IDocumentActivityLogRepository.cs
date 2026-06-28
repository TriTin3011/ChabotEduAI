using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer.Entities;

namespace DataAccessLayer.IRepositories
{
    public interface IDocumentActivityLogRepository
    {
        Task AddLogAsync(DocumentActivityLog log);
        Task<IEnumerable<DocumentActivityLog>> GetLogsBySubjectIdAsync(int subjectId);
    }
}
