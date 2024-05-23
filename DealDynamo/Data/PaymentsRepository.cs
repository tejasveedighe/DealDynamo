using DealDynamo.Models;
using DealDynamo.Services;
using Microsoft.EntityFrameworkCore;

namespace DealDynamo.Data
{
    public class PaymentsRepository : IPaymentsRepository
    {
        private readonly DealDynamoContext _context;
        public PaymentsRepository(DealDynamoContext context)
        {

            _context = context;

        }
        public void AddPayments(Payments payments)
        {
            _context.Payments.Add(payments);
            SaveChanges();
        }

        public void DeletePayments(int id)
        {
            _context.Payments.Remove(GetPaymentsById(id));
        }

        public IEnumerable<Payments> GetAllPayments()
        {
            return _context.Payments.Include(o => o.Order).ToList();
        }

        public Payments GetPaymentsById(int id)
        {
            return _context.Payments.Include(o => o.Order).SingleOrDefault(x => x.Id == id);
        }


        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public void UpdatePayments(Payments payments)
        {
            _context.Payments.Update(payments);
        }
    }
}
