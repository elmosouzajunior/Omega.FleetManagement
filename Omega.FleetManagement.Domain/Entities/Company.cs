namespace Omega.FleetManagement.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string? Cnpj { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Relacionamentos (Navegação)
        public ICollection<CompanyAdmin> Users { get; set; } = new List<CompanyAdmin>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Driver> Drivers { get; set; } = new List<Driver>();

        public Company(string name, string cnpj)
        {
            Name = name;
            Cnpj = cnpj;
            IsActive = true;
        }
    }
}