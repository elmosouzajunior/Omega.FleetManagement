using Omega.FleetManagement.Domain.Common;

namespace Omega.FleetManagement.Domain.Entities;

public class ReceiptDocumentType : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ICollection<Trip> Trips { get; private set; }

    public ReceiptDocumentType(Guid companyId, string name, string? description = null)
        : base(companyId)
    {
        SetName(name);
        Description = description;
        IsActive = true;
        Trips = new List<Trip>();
    }

    protected ReceiptDocumentType() : base(Guid.Empty)
    {
        Trips = new List<Trip>();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do tipo de documento e obrigatorio.");

        if (name.Trim().Length < 2)
            throw new ArgumentException("O nome do tipo de documento deve ter pelo menos 2 caracteres.");

        Name = name.Trim();
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
