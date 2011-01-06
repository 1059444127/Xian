#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Validation;
using ClearCanvas.ImageViewer.Services.ServerTree;

namespace ClearCanvas.ImageViewer.Configuration.ServerTree
{
	/// <summary>
	/// Extension point for views onto <see cref="DicomServerGroupEditComponent"/>
	/// </summary>
	[ExtensionPoint]
	public sealed class DicomServerGroupEditComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
	{
	}

	/// <summary>
	/// DicomServerGroupEditComponent class
	/// </summary>
	[AssociateView(typeof(DicomServerGroupEditComponentViewExtensionPoint))]
	public class DicomServerGroupEditComponent : ApplicationComponent
	{
		private class ConflictingServerGroupValidationRule : IValidationRule
		{
			public ConflictingServerGroupValidationRule()
			{
			}

			#region IValidationRule Members

			public string PropertyName
			{
				get { return "ServerGroupName"; }
			}

			public ValidationResult GetResult(IApplicationComponent component)
			{
				DicomServerGroupEditComponent groupComponent = (DicomServerGroupEditComponent)component;

				ImageViewer.Services.ServerTree.ServerTree serverTree = groupComponent._serverTree;

				bool valid = true; 
				string conflictingPath = "";
				if (groupComponent._isNewServerGroup && serverTree.CanAddGroupToCurrentGroup(groupComponent._serverGroupName, out conflictingPath))
					valid = false;
				else if (!groupComponent._isNewServerGroup && serverTree.CanEditCurrentGroup(groupComponent._serverGroupName, out conflictingPath))
					valid = false;

				if (!valid)
					return new ValidationResult(false, String.Format(SR.FormatServerGroupConflict, groupComponent._serverGroupName, conflictingPath));

				return new ValidationResult(true, "");
			}

			#endregion
		}

		#region Private Fields

		private readonly ImageViewer.Services.ServerTree.ServerTree _serverTree;
		private string _serverGroupName;
		private readonly bool _isNewServerGroup;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public DicomServerGroupEditComponent(ImageViewer.Services.ServerTree.ServerTree dicomServerTree, ServerUpdateType updatedType)
		{
			_isNewServerGroup = updatedType.Equals(ServerUpdateType.Add)? true : false;
			_serverTree = dicomServerTree;
			if (!_isNewServerGroup)
			{
				_serverGroupName = _serverTree.CurrentNode.Name;
			}
			else
			{
				_serverGroupName = "";
			}

		}

		public override void Start()
		{
			base.Start();
			base.Validation.Add(new ConflictingServerGroupValidationRule());
		}

		#region Public Properties

		[ValidateNotNull(Message = "MessageServerGroupNameCannotBeEmpty")]
		public string ServerGroupName
		{
			get { return _serverGroupName; }
			set
			{
				if (_serverGroupName == value)
					return;

				_serverGroupName = value;
				this.AcceptEnabled = true;
				NotifyPropertyChanged("ServerGroupName");
			}
		}

		public bool AcceptEnabled
		{
			get { return this.Modified; }
			set
			{
				if (value == this.Modified)
					return;

				this.Modified = value;
				NotifyPropertyChanged("AcceptEnabled");
			}
		}

		#endregion
		
		public void Accept()
		{
			ServerGroupName = ServerGroupName.Trim();

			if (base.HasValidationErrors)
			{
				base.ShowValidation(true);
			}
			else
			{
				if (!_isNewServerGroup)
				{
					ServerGroup serverGroup = (ServerGroup) _serverTree.CurrentNode;
					serverGroup.NameOfGroup = _serverGroupName;
				}
				else
				{
					ServerGroup serverGroup = new ServerGroup(_serverGroupName);
					((ServerGroup) _serverTree.CurrentNode).AddChild(serverGroup);
					_serverTree.CurrentNode = serverGroup;
				}

				_serverTree.Save();
				_serverTree.FireServerTreeUpdatedEvent();
				this.ExitCode = ApplicationComponentExitCode.Accepted;
				Host.Exit();
			}
		}

		public void Cancel()
		{
			Host.Exit();
		}
	}
}