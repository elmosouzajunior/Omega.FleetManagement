using Omega.FleetManagement.Domain.Common;

namespace Omega.FleetManagement.Domain.Entities;

public class ExpenseType : Entity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    // Propriedade de navegação para o Entity Framework
    public virtual ICollection<Expense> Expenses { get; private set; }

    // Construtor principal para criação de novos tipos de despesa via Application Service.
    public ExpenseType(Guid companyId, string name, string? description = null)
        : base(companyId)
    {
        SetName(name);
        Description = description;
        IsActive = true;
        Expenses = new List<Expense>();
    }

    // Construtor protegido exigido pelo Entity Framework
    protected ExpenseType() : base(Guid.Empty)
    {
        Expenses = new List<Expense>();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do tipo de despesa é obrigatório.");

        if (name.Length < 3)
            throw new ArgumentException("O nome deve ter pelo menos 3 caracteres.");

        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
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