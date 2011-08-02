#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.Dicom.Utilities;
using ClearCanvas.ImageViewer.StudyManagement;

namespace ClearCanvas.ImageViewer.Explorer.Dicom
{
	/// <summary>
	/// Extension point for views onto <see cref="SearchPanelComponent"/>
	/// </summary>
	[ExtensionPoint]
	public sealed class SearchPanelComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}
	
	/// <summary>
	/// SearchPanelComponent class
	/// </summary>
	[AssociateView(typeof(SearchPanelComponentViewExtensionPoint))]
	public class SearchPanelComponent : ApplicationComponent, ISearchPanelComponent
	{
		private const string _disallowedCharacters = @"\r\n\e\f\\";
		private const string _disallowedCharactersPattern = @"[" + _disallowedCharacters + @"]+";

		private string _title;

		private string _name = "";
		private string _referringPhysiciansName = "";
		private string _patientID = "";
		private string _accessionNumber = "";
		private string _studyDescription = "";

		private DateTime? _studyDateFrom = null;
		private DateTime? _studyDateTo = null;

		private readonly Regex _openNameSearchRegex;
		private readonly Regex _lastNameFirstNameRegex;

		private readonly List<string> _searchModalities;
		private readonly ICollection<string> _availableModalities;

		private event EventHandler<SearchRequestedEventArgs> _searchRequested;

		/// <summary>
		/// Constructor
		/// </summary>
		public SearchPanelComponent()
		{
			_searchModalities = new List<string>();
			_availableModalities = StandardModalities.Modalities;

			InternalClearDates();

			char separator = DicomExplorerConfigurationSettings.Default.NameSeparator;
			string allowedExceptSeparator = String.Format("[^{0}{1}]", _disallowedCharacters, separator);

			// Last Name, First Name search
			// \A\s*[^\r\n\e\f\\,]+\s*[^\r\n\e\f\\,]*\s*,[^\r\n\e\f\\,]*\Z
			//
			// Examples of matches:
			// Doe, John
			// Doe,
			//
			// Examples of non-matches:
			// ,
			// Doe, John,

			_lastNameFirstNameRegex = new Regex(String.Format(@"\A\s*{0}+\s*{0}*\s*{1}{0}*\Z", allowedExceptSeparator, separator));

			// Open search
			// \A\s*[^\r\n\e\f\\,]+\s*\Z
			//
			// John
			// Doe
			//
			// Examples of non-matches:
			// ,
			// Doe,
			// John

			_openNameSearchRegex = new Regex(String.Format(@"\A\s*{0}+\s*\Z", allowedExceptSeparator));
		}

		public IDesktopWindow DesktopWindow
		{
			get { return this.Host.DesktopWindow; }
		}

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				NotifyPropertyChanged("Title");
			}
		}

		[ValidateRegex(_disallowedCharactersPattern, SuccessOnMatch = false, Message = "ValidationInvalidCharacters")]
		public string AccessionNumber
		{
			get { return _accessionNumber; }
			set
			{
				_accessionNumber = value ?? "";
				NotifyPropertyChanged("AccessionNumber");
			}
		}

		[ValidateRegex(_disallowedCharactersPattern, SuccessOnMatch = false, Message = "ValidationInvalidCharacters")]
		public string PatientID
		{
			get { return _patientID; }
			set
			{
				_patientID = value ?? "";
				NotifyPropertyChanged("PatientID");
			}
		}

		public string PatientsName
		{
			get { return _name; }
			set
			{
				_name = value ?? "";
				NotifyPropertyChanged("PatientsName");
			}
		}

		[ValidateRegex(_disallowedCharactersPattern, SuccessOnMatch = false, Message = "ValidationInvalidCharacters")]
		public string StudyDescription
		{
			get { return _studyDescription; }
			set
			{
				_studyDescription = value ?? "";
				NotifyPropertyChanged("StudyDescription");
			}
		}

		public string ReferringPhysiciansName
		{
			get { return _referringPhysiciansName; }
			set
			{
				_referringPhysiciansName = value ?? "";
				NotifyPropertyChanged("ReferringPhysiciansName");
			}
		}

		public DateTime? StudyDateFrom
		{
			get
			{
				return _studyDateFrom;
			}
			set
			{
				_studyDateFrom = (value == null) ? value : MinimumDate(((DateTime)value).Date, this.MaximumStudyDateFrom);
				NotifyPropertyChanged("StudyDateFrom");
			}
		}

		public DateTime? StudyDateTo
		{
			get
			{
				return _studyDateTo;
			}
			set
			{
				_studyDateTo = (value == null) ? value : MinimumDate(((DateTime)value).Date, this.MaximumStudyDateTo);
				NotifyPropertyChanged("StudyDateTo");
			}
		}

		public DateTime MaximumStudyDateFrom
		{
			get { return Platform.Time.Date; }
		}

		public DateTime MaximumStudyDateTo
		{
			get { return Platform.Time.Date; }
		}

		public ICollection<string> AvailableSearchModalities
		{
			get { return _availableModalities; }
		}

		public IList<string> SearchModalities
		{
			get { return _searchModalities; }
			set
			{
				_searchModalities.Clear();

				if (value != null)
					_searchModalities.AddRange(value);

				NotifyPropertyChanged("SearchModalities");
			}
		}

		public override void Start()
		{
			this.Title = SR.TitleSearch;

			base.Start();
		}

		public virtual void Clear()
		{
			this.PatientID = "";
			this.PatientsName = "";
			this.AccessionNumber = "";
			this.StudyDescription = "";
			this.ReferringPhysiciansName = "";
			this.SearchModalities = new List<string>(); //clear the checked modalities.

			InternalClearDates();
		}

		public virtual void Search()
		{
			if (base.HasValidationErrors)
			{
				base.ShowValidation(true);
				return;
			}
			else
			{
				base.ShowValidation(false);
			}

			var queryParams = PrepareBaseQueryParameters();
			var eventArgs = new SearchRequestedEventArgs(new List<QueryParameters> { queryParams });
			OnSearchRequested(eventArgs);
		}

		public void SearchToday()
        {
			InternalSearchLastXDays(0);
        }

		public void SearchLastWeek()
		{
			InternalSearchLastXDays(7);
		}

		public void SearchLastXDays(int numberOfDays)
		{
			InternalSearchLastXDays(numberOfDays);
		}

		public event EventHandler<SearchRequestedEventArgs> SearchRequested
		{
			add { _searchRequested += value; }
			remove { _searchRequested -= value; }
		}

		private void InternalSearchLastXDays(int numberOfDays)
		{
			this.StudyDateTo = Platform.Time.Date;
			this.StudyDateFrom = Platform.Time.Date - TimeSpan.FromDays((double)numberOfDays);

			Search();
		}

		private void InternalClearDates()
		{
			this.StudyDateTo = Platform.Time.Date;
			this.StudyDateFrom = Platform.Time.Date;
			this.StudyDateTo = null;
			this.StudyDateFrom = null;
		}

		private DateTime MinimumDate(params DateTime[] dates)
		{
			DateTime minimumDate = dates[0];
			for (int i = 1; i < dates.Length; ++i)
			{
				if (dates[i] < minimumDate)
					minimumDate = dates[i];
			}

			return minimumDate;
		}

		[ValidationMethodFor("StudyDateFrom")]
		private ValidationResult ValidateStudyDateFrom()
		{
			return ValidateDateRange();
		}

		[ValidationMethodFor("StudyDateTo")]
		private ValidationResult ValidateStudyDateTo()
		{
			return ValidateDateRange();
		}

		[ValidationMethodFor("PatientsName")]
		private ValidationResult ValidatePatientsName()
		{
			return ValidateName(PatientsName);
		}
		
		[ValidationMethodFor("ReferringPhysiciansName")]
		private ValidationResult ValidateReferringPhysiciansName()
		{
			return ValidateName(ReferringPhysiciansName);
		}

		private ValidationResult ValidateName(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				return new ValidationResult(true, "");
			}
			else if (_openNameSearchRegex.IsMatch(name) || _lastNameFirstNameRegex.IsMatch(name))
			{
				return new ValidationResult(true, "");
			}
			else if (name.Contains(DicomExplorerConfigurationSettings.Default.NameSeparator.ToString()))
			{
				return new ValidationResult(false, SR.ValidationInvalidLastNameSearch);
			}
			else
			{
				return new ValidationResult(false, SR.ValidationInvalidCharacters);
			}
		}

		private ValidationResult ValidateDateRange()
		{
			if (this.StudyDateFrom.HasValue && this.StudyDateTo.HasValue)
			{
				if (this.StudyDateFrom.Value <= this.StudyDateTo.Value)
					return new ValidationResult(true, "");

				return new ValidationResult(false, SR.MessageFromDateCannotBeAfterToDate);
			}

			return new ValidationResult(true, "");
		}

		private QueryParameters PrepareBaseQueryParameters()
		{
			var queryParams = new QueryParameters();
			queryParams["PatientsName"] = QueryStringHelper.ConvertNameToSearchCriteria(this.PatientsName);
			queryParams["ReferringPhysiciansName"] = QueryStringHelper.ConvertNameToSearchCriteria(this.ReferringPhysiciansName);
			queryParams["PatientId"] = QueryStringHelper.ConvertStringToWildcardSearchCriteria(this.PatientID, false, true);
			queryParams["AccessionNumber"] = QueryStringHelper.ConvertStringToWildcardSearchCriteria(this.AccessionNumber, false, true);
			queryParams["StudyDescription"] = QueryStringHelper.ConvertStringToWildcardSearchCriteria(this.StudyDescription, false, true);
			queryParams["StudyDate"] = DateRangeHelper.GetDicomDateRangeQueryString(this.StudyDateFrom, this.StudyDateTo);

			//At the application level, ClearCanvas defines the 'ModalitiesInStudy' filter as a multi-valued
			//Key Attribute.  This goes against the Dicom standard for C-FIND SCU behaviour, so the
			//underlying IStudyFinder(s) must handle this special case, either by ignoring the filter
			//or by running multiple queries, one per modality specified (for example).
			queryParams["ModalitiesInStudy"] = DicomStringHelper.GetDicomStringArray(this.SearchModalities);

			return queryParams;
		}

		protected virtual void OnSearchRequested(SearchRequestedEventArgs e)
		{
			EventsHelper.Fire(_searchRequested, this, e);
		}
	}
}
