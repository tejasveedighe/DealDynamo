using DealDynamo.Models;

namespace DealDynamo.Services
{
    public interface IPaymentsRepository
    {
        IEnumerable<Payments> GetAllPayments();
        Payments GetPaymentsById(int id);
        void UpdatePayments(Payments payments);
        void DeletePayments(int id);
        void AddPayments(Payments payments);
        void SaveChanges();
    }
}
