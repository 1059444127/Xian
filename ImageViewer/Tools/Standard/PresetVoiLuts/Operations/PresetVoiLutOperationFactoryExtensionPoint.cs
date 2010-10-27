#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.ComponentModel;
using ClearCanvas.Common;
using ClearCanvas.Desktop;

namespace ClearCanvas.ImageViewer.Tools.Standard.PresetVoiLuts.Operations
{
	public enum EditContext
	{
		Add = 0,
		Edit
	}

	public interface IPresetVoiLutOperationComponent : IApplicationComponent, INotifyPropertyChanged, IDataErrorInfo
	{
		IPresetVoiLutOperation GetOperation();

		EditContext EditContext { get; set; }
		bool Valid { get; }
	}

	public interface IPresetVoiLutOperation : IUndoableOperation<IPresentationImage>
	{
		string Name { get; }
		string Description { get; }

		IPresetVoiLutOperationFactory SourceFactory { get; }

		PresetVoiLutConfiguration GetConfiguration();
	}
	
	public interface IPresetVoiLutOperationFactory
	{
		string Name { get; }
		string Description { get; }
		
		bool CanCreateMultiple { get; }

		IPresetVoiLutOperation Create(PresetVoiLutConfiguration configuration);
		IPresetVoiLutOperationComponent GetEditComponent(PresetVoiLutConfiguration configuration);
	}

	public sealed class PresetVoiLutOperationFactoryExtensionPoint : ExtensionPoint<IPresetVoiLutOperationFactory>
	{
	}
}
