using Omega.FleetManagement.Domain.Common;

namespace Omega.FleetManagement.Domain.Entities;

public class Expense : Entity
{
    public string Description { get; private set; }
    public decimal Value { get; private set; }
    public DateTime Date { get; private set; }
    public string? ReceiptPath { get; private set; }
    public bool IsApproved { get; private set; }

    // Chaves Estrangeiras (Foreign Keys)
    public Guid TripId { get; private set; }
    public Guid ExpenseTypeId { get; private set; }

    // Propriedades de Navegação para o Entity Framework
    public virtual Trip Trip { get; private set; }
    public virtual ExpenseType ExpenseType { get; private set; }

    // Construtor principal para criar uma nova despesa vinculada a uma viagem.
    public Expense(
        Guid companyId,
        Guid tripId,
        Guid expenseTypeId,
        string description,
        decimal value,
        DateTime date,
        bool isApproved = false,
        string? receiptPath = null)
        : base(companyId)
    {
        if (tripId == Guid.Empty) throw new ArgumentException("A despesa deve estar vinculada a uma viagem.");
        if (expenseTypeId == Guid.Empty) throw new ArgumentException("O tipo de despesa deve ser informado.");

        TripId = tripId;
        ExpenseTypeId = expenseTypeId;
        Date = date;
        ReceiptPath = receiptPath;

        SetDescription(description);
        SetValue(value);
    }

    // Construtor protegido para o Entity Framework.
    protected Expense() : base(Guid.Empty) { }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("A descrição da despesa é obrigatória.");

        if (description.Length > 255)
            throw new ArgumentException("A descrição não pode exceder 255 caracteres.");

        Description = description;
    }

    public void SetValue(decimal value)
    {
        if (value <= 0)
            throw new ArgumentException("O valor da despesa deve ser maior que zero.");

        Value = value;
    }

    public void UpdateReceipt(string? path)
    {
        ReceiptPath = path;
    }

    public void ChangeDate(DateTime newDate)
    {
        if (newDate > DateTime.UtcNow.AddDays(1))
            throw new ArgumentException("A data da despesa não pode ser no futuro.");

        Date = newDate;
    }
}