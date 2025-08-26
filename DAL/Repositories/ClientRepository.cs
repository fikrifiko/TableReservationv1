using Table_Reservation.Models;

namespace Table_Reservation.DAL.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;


        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public ClientModel GetClientById(int clientId)
        {
            return _context.ClientModels.Where(c => c.Id == clientId).FirstOrDefault();
        }

        public ClientModel UpdateClientDetails(ClientModel clientDetails)
        {
            //var exisitngClient = _context.ClientModels.Where(c => c.Id == clientDetails.Id).FirstOrDefault();

            //if (exisitngClient == null) 
            //{
            //    throw new Exception("Client not found");
            //}

            _context.ClientModels.Update(clientDetails);
            _context.SaveChanges();

            return clientDetails;

        }
        public void DeleteClient(int clientId)
        {
            _context.ClientModels.Remove(GetClientById(clientId));
            _context.SaveChanges();
        }

        public ClientModel AddClient(ClientModel client)
        {
            _context.ClientModels.Add(client);
            _context.SaveChanges();

            return client;
        }

        public List<ClientModel> GetClients()
        {
            return _context.ClientModels.ToList();
        }
    }
}
