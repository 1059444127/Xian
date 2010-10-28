#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using ClearCanvas.Desktop.Actions;

namespace ClearCanvas.Desktop
{
    /// <summary>
    /// Indicates the exit status of an application component.
    /// </summary>
    public enum ApplicationComponentExitCode
    {
        /// <summary>
        /// Implies that nothing of significance occured; the component was closed or cancelled.
        /// </summary>
        None,

        /// <summary>
        /// For an editable component, implies that data was changed and the user accepted the changes.
        /// </summary>
        Accepted,

        /// <summary>
        /// An error occured during the component execution.
        /// </summary>
        Error,
    }

    /// <summary>
    /// Defines the interface to an application component as seen by an application component host.
    /// </summary>
    /// <remarks>
    /// An application component must implement this interface in order to be hosted by the desktop framework.
    /// </remarks>
    public interface IApplicationComponent
    {
        /// <summary>
        /// Called by the framework to initialize the component with a host.
        /// </summary>
        void SetHost(IApplicationComponentHost host);

        /// <summary>
        /// Allows the component to export a set of actions to the host.
        /// </summary>
        /// <remarks>
        /// It is up to the host implementation to determine what, if anything,
        /// is done with the actions.
		/// </remarks>
        IActionSet ExportedActions { get; }

		/// <summary>
		/// Allows the component to specify the namespace that qualifies its global action models. This value may not be null.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value is used by the default implementation of <see cref="IDesktopWindow"/> to qualify the action model
		/// to be used for the global toolbar and menu sites.
		/// </para>
		/// </remarks>
		string GlobalActionsNamespace { get; }

        /// <summary>
        /// Called by the framework to initialize the component.
        /// </summary>
        /// <remarks>
		/// This method will be called before the component becomes visible
		/// on the screen.  All significant initialization should be performed
		/// here rather than in the constructor.
		/// </remarks>
        void Start();

        /// <summary>
        /// Called by the framework to allow the component to perform any clean-up.
        /// </summary>
        void Stop();

        /// <summary>
        /// Returns true if the component is live.
        /// </summary>
        /// <remarks>
		/// A component is considered live after the Start()
		/// method has been called, and before the Stop() method is called.
		/// </remarks>
        bool IsStarted { get; }

        /// <summary>
        /// Allows the host to determine whether this component holds modified
        /// data that may need to be saved.
        /// </summary>
        bool Modified { get; }

        /// <summary>
        /// Notifies the host that the value of the <see cref="Modified"/> property has changed.
        /// </summary>
        event EventHandler ModifiedChanged;

        /// <summary>
        /// Notifies the host that the value of any or all properties may have changed.
        /// </summary>
        event EventHandler AllPropertiesChanged;

        /// <summary>
        /// Gets a value indicating whether there are any validation errors based on the current state of the component.
        /// </summary>
        bool HasValidationErrors { get; }

        /// <summary>
        /// Shows or hides validation errors.
        /// </summary>
        void ShowValidation(bool show);

        /// <summary>
        /// Gets a value indicating whether validation errors should be visible on the user-interface.
        /// </summary>
        bool ValidationVisible { get; }

        /// <summary>
        /// Occurs when the <see cref="ValidationVisible"/> property has changed.
        /// </summary>
        event EventHandler ValidationVisibleChanged;

        /// <summary>
        /// Called by the framework to determine if this component in a state
        /// such that it can be stopped without user interaction.
        /// </summary>
        bool CanExit();

        /// <summary>
        /// Called by the framework in the case where the host has initiated the exit, rather than the component,
        /// to give the component a chance to prepare prior to being stopped.
        /// </summary>
        /// <returns>Whether or not the component is capable of exiting at this time.</returns>
        bool PrepareExit();

        /// <summary>
        /// Gets or sets the exit code for the component.
        /// </summary>
        ApplicationComponentExitCode ExitCode { get; }

		/// <summary>
		/// Occurs after the component has started.
		/// </summary>
    	event EventHandler Started;

		/// <summary>
		/// Occurs after the component has stopped.
		/// </summary>
    	event EventHandler Stopped;
    }
}
