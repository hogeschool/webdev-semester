namespace Services
{
  public class Address
  {
    public Guid Id { get; set; }
    public string StreetName { get; set; }
    public int HouseNumber { get; set; }
    public char? ApartmentNumber { get; set; }
    public Guid PersonId { get; set; }
  }
  public interface IAddressStorage
  {
    Task AddAddress(Address address);
    Task AddAddresses(Address[] addresses);
    Task<List<Address>> FindAddresses(Guid personId);
    Task DeleteAddress(Guid addressId);
  }
}