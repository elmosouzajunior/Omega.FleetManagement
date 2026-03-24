using Omega.FleetManagement.Domain.Common;
using System.Drawing;

namespace Omega.FleetManagement.Domain.Entities
{
    public class Driver : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public decimal CommissionRate { get; private set; }
        public bool IsActive { get; private set; }
        public Guid UserId { get; private set; }
        protected Driver() : base(Guid.Empty) { } // Construtor protegido para EF Core

        public Driver(Guid companyId, Guid userId, string name, string cpf, decimal commissionRate) : base(Guid.NewGuid())
        {
            CompanyId = companyId;
            UserId = userId;
            Name = name;
            Cpf = cpf.Replace(".", "").Replace("-", ""); // Limpa a máscara
            CommissionRate = commissionRate;
            IsActive = true;
        }

        // Método para desativar (Regra de Negócio)
        public void Deactivate() => IsActive = false;

        // Método para atualizar comissão
        public void UpdateCommission(decimal newRate) => CommissionRate = newRate;

        public void UpdateInfo(string name, string cpf, decimal comissionRate, bool isActive)
        {
            Name = name;
            Cpf = cpf;
            CommissionRate = comissionRate;
            IsActive = isActive;
        }
    }
}
