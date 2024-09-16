using System.Text.Json;

namespace Services
{
  public class TextFilesAddressStorage : IAddressStorage
  {
    private readonly IPersonStorage PersonStorage;
    public TextFilesAddressStorage(IPersonStorage personStorage)
    {
      PersonStorage = personStorage;
    }
    public async Task AddAddress(Address address)
    {
      address.Id = Guid.NewGuid();
      var person = await PersonStorage.Find(address.PersonId);
      if (person != null)
      {
        person.Addresses.Add(address.Id);
        await System.IO.File.WriteAllTextAsync($"people/{person.Id}.json", JsonSerializer.Serialize(person));
        await System.IO.File.WriteAllTextAsync($"addresses/{address.Id}.json", JsonSerializer.Serialize(address));
      }
    }

    public async Task AddAddresses(Address[] addresses)
    {
      foreach (var address in addresses)
      {
        await AddAddress(address);
      }
    }

    public async Task DeleteAddress(Guid addressId)
    {
      var path = $"addresses/{addressId}.json";
      if (System.IO.File.Exists(path))
      {
        var address = JsonSerializer.Deserialize<Address>(await System.IO.File.ReadAllTextAsync(path));
        if (address != null)
        {
          var personPath = $"people/{address.PersonId}.json";
          if (System.IO.File.Exists(personPath))
          {
            var person = JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(personPath));
            if (person != null)
            {
              person.Addresses.Remove(address.Id);
              await PersonStorage.Overwrite(person);
            }
          }
          System.IO.File.Delete(path);
        }
      }
    }

    public async Task<List<Address>> FindAddresses(Guid personId)
    {
      var person = await PersonStorage.Find(personId);
      var addresses = new List<Address>();
      if (person != null)
      {
        foreach (var addressId in person.Addresses)
        {
          var path = $"addresses/{addressId}.json";
          if (System.IO.File.Exists(path))
          {
            var address = JsonSerializer.Deserialize<Address>(await System.IO.File.ReadAllTextAsync(path));
            if (address is not null)
              addresses.Add(address);
          }
        }
      }
      return addresses;
    }
  }
}