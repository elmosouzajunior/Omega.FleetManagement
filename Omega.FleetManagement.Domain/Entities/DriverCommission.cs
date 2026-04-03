namespace Omega.FleetManagement.Domain.Entities
{
    public class DriverCommission
    {
        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public decimal Rate { get; private set; }

        public virtual Driver Driver { get; private set; } = null!;

        protected DriverCommission() { }

        public DriverCommission(decimal rate)
        {
            Id = Guid.NewGuid();
            SetRate(rate);
        }

        public void AttachToDriver(Guid driverId)
        {
            DriverId = driverId;
        }

        public void SetRate(decimal rate)
        {
            if (rate < 0 || rate > 100)
                throw new ArgumentException("A comissão deve estar entre 0 e 100.");

            Rate = decimal.Round(rate, 2, MidpointRounding.AwayFromZero);
        }
    }
}
