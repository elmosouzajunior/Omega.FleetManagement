using Omega.FleetManagement.Domain.Common;
using Omega.FleetManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Omega.FleetManagement.Domain.Entities
{
    public class Trip : Entity
    {
        [ForeignKey("Driver")]
        public Guid DriverId { get; private set; }

        [ForeignKey("Vehicle")]
        public Guid VehicleId { get; private set; }

        public string LoadingLocation { get; private set; }
        public string? UnloadingLocation { get; private set; }
        public DateTime LoadingDate { get; private set; }
        public DateTime? UnloadingDate { get; private set; }
        public decimal StartKm { get; private set; }
        public decimal FinishKm { get; private set; }
        public decimal FreightValue { get; private set; }
        public decimal CommissionPercent { get; private set; }
        public decimal CommissionValue { get; private set; }
        public TripStatus Status { get; private set; }
        public string? AttachmentPath { get; private set; }

        // Propriedades de navegação
        public virtual Driver Driver { get; private set; }
        public virtual Vehicle Vehicle { get; private set; }

        // Relacionamento com Despesas
        public virtual ICollection<Expense> Expenses { get; private set; }

        public Trip(
            Guid companyId,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal freightValue,
            decimal commissionPercent,
            string? attachmentPath) : base(companyId)
        {
            DriverId = driverId;
            VehicleId = vehicleId;
            LoadingLocation = loadingLocation;
            LoadingDate = loadingDate;
            StartKm = startKm;
            FreightValue = freightValue;
            CommissionPercent = commissionPercent;
            CommissionValue = (freightValue * commissionPercent) / 100;
            Status = TripStatus.Open;
            AttachmentPath = attachmentPath;
            Expenses = new List<Expense>();
        }

        protected Trip() : base(Guid.Empty)
        {
            Expenses = new List<Expense>();
        }

        public void Finish(string unloadingLocation, DateTime unloadingDate, decimal finishKm)
        {
            if (finishKm <= StartKm)
                throw new Exception("KM final não pode ser menor ou igual ao inicial.");

            UnloadingLocation = unloadingLocation;
            UnloadingDate = unloadingDate;
            FinishKm = finishKm;
            Status = TripStatus.Finished;
        }

        public void AddExpense(Expense expense)
        {
            if (this.Status == TripStatus.Cancelled)
                throw new ApplicationException("Não é possível adicionar despesas a uma viagem cancelada.");

            // Se a despesa for de uma empresa diferente da viagem, barramos aqui
            if (expense.CompanyId != this.CompanyId)
                throw new ApplicationException("Empresa da despesa diverge da empresa da viagem.");

            this.Expenses.Add(expense);
        }
    }
}