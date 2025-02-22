
using BreweryFinder.API.Models;

namespace BreweryFinder.API.Services
{
    public interface IBreweryService
    {
        Task<List<Brewery>> GetBreweriesByCityAsync(string city);

        Task<List<Brewery>> GetBreweriesByStateAsync(string state);
         
        Task<List<Brewery>> GetBreweriesByTypeAsync(string type);
    }
}