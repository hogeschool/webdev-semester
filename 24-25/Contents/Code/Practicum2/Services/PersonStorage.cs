using System.Text.Json;

namespace Services
{
  public class TextFilesPersonStorage : IPersonStorage
  {
    public async Task Create(Person person)
    {
      person.Id = Guid.NewGuid();
      await System.IO.File.WriteAllTextAsync($"people/{person.Id}.json", JsonSerializer.Serialize(person));
    }

    public async Task Overwrite(Person person)
    {
      await System.IO.File.WriteAllTextAsync($"people/{person.Id}.json", JsonSerializer.Serialize(person));
    }

    public async Task<Person?> Find(Guid personId)
    {
      var path = $"people/{personId}.json";
      if (!System.IO.File.Exists(path)) return null;
      return JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(path));
    }

    public async Task<List<Person>> FindMany(Guid[] personIds)
    {
      var people = new List<Person>();
      foreach (var personId in personIds)
      {
        var path = $"people/{personId}.json";
        if (System.IO.File.Exists(path))
        {
          var person = JsonSerializer.Deserialize<Person>(await System.IO.File.ReadAllTextAsync(path));
          if (person is not null)
            people.Add(person);
        }
      }
      return people;
    }

    public async Task Delete(Guid personId)
    {
      var path = $"people/{personId}.json";
      if (System.IO.File.Exists(path))
      {
        System.IO.File.Delete(path);
      }
    }

    public async Task Create(Person[] people)
    {
      foreach (var person in people)
      {
        await Create(person);
      }
    }
  }
}