using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Parcsis.PSD.Publisher.SystemService
{
	[RunInstaller(true)]
	public class HostServiceInstaller : Installer
	{
		private const string SERVICE_NAME_PARAMETER_FIRST = @"/name";
		private const string SERVICE_NAME_PARAMETER_SECOND = @"-name";

		public static string ServiceName = @"PSD Publisher";

		public HostServiceInstaller()
		{
			TryGetServiceName();

            ServiceProcessInstaller processInstaller = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };

			ServiceInstaller hostServiceInstaller = new ServiceInstaller
            	{
            		StartType = ServiceStartMode.Manual,
					ServiceName = ServiceName,
					Description = ServiceName,
            	};

			Installers.Add(hostServiceInstaller);
			Installers.Add(processInstaller);
		}

		private static void TryGetServiceName()
		{
			string serviceNameParamName = Environment.GetCommandLineArgs().FirstOrDefault(x =>
					x.ToLowerInvariant().Contains(SERVICE_NAME_PARAMETER_FIRST) ||
					x.ToLowerInvariant().Contains(SERVICE_NAME_PARAMETER_SECOND));

			if (serviceNameParamName != null)
			{
				Match match = Regex.Match(serviceNameParamName,
					string.Format(CultureInfo.InvariantCulture, @"({0}|{1})=(.*)", SERVICE_NAME_PARAMETER_FIRST, SERVICE_NAME_PARAMETER_SECOND),
					RegexOptions.Compiled | RegexOptions.IgnoreCase);

				Console.WriteLine("----- {0}", match.Groups[2].Value);

				if (match.Success && !string.IsNullOrEmpty(match.Groups[2].Value))
					ServiceName = match.Groups[2].Value;
			}
		}
	}
}