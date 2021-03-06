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
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.ImageViewer.Utilities.StudyFilters.Utilities;

namespace ClearCanvas.ImageViewer.Utilities.StudyFilters.AutoFilters
{
	[ExtensionPoint]
	public sealed class AutoFilterToolExtensionPoint : ExtensionPoint<ITool> {}

	public interface IAutoFilterToolContext : IToolContext
	{
		StudyFilterColumn Column { get; }
	}

	public abstract class AutoFilterTool : Tool<IAutoFilterToolContext>
	{
		public event EventHandler VisibleChanged;

		private bool _visible = true;

		public bool Visible
		{
			get { return _visible; }
			protected set
			{
				if (_visible != value)
				{
					_visible = value;
					EventsHelper.Fire(this.VisibleChanged, this, EventArgs.Empty);
				}
			}
		}

		public StudyFilterColumn Column
		{
			get { return base.Context.Column; }
		}

		public CompositeFilterPredicate AutoFilterRoot
		{
			get { return base.Context.Column.AutoFilterRoot; }
		}

		public IStudyFilter StudyFilter
		{
			get { return base.Context.Column.Owner; }
		}

		protected virtual bool IsColumnSupported()
		{
			return true;
		}

		public override void Initialize()
		{
			base.Initialize();
			this.Visible = this.IsColumnSupported();
		}
	}
}