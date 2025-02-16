
using BreweryFinder.API.Models;

namespace BreweryFinder.API.Services
{
    public interface IBreweryService
    {
        Task<List<Brewery>> GetBreweriesByCity(string city);
    }
}