#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.Imaging;

namespace ClearCanvas.ImageViewer.AdvancedImaging.Fusion
{
	[Cloneable]
	internal class VoiLutManagerProxy : IVoiLutManager
	{
		[CloneIgnore]
		private readonly IVoiLutManager _placeholderVoiLutManager;

		[CloneIgnore]
		private IVoiLutManager _realVoiLutManager;

		public VoiLutManagerProxy()
		{
			_placeholderVoiLutManager = new VoiLutManager(new XVoiLutInstaller(), false);
		}

		/// <summary>
		/// Cloning constructor.
		/// </summary>
		/// <param name="source">The source object from which to clone.</param>
		/// <param name="context">The cloning context object.</param>
		protected VoiLutManagerProxy(VoiLutManagerProxy source, ICloningContext context)
		{
			context.CloneFields(source, this);

			_placeholderVoiLutManager = new VoiLutManager(new XVoiLutInstaller(source._realVoiLutManager ?? source._placeholderVoiLutManager), false);
		}

		public void SetRealVoiLutManager(IVoiLutManager realVoiLutManager)
		{
			if (_realVoiLutManager != null)
			{
				Replicate(_realVoiLutManager, _placeholderVoiLutManager);
			}

			_realVoiLutManager = realVoiLutManager;

			if (_realVoiLutManager != null)
			{
				Replicate(_placeholderVoiLutManager, _realVoiLutManager);
			}
		}

		private static void Replicate<T>(T source, T destination) where T : IMemorable
		{
			destination.SetMemento(source.CreateMemento());
		}

		#region IVoiLutManager Members

		[Obsolete("Use the VoiLut property instead.")]
		IComposableLut IVoiLutManager.GetLut()
		{
			if (_realVoiLutManager != null)
				return _realVoiLutManager.GetLut();
			else
				return _placeholderVoiLutManager.GetLut();
		}

		[Obsolete("Use the InstallVoiLut method instead.")]
		void IVoiLutManager.InstallLut(IComposableLut voiLut)
		{
			if (_realVoiLutManager != null)
				_realVoiLutManager.InstallLut(voiLut);
			else
				_placeholderVoiLutManager.InstallLut(voiLut);
		}

		[Obsolete("Use the Invert property instead.")]
		void IVoiLutManager.ToggleInvert() {}

		bool IVoiLutManager.Enabled
		{
			get
			{
				if (_realVoiLutManager != null)
					return _realVoiLutManager.Enabled;
				else
					return _placeholderVoiLutManager.Enabled;
			}
			set
			{
				if (_realVoiLutManager != null)
					_realVoiLutManager.Enabled = value;
				else
					_placeholderVoiLutManager.Enabled = value;
			}
		}

		#endregion

		#region IVoiLutInstaller Members

		IComposableLut IVoiLutInstaller.VoiLut
		{
			get
			{
				if (_realVoiLutManager != null)
					return _realVoiLutManager.VoiLut;
				else
					return _placeholderVoiLutManager.VoiLut;
			}
		}

		void IVoiLutInstaller.InstallVoiLut(IComposableLut voiLut)
		{
			if (_realVoiLutManager != null)
				_realVoiLutManager.InstallVoiLut(voiLut);
			else
				_placeholderVoiLutManager.InstallVoiLut(voiLut);
		}

		bool IVoiLutInstaller.Invert
		{
			get { return false; }
			set { }
		}

		#endregion

		#region IMemorable Members

		object IMemorable.CreateMemento()
		{
			if (_realVoiLutManager != null)
				return _realVoiLutManager.CreateMemento();
			else
				return _placeholderVoiLutManager.CreateMemento();
		}

		void IMemorable.SetMemento(object memento)
		{
			if (_realVoiLutManager != null)
			{
				_realVoiLutManager.SetMemento(memento);
				_realVoiLutManager.Invert = false;
			}
			else
			{
				_placeholderVoiLutManager.SetMemento(memento);
				_placeholderVoiLutManager.Invert = false;
			}
		}

		#endregion

		#region XVoiLutInstaller Class

		private class XVoiLutInstaller : IVoiLutInstaller
		{
			public bool Invert { get; set; }
			public IComposableLut VoiLut { get; set; }

			public XVoiLutInstaller()
			{
				this.Invert = false;
				this.VoiLut = new BasicVoiLutLinear(ushort.MaxValue + 1, 0);
			}

			public XVoiLutInstaller(IVoiLutInstaller source)
			{
				this.Invert = source.Invert;
				this.VoiLut = source.VoiLut.Clone();
			}

			public void InstallVoiLut(IComposableLut voiLut)
			{
				this.VoiLut = voiLut;
			}
		}

		#endregion
	}
}