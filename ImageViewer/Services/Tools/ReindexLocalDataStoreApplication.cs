﻿using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;

namespace ClearCanvas.ImageViewer.Services.Tools
{
	[ExtensionOf(typeof(ApplicationRootExtensionPoint))]
	public class ReindexLocalDataStoreApplication : IApplicationRoot
	{
		private class CommandLine : ClearCanvas.Common.Utilities.CommandLine
		{
			public CommandLine(string[] args)
				: base(args)
			{
			}

			[CommandLineParameter("silent", "s", "Specifies whether the application should be silent (e.g. console app).")]
			public bool Silent { get; set; }

			[CommandLineParameter("nowait", "n", "When true, the application will initiate the reindex and quit without waiting for it to complete.")]
			public bool NoWait { get; set; }
		}

		#region IApplicationRoot Members

		public void RunApplication(string[] args)
		{
			CommandLine commandLine = new CommandLine(args);
			if (!commandLine.Silent)
				((IApplicationRoot)new ReindexLocalDataStoreDesktopApplication()).RunApplication(args);
			else
				new ReindexLocalDataStoreConsoleApplication{ NoWait = commandLine.NoWait }.Run();
		}

		#endregion
	}
}