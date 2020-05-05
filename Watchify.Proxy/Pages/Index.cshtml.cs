using Microsoft.AspNetCore.Mvc.RazorPages;
using Watchify.Proxy.Api;

namespace Watchify.Proxy.Pages
{
    public class IndexPageModel : PageModel
    {
	    public IndexPageModel(IProxyState state, IProxySettings settings)
	    {
		    State = state;
		    Settings = settings;
	    }

	    public IProxyState State { get; }
	    public IProxySettings Settings { get; }


	    public void OnGet()
        {

        }
    }
}