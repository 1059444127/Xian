using System;

namespace ClearCanvas.Common
{
	/// <summary>
	/// Summary description for PluginProgressEventArgs.
	/// </summary>
	public class PluginLoadedEventArgs : EventArgs
	{
		string _message;

		public PluginLoadedEventArgs(string message)
		{
			_message = message;
		}

		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				_message = value;
			}
		}
	}
}
