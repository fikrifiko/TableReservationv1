using Table_Reservation.Models;

namespace Table_Reservation.Services.Interfaces
{
    public interface IClientService
    {
        ClientModel GetClientById(int id);
        ClientModel UpdateClient(ClientModel client);
        void DeleteClient(int id);

        List<ClientModel> GetClients();

        ClientModel AddClient(ClientModel client);
    }
}
