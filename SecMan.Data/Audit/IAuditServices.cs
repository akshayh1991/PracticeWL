using SecMan.Model;

namespace SecMan.Data.Audit
{
    public interface IAuditServices
    {
        Task APIAudit(AuditDto model);
    }
}