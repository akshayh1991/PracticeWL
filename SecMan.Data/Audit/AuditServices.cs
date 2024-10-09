using SecMan.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.Audit
{
    public class AuditServices : IAuditServices
    {
        public async Task APIAudit(AuditDto model)
        {
            using SQLCipher.Db db = new SQLCipher.Db();
            SQLCipher.APIAudit objAPIAudit = new SQLCipher.APIAudit()
            {
                ActionBy = model.ActionBy,
                APIDescription = model.APIDescription,
                APIResponseCode = model.APIResponseCode,
                Component = model.Component,
                Description = model.Description,
                EntityName = model.EntityName,
                ServerIP = model.ServerIP,
                Status = model.Status,
                ResponseTime = model.ResponseTime,
                RequestedDate = DateTime.Now,
            };
            await db.AddAsync(objAPIAudit);
            await db.SaveChangesAsync();
        }
    }



}
