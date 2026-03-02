namespace Omega.FleetManagement.Domain.Entities
{
    public class CompanyAdmin
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; private set; } = true;


        // Vínculo com a Empresa
        public Guid CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;

        // ID que liga ao AspNetUser (opcional, mas ajuda na rastreabilidade)
        public string? IdentityUserId { get; set; }
    }
}
