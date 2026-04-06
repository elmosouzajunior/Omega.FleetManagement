using Omega.FleetManagement.Domain.Common;
using Omega.FleetManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Omega.FleetManagement.Domain.Entities
{
    public class Trip : Entity
    {
        [ForeignKey("Product")]
        public Guid? ProductId { get; private set; }
        [ForeignKey("Driver")]
        public Guid DriverId { get; private set; }

        [ForeignKey("Vehicle")]
        public Guid VehicleId { get; private set; }

        public string? ProductName { get; private set; }
        public string? ClientName { get; private set; }
        public string LoadingLocation { get; private set; } = string.Empty;
        public string? UnloadingLocation { get; private set; }
        public DateTime LoadingDate { get; private set; }
        public DateTime? UnloadingDate { get; private set; }
        public decimal StartKm { get; private set; }
        public decimal TonValue { get; private set; }
        public decimal LoadedWeightTons { get; private set; }
        public decimal? UnloadedWeightTons { get; private set; }
        public decimal FinishKm { get; private set; }
        public decimal FreightValue { get; private set; }
        public decimal? DieselKmPerLiter { get; private set; }
        public decimal? ArlaKmPerLiter { get; private set; }
        public decimal CommissionPercent { get; private set; }
        public decimal CommissionValue { get; private set; }
        public TripStatus Status { get; private set; }
        public string? AttachmentPath { get; private set; }

        // Propriedades de navegação
        public virtual Product? Product { get; private set; }
        public virtual Driver Driver { get; private set; } = null!;
        public virtual Vehicle Vehicle { get; private set; } = null!;

        // Relacionamento com Despesas
        public virtual ICollection<Expense> Expenses { get; private set; } = new List<Expense>();

        public Trip(
            Guid companyId,
            Guid? productId,
            string? productName,
            string? clientName,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
            decimal freightValue,
            decimal commissionPercent,
            string? attachmentPath) : base(companyId)
        {
            ProductId = productId;
            ProductName = productName;
            ClientName = clientName;
            DriverId = driverId;
            VehicleId = vehicleId;
            LoadingLocation = loadingLocation;
            UnloadingLocation = unloadingLocation;
            LoadingDate = loadingDate;
            StartKm = startKm;
            TonValue = tonValue;
            LoadedWeightTons = loadedWeightTons;
            FreightValue = freightValue;
            CommissionPercent = commissionPercent;
            CommissionValue = (freightValue * commissionPercent) / 100;
            Status = TripStatus.Open;
            AttachmentPath = attachmentPath;
        }

        protected Trip() : base(Guid.Empty)
        {
        }

        public void UpdateOpening(
            Guid? productId,
            string? productName,
            string? clientName,
            Guid driverId,
            Guid vehicleId,
            string loadingLocation,
            string unloadingLocation,
            DateTime loadingDate,
            decimal startKm,
            decimal tonValue,
            decimal loadedWeightTons,
            decimal freightValue)
        {
            if (Status != TripStatus.Open)
                throw new ApplicationException("Somente viagens abertas podem ter a abertura editada.");

            ProductId = productId;
            ProductName = productName;
            ClientName = clientName;
            DriverId = driverId;
            VehicleId = vehicleId;
            LoadingLocation = loadingLocation;
            UnloadingLocation = unloadingLocation;
            LoadingDate = loadingDate;
            StartKm = startKm;
            TonValue = tonValue;
            LoadedWeightTons = loadedWeightTons;
            FreightValue = freightValue;
            CommissionValue = (freightValue * CommissionPercent) / 100;
        }

        public void Finish(string unloadingLocation, DateTime unloadingDate, decimal finishKm, decimal unloadedWeightTons, decimal freightValue, decimal? dieselKmPerLiter, decimal? arlaKmPerLiter)
        {
            if (Status != TripStatus.Open)
                throw new ApplicationException("Apenas viagens abertas podem ser finalizadas.");

            if (finishKm <= StartKm)
                throw new Exception("KM final não pode ser menor ou igual ao inicial.");

            UnloadingLocation = unloadingLocation;
            UnloadingDate = unloadingDate;
            FinishKm = finishKm;
            UnloadedWeightTons = unloadedWeightTons;
            FreightValue = freightValue;
            CommissionValue = (freightValue * CommissionPercent) / 100;
            DieselKmPerLiter = dieselKmPerLiter;
            ArlaKmPerLiter = arlaKmPerLiter;
            Status = TripStatus.Finished;
        }

        public void Reopen()
        {
            if (Status != TripStatus.Finished)
                throw new ApplicationException("Apenas viagens finalizadas podem ser reabertas.");

            Status = TripStatus.Open;
            UnloadingDate = null;
            UnloadingLocation = null;
            UnloadedWeightTons = null;
            FinishKm = 0;
        }

        public void Cancel()
        {
            if (Status == TripStatus.Cancelled)
                throw new ApplicationException("A viagem já está cancelada.");

            if (Status != TripStatus.Open)
                throw new ApplicationException("Apenas viagens abertas podem ser canceladas.");

            Status = TripStatus.Cancelled;
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
