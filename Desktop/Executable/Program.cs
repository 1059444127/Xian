#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Desktop.View.WinForms;
using System.Windows.Forms;
#if !MONO

#endif

namespace ClearCanvas.Desktop.Executable
{
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
#if !MONO
			SplashScreenManager.DisplaySplashScreen();
#endif
			Platform.PluginManager.PluginLoaded += new EventHandler<PluginLoadedEventArgs>(OnPluginProgress);

			// check for command line arguments
            if (args.Length > 0)
            {
                // for the sake of simplicity, this is a naive implementation (probably needs to change in future)
                // if there is > 0 arguments, assume the first argument is a class name
                // and bundle the subsequent arguments into a secondary array which is 
                // forwarded to the application root class
                string[] args1 = new string[args.Length - 1];
                Array.Copy(args, 1, args1, 0, args1.Length);

                Platform.StartApp(args[0], args1);
            }
            else
            {
                Platform.StartApp();
            }
		}

		private static void OnPluginProgress(object sender, PluginLoadedEventArgs e)
		{
			Platform.CheckForNullReference(e, "e");
#if !MONO
			SplashScreenManager.SetStatus(e.Message);

			if (e.PluginAssembly != null)
				SplashScreenManager.AddAssemblyIcon(e.PluginAssembly);
#endif
        }
	}
}
