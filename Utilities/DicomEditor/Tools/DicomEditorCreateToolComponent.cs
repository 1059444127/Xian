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
using System.Text;

using ClearCanvas.Common;
using ClearCanvas.Desktop;
using ClearCanvas.Dicom;

namespace ClearCanvas.Utilities.DicomEditor.Tools
{
    /// <summary>
    /// Extension point for views onto <see cref="DicomEditorCreateToolComponent"/>
    /// </summary>
    [ExtensionPoint]
	public sealed class DicomEditorCreateToolComponentViewExtensionPoint : ExtensionPoint<IApplicationComponentView>
    {
    }

    /// <summary>
    /// DicomEditorCreateToolComponent class
    /// </summary>
    [AssociateView(typeof(DicomEditorCreateToolComponentViewExtensionPoint))]
    public class DicomEditorCreateToolComponent : ApplicationComponent
    {
        private bool _vrEnabled;
        private bool _acceptEnabled;
        private ushort _group;
        private ushort _element;
        private string _tagName;
        private string _vr;
        private int _length;
        private string _value;

        public DicomEditorCreateToolComponent()
        {
            _vrEnabled = false;
            _acceptEnabled = false;
        }        

        public override void Start()
        {
            base.Start();
        }

        public override void Stop()
        {
            base.Stop();
        }

        public uint TagId
        {
            get { return DicomTagDictionary.GetDicomTag(_group, _element).TagValue; }
        }

        public string Group
        {
            get { return String.Format("{0:x4}", _group); }
            set 
            {
                _group = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber);
                UpdateDialog();
            }
        }

        public string Element
        {
            get { return String.Format("{0:x4}", _element); }
            set 
            {
                _element = ushort.Parse(value, System.Globalization.NumberStyles.HexNumber);
                UpdateDialog();
            }
        }

        public string TagName
        {
            get { return _tagName; }
            set { _tagName = value; }
        }

        public string Vr
        {
            get { return _vr; }
            set { _vr = value; }
        }

        public string Value
        {
            get { return _value; }
            set 
            {
                _value = value;
                if (_vr != "US" && _vr != "UL")
                {
                    _length = _value.Length % 2 == 0 ? _value.Length : _value.Length + 1;
                }
                else if (_vr == "US")
                {
                    _length = 2;
                }
                else if (_vr == "UL")
                {
                    _length = 4;
                }
            }
        }

        public bool VrEnabled
        {
            get { return _vrEnabled; }
            set { _vrEnabled = value; }
        }

        public void Accept()
        {
            //check if exists
            this.ExitCode = ApplicationComponentExitCode.Accepted;
            this.Host.Exit();            
        }

        public bool AcceptEnabled
        {
            get { return _acceptEnabled; }
            set { _acceptEnabled = value; }
        }

        public void Cancel()
        {
            this.Host.Exit();
        }

        private void UpdateDialog()
        {       
            DicomTag entry = DicomTagDictionary.GetDicomTag(_group, _element);

            if (entry != null)
            {
                this.TagName = entry.Name;
                this.Vr = entry.VR.ToString();
                this.VrEnabled = false;
            }
            else
            {
                this.TagName = "Unknown";
                this.Vr = "";
                this.VrEnabled = true;
            }
      
            this.AcceptEnabled = this.AllowTagAddition();                        
        }

        private bool AllowTagAddition()
        {
            ICollection<string> badGroups = new string[] {"0000", "0001", "0003"};

            // if the group number is odd, then it's a private tag and we 
            // cannot handle private tags yet at this point, so we fail out
            // the validation
			return !(badGroups.Contains(this.Group) || this.TagName.StartsWith("Illegal") || this.Element == "0000" || (_group % 2 > 0));
        }
    }
}
