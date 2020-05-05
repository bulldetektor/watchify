using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Watchify.Proxy.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProxyController : ControllerBase
    {
	    private readonly IProxyState _state;
	    private readonly IProxySettings _settings;

	    public ProxyController(IProxyState state, IProxySettings settings)
	    {
		    _state = state;
		    _settings = settings;
	    }

	    [HttpPut("init")]
	    public void Initialize([FromBody] InitializationInput input)
	    {
		    _settings.Initialize(input.ProxyTo);
			_state.UpdateStatus("Initialized");
	    }

	    [HttpPut("status")]
	    public void UpdateStatus([FromBody] StatusInput input)
	    {
		    _state.UpdateStatus(input.Status);
	    }

	    [HttpGet]
	    public IActionResult GetStatus()
	    {
		    return Ok(_state.Status);
	    }

	    public class StatusInput
	    {
		    public string Status { get; set; }
	    }
    }

    public class InitializationInput
    {
		/// <summary>
		/// The endpoint to proxy requests to, e.g. "http://localhost:5000"
		/// </summary>
	    public string ProxyTo { get; set; }
    }

    public interface IProxySettings
    {
	    void Initialize(string proxyTo);

		string ProxyTo { get; }
    }

    public interface IProxyState
    {
	    string Status { get; }

	    void UpdateStatus(string newStatus);
    }

    public class ProxyOptions : IProxyState, IProxySettings
    {
	    public ProxyOptions(string initalStatus)
	    {
		    Status = initalStatus;
	    }

	    public string Status { get; private set; }
	    public string ProxyTo { get; private set; }

	    public void UpdateStatus(string newStatus)
	    {
		    Status = newStatus;
	    }

	    public void Initialize(string proxyTo)
	    {
		    ProxyTo = proxyTo;
	    }
    }
}