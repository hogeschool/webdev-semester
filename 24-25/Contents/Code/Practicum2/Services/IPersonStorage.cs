namespace Services
{
  public class Person
  {
    public Guid Id { get; set; } 
    public string Name { get; set; }
    public string Surname { get; set;} 
    public DateTime Birthday { get; set; }
    public List<Guid> Addresses { get; set; } = new List<Guid>();
  }


  public interface IPersonStorage
  {
    Task Create(Person person);
    Task Create(Person[] people);
    Task Delete(Guid personId);
    Task<Person?> Find(Guid personId);
    Task<List<Person>> FindMany(Guid[] personIds);
    Task Overwrite(Person person);
  }
}