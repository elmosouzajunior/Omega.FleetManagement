using Omega.FleetManagement.Domain.Common;

namespace Omega.FleetManagement.Domain.Entities
{
    public class Driver : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public string Cpf { get; private set; } = string.Empty;
        public decimal CommissionRate { get; private set; }
        public bool IsActive { get; private set; }
        public Guid UserId { get; private set; }
        public virtual ICollection<DriverCommission> Commissions { get; private set; } = new List<DriverCommission>();

        protected Driver() : base(Guid.Empty) { }

        public Driver(Guid companyId, Guid userId, string name, string cpf, decimal commissionRate)
            : this(companyId, userId, name, cpf, new[] { commissionRate })
        {
        }

        public Driver(Guid companyId, Guid userId, string name, string cpf, IEnumerable<decimal> commissionRates)
            : base(Guid.NewGuid())
        {
            CompanyId = companyId;
            UserId = userId;
            Name = name;
            Cpf = cpf.Replace(".", "").Replace("-", "");
            IsActive = true;
            SetCommissions(commissionRates);
        }

        public void Deactivate() => IsActive = false;

        public void UpdateCommission(decimal newRate) => CommissionRate = newRate;

        public IReadOnlyCollection<decimal> GetCommissionRates()
        {
            var rates = Commissions.Select(c => c.Rate).OrderBy(c => c).ToList();
            if (rates.Count == 0 && CommissionRate >= 0)
            {
                rates.Add(CommissionRate);
            }

            return rates.AsReadOnly();
        }

        public bool HasCommissionRate(decimal rate)
        {
            var normalizedRate = decimal.Round(rate, 2, MidpointRounding.AwayFromZero);
            return GetCommissionRates().Contains(normalizedRate);
        }

        public void LinkUser(Guid userId)
        {
            UserId = userId;
        }

        public void UpdateInfo(string name, string cpf, decimal commissionRate, bool isActive)
        {
            Name = name;
            Cpf = cpf.Replace(".", "").Replace("-", "");
            IsActive = isActive;
            CommissionRate = decimal.Round(commissionRate, 2, MidpointRounding.AwayFromZero);
        }

        private void SetCommissions(IEnumerable<decimal> commissionRates)
        {
            var normalizedRates = (commissionRates ?? Enumerable.Empty<decimal>())
                .Select(rate => decimal.Round(rate, 2, MidpointRounding.AwayFromZero))
                .Distinct()
                .OrderBy(rate => rate)
                .ToList();

            if (normalizedRates.Count == 0)
                throw new ArgumentException("Informe ao menos uma comissão para o motorista.");

            Commissions.Clear();
            foreach (var rate in normalizedRates)
            {
                var commission = new DriverCommission(rate);
                commission.AttachToDriver(Id);
                Commissions.Add(commission);
            }

            CommissionRate = normalizedRates[0];
        }
    }
}
