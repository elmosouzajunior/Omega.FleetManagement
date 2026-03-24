using Omega.FleetManagement.Domain.Common;

namespace Omega.FleetManagement.Domain.Entities
{
    public class Vehicle : Entity
    {
        public string LicensePlate { get; private set; }
        public string Manufacturer { get; private set; }
        public string? Color { get; private set; }
        public bool IsActive { get; private set; }

        // FK e Propriedade de Navegação
        public Guid? DriverId { get; private set; }
        public virtual Driver? Driver { get; private set; } // O virtual permite o Lazy Loading se necessário
        public virtual ICollection<Expense> Expenses { get; private set; }

        // Construtor para o EF Core
        protected Vehicle() : base(Guid.Empty)
        {
            // Inicializamos as strings para silenciar o aviso de non-nullable
            LicensePlate = null!;
            Manufacturer = null!;
            Expenses = new List<Expense>();
        }

        public Vehicle(Guid companyId, string licensePlate, string manufacturer, string color) : base(companyId)
        {
            LicensePlate = licensePlate;
            Manufacturer = manufacturer;
            Color = color;
            IsActive = true;
            Expenses = new List<Expense>();
        }

        // --- Regras de Negócio ---

        public void AssignDriver(Guid driverId)
        {
            DriverId = driverId;
        }
                
        public void ReleaseVehicle()
        {
            DriverId = null;
        }

        public void Deactivate() => IsActive = false;

        public void UpdateInfo(string model, string manufacturer, string color, bool isActive)
        {
            Manufacturer = manufacturer;
            Color = color;
            IsActive = isActive;
        }
    }
}
