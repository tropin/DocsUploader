using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Parcsis.PSD.Publisher
{
	public class WcfNamedPipeServiceClient<T> : IDisposable
	{
		private readonly T _channel;
		private readonly IClientChannel _clientChannel;

		public WcfNamedPipeServiceClient(string url)
		{
			NetNamedPipeBinding binding = new NetNamedPipeBinding();			
			EndpointAddress adress = new EndpointAddress(url);
			ChannelFactory<T> factory = new ChannelFactory<T>(binding, adress);
			_clientChannel = (IClientChannel)factory.CreateChannel();
			_channel = (T)_clientChannel;
		}

		public T Channel
		{
			get { return this._channel; }
		}

		public IClientChannel ClientChannel
		{
			get { return this._clientChannel; }
		}

		public void Dispose()
		{
			_clientChannel.Dispose();
		}
	}
}
