using Microsoft.AspNetCore.Mvc;
using Services;

namespace Controllers
{
  [Route("api/address")]
  public class AddressController : Controller
  {
    private readonly IAddressStorage AddressStorage;
    public AddressController(IAddressStorage addressStorage)
    {
      AddressStorage = addressStorage;
    }

    [HttpPost()]
    public async Task<IActionResult> AddAddress([FromBody] Address address)
    {
      await AddressStorage.AddAddress(address);
      return Ok();
    }

    [HttpPost("batch-add")]
    public async Task<IActionResult> AddAddresses([FromBody] Address[] addresses)
    {
      await AddressStorage.AddAddresses(addresses);
      return Ok();
    }

    [HttpGet()]
    public async Task<IActionResult> GetAddresses([FromQuery] Guid personId)
    {
      var addresses = await AddressStorage.FindAddresses(personId);
      return Ok(addresses);
    }

    [HttpDelete()]
    public async Task<IActionResult> DeleteAddress([FromQuery] Guid addressId)
    {
      await AddressStorage.DeleteAddress(addressId);
      return Ok();
    }
  }
}