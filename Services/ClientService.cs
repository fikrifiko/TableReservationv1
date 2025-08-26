using Table_Reservation.DAL.Repositories;
using Table_Reservation.Models;
using Table_Reservation.Services.Interfaces;

namespace Table_Reservation.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }
        public ClientModel GetClientById(int id)
        {
            return _clientRepository.GetClientById(id);
        }

        public ClientModel UpdateClient(ClientModel client)
        {
            return _clientRepository.UpdateClientDetails(client);
        }
        public void DeleteClient(int id)
        {
            _clientRepository.DeleteClient(id);
        }

        public List<ClientModel> GetClients()
        {
            return _clientRepository.GetClients();
        }

        public ClientModel AddClient(ClientModel client)
        {
            return _clientRepository.AddClient(client);
        }
    }
}
