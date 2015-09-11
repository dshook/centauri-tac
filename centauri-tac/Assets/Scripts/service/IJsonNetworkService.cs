using ctac.signals;

namespace ctac.services
{
	public interface IJsonNetworkService
	{
		void Request(string url);
		//Instead of an EventDispatcher, we put the actual Signals into the Interface
		FulfillWebServiceRequestSignal fulfillSignal{get;}
	}
}

