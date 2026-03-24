namespace Omega.FleetManagement.Application.DTOs
{
    public class UpdateCompanyRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
