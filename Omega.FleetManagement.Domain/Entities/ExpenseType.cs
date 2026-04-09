using Omega.FleetManagement.Domain.Common;
using Omega.FleetManagement.Domain.Enums;

namespace Omega.FleetManagement.Domain.Entities;

public class ExpenseType : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ExpenseCostCategory CostCategory { get; private set; }
    public bool IsActive { get; private set; }

    public virtual ICollection<Expense> Expenses { get; private set; }

    public ExpenseType(Guid companyId, string name, ExpenseCostCategory costCategory, string? description = null)
        : base(companyId)
    {
        SetName(name);
        SetCostCategory(costCategory);
        Description = description;
        IsActive = true;
        Expenses = new List<Expense>();
    }

    protected ExpenseType() : base(Guid.Empty)
    {
        Expenses = new List<Expense>();
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome do tipo de despesa e obrigatorio.");

        if (name.Length < 3)
            throw new ArgumentException("O nome deve ter pelo menos 3 caracteres.");

        Name = name;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void SetCostCategory(ExpenseCostCategory costCategory)
    {
        if (!Enum.IsDefined(costCategory))
            throw new ArgumentException("Categoria de custo invalida.");

        CostCategory = costCategory;
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
