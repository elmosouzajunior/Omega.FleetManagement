namespace Omega.FleetManagement.Domain.Common
{
    public abstract class Entity
    {
        public Guid Id { get; protected set; }
        public Guid CompanyId { get; protected set; }
        public DateTime CreatedAt { get; private set; }

        protected Entity(Guid companyId)
        {
            Id = Guid.NewGuid();
            CompanyId = companyId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
