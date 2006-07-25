using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using ClearCanvas.Common;
using ClearCanvas.Desktop.Tools;

namespace ClearCanvas.Desktop
{
    public delegate void ApplicationComponentExitDelegate(IApplicationComponent component);    
    
    public abstract class ApplicationComponent : IApplicationComponent
    {
        /// <summary>
        /// Executes the specified application component in a new workspace.  The exit callback will be invoked
        /// when the workspace is closed.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="title"></param>
        /// <param name="exitCallback"></param>
        /// <returns></returns>
        public static IWorkspace LaunchAsWorkspace(
            IApplicationComponent component,
            string title,
            ApplicationComponentExitDelegate exitCallback)
        {
            IExtensionPoint viewExtensionPoint = GetViewExtensionPoint(component.GetType());
            IWorkspace workspace = new ApplicationComponentHostWorkspace(title, component, viewExtensionPoint, exitCallback);
            DesktopApplication.WorkspaceManager.Workspaces.Add(workspace);
            return workspace;
        }

        /// <summary>
        /// Executes the specified application component in a modal dialog box.  This call will block until
        /// the dialog box is closed.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static ApplicationComponentExitCode LaunchAsDialog(
            IApplicationComponent component,
            string title)
        {
            ApplicationComponentHostDialog hostDialog = new ApplicationComponentHostDialog(title, component);
            return hostDialog.RunModal();
        }

        /// <summary>
        /// Returns the view extension point associated with the specified application component type by
        /// looking for an attribute of type <see cref="ApplicationComponentViewAttribute"/>.
        /// </summary>
        /// <param name="applicationComponentType"></param>
        /// <returns></returns>
        public static IExtensionPoint GetViewExtensionPoint(Type applicationComponentType)
        {
            object[] attrs = applicationComponentType.GetCustomAttributes(typeof(ApplicationComponentViewAttribute), false);
            if (attrs.Length == 0)
                throw new Exception("View attribute not specified");    //TODO elaborate

            ApplicationComponentViewAttribute viewAttribute = (ApplicationComponentViewAttribute)attrs[0];
            return (IExtensionPoint)Activator.CreateInstance(viewAttribute.ViewExtensionPointType);
        }



        /// <summary>
        /// This class is intentionally not exposed as an extension point because
        /// it is not intended that any extensions ever be implemented - it is only
        /// used by the StubToolContext
        /// </summary>
        public class StubToolExtensionPoint : ExtensionPoint<ITool>
        {
        }

        private IApplicationComponentHost _host;
        private ToolSet _stubToolSet;
        private ApplicationComponentExitCode _exitCode;
        private event PropertyChangedEventHandler _propertyChanged;

        public ApplicationComponent()
        {
            _exitCode = ApplicationComponentExitCode.Normal;    // default exit code
        }

        protected IApplicationComponentHost Host
        {
            get { return _host; }
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            EventsHelper.Fire(_propertyChanged, this, new PropertyChangedEventArgs(propertyName));   
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }


        #region IApplicationComponent Members

        public void SetHost(IApplicationComponentHost host)
        {
            _host = host;
        }

        /// <summary>
        /// Returns an empty <see cref="IToolSet"/>.  Subclasses may override this property if they
        /// wish to expose a toolset to the framework.
        /// </summary>
        public virtual IToolSet ToolSet
        {
            get
            {
                if (_stubToolSet == null)
                {
                    _stubToolSet = new ToolSet(new StubToolExtensionPoint(), new ToolContext());
                }
                return _stubToolSet;
            }
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual bool CanExit()
        {
            return true;
        }

        public virtual ApplicationComponentExitCode ExitCode
        {
            get { return _exitCode; }
            protected set { _exitCode = value; }
        }

        #endregion
    }
}
