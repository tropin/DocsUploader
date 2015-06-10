using System;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Parcsis.PSD.Publisher
{
	public class ServiceHost<T> : ServiceHost
	{
		class ErrorHandlerBehavior : IServiceBehavior,IErrorHandler
		{
			private readonly IErrorHandler _errorHandler;

			public ErrorHandlerBehavior(IErrorHandler errorHandler)
			{
				_errorHandler = errorHandler;
			}
			void IServiceBehavior.Validate(ServiceDescription description,ServiceHostBase host)
			{}
			void IServiceBehavior.AddBindingParameters(ServiceDescription description,ServiceHostBase host,Collection<ServiceEndpoint> endpoints,BindingParameterCollection parameters)
			{}
			void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description,ServiceHostBase host)
			{
				foreach(ChannelDispatcher dispatcher in host.ChannelDispatchers)
				{
					dispatcher.ErrorHandlers.Add(this);
				}
			}
			bool IErrorHandler.HandleError(Exception error)
			{
				return _errorHandler.HandleError(error);
			}
			void IErrorHandler.ProvideFault(Exception error,MessageVersion version, ref Message fault)
			{
				_errorHandler.ProvideFault(error,version,ref fault);
			}
		}

		private readonly List<IServiceBehavior> _errorHandlers = new List<IServiceBehavior>();

		/// <summary>
		/// Can only call before openning the host
		/// </summary>
		public void AddErrorHandler(IErrorHandler errorHandler)
		{
			if(State == CommunicationState.Opened)
			{
				throw new InvalidOperationException(@"Host is already opened");
			}
			Debug.Assert(errorHandler != null);
			IServiceBehavior errorHandlerBehavior = new ErrorHandlerBehavior(errorHandler);

			_errorHandlers.Add(errorHandlerBehavior);
		}

		/// <summary>
		/// Can only call before openning the host
		/// </summary>
		public void EnableMetadataExchange()
		{
			EnableMetadataExchange(true);
		}
		/// <summary>
		/// Can only call before openning the host
		/// </summary>
		public void EnableMetadataExchange(bool enableHttpGet)
		{
			if(State == CommunicationState.Opened)
			{
				throw new InvalidOperationException(@"Host is already opened");
			}

			ServiceMetadataBehavior metadataBehavior = Description.Behaviors.Find<ServiceMetadataBehavior>();

			if(metadataBehavior == null)
			{
				metadataBehavior = new ServiceMetadataBehavior();
				Description.Behaviors.Add(metadataBehavior);
                                                            
				if(BaseAddresses.Any(uri=>uri.Scheme == "http"))
				{
					metadataBehavior.HttpGetEnabled = enableHttpGet;
				}
                                              
				if(BaseAddresses.Any(uri=>uri.Scheme == "https"))
				{
					metadataBehavior.HttpsGetEnabled = enableHttpGet;
				}
			}
			AddAllMexEndPoints();
		}
		public void AddAllMexEndPoints()
		{
			Debug.Assert(HasMexEndpoint == false);

			foreach(Uri baseAddress in BaseAddresses)
			{
				BindingElement bindingElement = null;
				switch(baseAddress.Scheme)
				{
					case "net.tcp":
						{
							bindingElement = new TcpTransportBindingElement();
							break;
						}
					case "net.pipe":
						{
							bindingElement = new NamedPipeTransportBindingElement();
							break;
						}
					case "http":
						{
							bindingElement = new HttpTransportBindingElement();
							break;
						}
					case "https":
						{
							bindingElement = new HttpsTransportBindingElement();
							break;
						}
				}
				if(bindingElement != null)
				{
					Binding binding = new CustomBinding(bindingElement);
					AddServiceEndpoint(typeof(IMetadataExchange), binding, @"MEX");
				}         
			}
		}
      
		public bool HasMexEndpoint
		{
			get
			{
				return Description.Endpoints.Any(endpoint => endpoint.Contract.ContractType == typeof(IMetadataExchange));
			}
		}
      
		protected override void OnOpening()
		{
			foreach(IServiceBehavior behavior in _errorHandlers)
			{
				Description.Behaviors.Add(behavior);
			}

			base.OnOpening();
		}
        
		/// <summary>
		/// Can only call after openning the host
		/// </summary>
		public ServiceThrottle Throttle
		{
			get
			{
				if(State != CommunicationState.Opened)
				{
					throw new InvalidOperationException(@"Host is not opened");
				}

				ChannelDispatcher dispatcher = OperationContext.Current.Host.ChannelDispatchers[0] as ChannelDispatcher;

				if (dispatcher != null)
					return dispatcher.ServiceThrottle;

				throw new InvalidOperationException(@"OperationContext.Current.Host.ChannelDispatchers count is 0!");
			}
		} 
		/// <summary>
		/// Can only call before openning the host
		/// </summary>
		public bool IncludeExceptionDetailInFaults
		{
			set
			{
				if(State == CommunicationState.Opened)
				{
					throw new InvalidOperationException(@"Host is already opened");
				}
				ServiceBehaviorAttribute debuggingBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
				debuggingBehavior.IncludeExceptionDetailInFaults = value;
			}
			get
			{
				ServiceBehaviorAttribute debuggingBehavior = Description.Behaviors.Find<ServiceBehaviorAttribute>();
				return debuggingBehavior.IncludeExceptionDetailInFaults;
			}
		}

		/// <summary>
		/// Can only call before openning the host
		/// </summary>
		public bool SecurityAuditEnabled
		{
			get
			{
				ServiceSecurityAuditBehavior securityAudit = Description.Behaviors.Find<ServiceSecurityAuditBehavior>();
				if(securityAudit != null)
				{
					return securityAudit.MessageAuthenticationAuditLevel == AuditLevel.SuccessOrFailure &&
					       securityAudit.ServiceAuthorizationAuditLevel == AuditLevel.SuccessOrFailure;
				}

				return false;
			}
			set
			{
				if(State == CommunicationState.Opened)
				{
					throw new InvalidOperationException(@"Host is already opened");
				}
				ServiceSecurityAuditBehavior securityAudit = Description.Behaviors.Find<ServiceSecurityAuditBehavior>();
				if(securityAudit == null && value)
				{
					securityAudit = new ServiceSecurityAuditBehavior
	                	{
	                		MessageAuthenticationAuditLevel = AuditLevel.SuccessOrFailure,
	                		ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure
	                	};
					Description.Behaviors.Add(securityAudit);
				}
			}
		}
		public ServiceHost() : base(typeof(T))
		{}
		public ServiceHost(params string[] baseAddresses) : base(typeof(T),Convert(baseAddresses))
		{}
		public ServiceHost(params Uri[] baseAddresses) : base(typeof(T),baseAddresses)
		{}
		public ServiceHost(T singleton,params string[] baseAddresses) : base(singleton,Convert(baseAddresses))
		{}
		public ServiceHost(T singleton) : base(singleton)
		{}
		public ServiceHost(T singleton,params Uri[] baseAddresses) : base(singleton,baseAddresses)
		{}
		public virtual T Singleton
		{
			get
			{
				if(SingletonInstance == null)
				{
					return default(T);
				}
				Debug.Assert(SingletonInstance is T);
				return (T)SingletonInstance;
			}
		}
		static Uri[] Convert(string[] baseAddresses)
		{
			Converter<string,Uri> convert = address=> new Uri(address);
			return Array.ConvertAll(baseAddresses, convert); 
		}
	}
}