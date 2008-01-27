#region License

// Copyright (c) 2006-2008, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System;

namespace ClearCanvas.ImageServer.Web.Common.WebControls
{
    /// <summary>
    /// Validate if the value in the number input control is in a specified range and that it is 
    /// less than or greater than the value of another input control.
    /// </summary>
    /// <remarks>
    /// This control has different behaviour than standard ASP.NET <seealso cref="RangeValidator"/>.
    /// Developers can optionally specify the background color for the input control if the 
    /// validation fails.  Also, a comparison can be done against another control to make sure the value
    /// of the control is less than or greater than the other control.
    /// </remarks>
    /// <example>
    /// The following block adds validation for the High Watermark. If the input is not within 1 and 99
    /// the High Watermark text box will be highlighted.
    /// 
    /// <clearcanvas:RangeComparisonValidator 
    ///                        ID="RangeValidator1" runat="server"
    ///                        ControlToValidate="HighWatermark"
    ///                        ControlToCompare="LowWatermark"
    ///                        GreaterThan="true"
    ///                        InvalidInputBackColor="#FAFFB5"
    ///                        ValidationGroup="vg1" 
    ///                        MinValue="1"
    ///                        MaxValue="99"
    ///                        ErrorMessage="The High Watermark must be between 1 and 99 and greater than the low watermark.">
    /// </clearcanvas:RangeValidator>
    /// </example>
    public class RangeComparisonValidator : BaseValidator
    {
        #region Private Members
        private int _min;
        private int _max;
        private string _comparisonControlId;
        private bool _greaterThan;
        #endregion Private Members

        #region Public Properties


        /// <summary>
        /// Sets or gets the minimum acceptable value.
        /// </summary>
        public int MinValue
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Sets or gets the maximum acceptable value.
        /// </summary>
        public int MaxValue
        {
            get { return _max; }
            set { _max = value; }
        }

        public string ControlToCompare
        {
            get { return _comparisonControlId; }
            set { _comparisonControlId = value; }
        }

        public bool GreaterThan
        {
            get { return _greaterThan; }
            set { _greaterThan = value; }
        }

        #endregion Public Properties

        #region Protected Methods
        

        /// <summary>
        /// Called during server-side validation
        /// </summary>
        /// <returns></returns>
        protected override bool OnServerSideEvaluate()
        {

            Decimal value1;
            if (Decimal.TryParse(GetControlValidationValue(ControlToValidate), out value1))
            {
                if (value1 < MinValue || value1 > MaxValue)
                {
                    return false;
                }

                Decimal value2 ;
                if (Decimal.TryParse(GetControlValidationValue(ControlToCompare), out value2))
                {
                    if (GreaterThan)
                    {
                        return value1 >= value2;
                        
                    }
                    else
                    {
                        return value1 <= value2;
                    }

                }
                else
                {
                    // can't parse value2... assume value1 is OK
                    return true;
                }
            }
            
            
            return false;
        }

       

        protected override void RegisterClientSideValidationFunction()
        {
            // Register Javascript for client-side validation
            string comparison = GreaterThan ? ">=" : "<=";
            
            ScriptTemplate template = new ScriptTemplate(GetType().Assembly, "ClearCanvas.ImageServer.Web.Common.WebControls.RangeComparisonValidator.js");
            template.Replace("@@FUNCTION_NAME@@", ClientEvalFunctionName);
            template.Replace("@@INPUT_CLIENTID@@", InputControl.ClientID);
            template.Replace("@@COMPARE_INPUT_CLIENTID@@", GetControlRenderID(ControlToCompare));
            template.Replace("@@MIN_VALUE@@", MinValue.ToString());
            template.Replace("@@MAX_VALUE@@", MaxValue.ToString());
            template.Replace("@@COMPARISON_OP@@", comparison);

            Page.ClientScript.RegisterClientScriptBlock(GetType(), ClientEvalFunctionName, template.Script, true);

        }

        #endregion Protected Methods
    }
}
