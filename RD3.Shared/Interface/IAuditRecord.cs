using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public interface IAuditRecord
    {
        public void AddAuditRecord(string module,AuditAction auditAction, string oldValue, string newValue);
    }
}
