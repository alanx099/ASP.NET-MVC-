using System.Collections.Generic;
using Servis_Centar_Za_Gitare.Data.Interfaces;
using Servis_Centar_Za_Gitare.Data.Mock;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        public IEnumerable<Stranka> GetAll()
        {
            return GuitarServiceMockData.Customers;
        }

        public Stranka? GetById(int id)
        {
            return GuitarServiceMockData.FindCustomer(id);
        }
    }
}