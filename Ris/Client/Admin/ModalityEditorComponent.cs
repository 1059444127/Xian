#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.Enterprise.Common;
using ClearCanvas.Ris.Application.Common;
using ClearCanvas.Ris.Application.Common.Admin.ModalityAdmin;

namespace ClearCanvas.Ris.Client.Admin
{
	/// <summary>
	/// Extension point for views onto <see cref="ModalityEditorComponent"/>
	/// </summary>
	[ExtensionPoint]
	public class ModalityEditorComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// ModalityEditorComponent class
	/// </summary>
	[AssociateView(typeof(ModalityEditorComponentViewExtensionPoint))]
	public class ModalityEditorComponent : ApplicationComponent
	{
		private readonly EnumValueInfo _dummyModalityNone = new EnumValueInfo("", "");
		private IList _dicomModalityChoices;
		private ModalityDetail _modalityDetail;
		private EntityRef _modalityRef;
		private readonly bool _isNew;

		private ModalitySummary _modalitySummary;

		/// <summary>
		/// Constructor
		/// </summary>
		public ModalityEditorComponent()
		{
			_isNew = true;
		}

		public ModalityEditorComponent(EntityRef modalityRef)
		{
			_isNew = false;
			_modalityRef = modalityRef;
		}

		/// <summary>
		/// Gets the summary object that is returned from the add/edit operation
		/// </summary>
		public ModalitySummary ModalitySummary
		{
			get { return _modalitySummary; }
		}

		public override void Start()
		{
			Platform.GetService(
				delegate(IModalityAdminService service)
				{
					var formDataResponse = service.LoadModalityEditorFormData(new LoadModalityEditorFormDataRequest());
					formDataResponse.DicomModalityChoices.Insert(0, _dummyModalityNone);
					_dicomModalityChoices = formDataResponse.DicomModalityChoices;

					if (_isNew)
					{
						_modalityDetail = new ModalityDetail();
					}
					else
					{
						var response = service.LoadModalityForEdit(new LoadModalityForEditRequest(_modalityRef));
						_modalityRef = response.ModalityDetail.ModalityRef;
						_modalityDetail = response.ModalityDetail;
					}
				});

			base.Start();
		}

		#region Presentation Model

		[ValidateNotNull]
		public string ID
		{
			get { return _modalityDetail.Id; }
			set
			{
				_modalityDetail.Id = value;
				this.Modified = true;
			}
		}

		[ValidateNotNull]
		public string Name
		{
			get { return _modalityDetail.Name; }
			set
			{
				_modalityDetail.Name = value;
				this.Modified = true;
			}
		}

		public EnumValueInfo DicomModality
		{
			get { return _modalityDetail.DicomModality ?? _dummyModalityNone; }
			set
			{
				if (value == null || ReferenceEquals(value, _dummyModalityNone))
					_modalityDetail.DicomModality = null;
				else
					_modalityDetail.DicomModality = value;

				this.Modified = true;
			}
		}

		public IList DicomModalityChoices
		{
			get { return _dicomModalityChoices; }
		}

		public string FormatDicomModality(object item)
		{
			var dicomModality = (EnumValueInfo)item;
			if (dicomModality == null || ReferenceEquals(dicomModality, _dummyModalityNone))
				return null;

			return string.Format("{0} - {1}", dicomModality.Value, dicomModality.Description);
		}

		public void Accept()
		{
			if (this.HasValidationErrors)
			{
				this.ShowValidation(true);
			}
			else
			{
				try
				{
					SaveChanges();
					this.Exit(ApplicationComponentExitCode.Accepted);
				}
				catch (Exception e)
				{
					ExceptionHandler.Report(e, SR.ExceptionSaveModality, this.Host.DesktopWindow,
						delegate
						{
							this.ExitCode = ApplicationComponentExitCode.Error;
							this.Host.Exit();
						});
				}
			}
		}

		public void Cancel()
		{
			this.ExitCode = ApplicationComponentExitCode.None;
			Host.Exit();
		}

		public bool AcceptEnabled
		{
			get { return this.Modified; }
		}

		#endregion

		private void SaveChanges()
		{
			Platform.GetService(
				delegate(IModalityAdminService service)
				{
					if (_isNew)
					{
						var response = service.AddModality(new AddModalityRequest(_modalityDetail));
						_modalityRef = response.Modality.ModalityRef;
						_modalitySummary = response.Modality;
					}
					else
					{
						var response = service.UpdateModality(new UpdateModalityRequest(_modalityDetail));
						_modalityRef = response.Modality.ModalityRef;
						_modalitySummary = response.Modality;
					}
				});
		}

		public event EventHandler AcceptEnabledChanged
		{
			add { this.ModifiedChanged += value; }
			remove { this.ModifiedChanged -= value; }
		}
	}
}
