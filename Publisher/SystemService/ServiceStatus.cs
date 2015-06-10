using System.Runtime.InteropServices;

namespace Parcsis.PSD.Publisher.SystemService
{
	[StructLayout(LayoutKind.Sequential)]
	class ServiceStatus
	{
		public int serviceType;
		public int currentState;
		public int controlsAccepted;
		public int win32ExitCode;
		public int serviceSpecificExitCode;
		public int checkPoint;
		public int waitHint;
	}
}