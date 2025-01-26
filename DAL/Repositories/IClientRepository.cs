using Table_Reservation.Models;

namespace Table_Reservation.DAL.Repositories
{
    public interface IClientRepository
    {
        public ClientModel GetClientById(int clientId);
        public ClientModel UpdateClientDetails(ClientModel clientDetails);
        public void DeleteClient(int clientId);

        public ClientModel AddClient(ClientModel client);

        public List<ClientModel> GetClients();
    }
}
