using System;
using System.Collections.Generic;
using System.Text;

namespace ClearCanvas.ImageViewer.Explorer.Dicom
{
    public interface IDicomServer
    {
        List<IDicomServer> ChildServers
        {
            get;
        }

        string ServerName
        {
            get;
        }

        string ServerPath
        {
            get;
        }

        bool IsServer
        {
            get;
        }

        string ServerDetails
        {
            get;
        }
    }
}
