#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using ClearCanvas.Common;
using ClearCanvas.Common.Actions;
using ClearCanvas.Dicom.Utilities.Rules;
using ClearCanvas.ImageServer.Model;

namespace ClearCanvas.ImageServer.Rules.AutoRouteAction
{
	[ExtensionOf(typeof(XmlActionCompilerOperatorExtensionPoint<ServerActionContext, ServerRuleTypeEnum>))]
	public class AutoRouteActionOperator : ActionOperatorCompilerBase, IXmlActionCompilerOperator<ServerActionContext, ServerRuleTypeEnum>
    {
        public AutoRouteActionOperator()
            : base("auto-route")
        {
        }

        #region IXmlActionCompilerOperator<ServerActionContext> Members

        public IActionItem<ServerActionContext> Compile(XmlElement xmlNode)
        {
            if (xmlNode.Attributes["device"] == null)
                throw new XmlActionCompilerException("Unexpected missing device attribute for auto-route action");

            string device = xmlNode.Attributes["device"].Value;

			if ((xmlNode.Attributes["startTime"] != null) 
				&& (xmlNode.Attributes["endTime"] != null))
			{
				DateTime startTime;
				if (!DateTime.TryParseExact(xmlNode.Attributes["startTime"].Value, "HH:mm:ss",
											CultureInfo.InvariantCulture, DateTimeStyles.None,
											out startTime))
				{
					throw new XmlActionCompilerException("Incorrect format of startTime: " + xmlNode.Attributes["startTime"].Value);	
				}

				DateTime endTime;
				if (!DateTime.TryParseExact(xmlNode.Attributes["endTime"].Value, "HH:mm:ss",
											CultureInfo.InvariantCulture, DateTimeStyles.None,
											out endTime))
				{
					throw new XmlActionCompilerException("Incorrect format of endTime: " + xmlNode.Attributes["endTime"].Value);
				}

				return new AutoRouteActionItem(device, startTime, endTime);
			}
			else if ((xmlNode.Attributes["startTime"] == null)
				&& (xmlNode.Attributes["endTime"] != null))
				throw new XmlActionCompilerException("Unexpected missing startTime attribute for auto-route action");
			else if ((xmlNode.Attributes["startTime"] != null)
		        && (xmlNode.Attributes["endTime"] == null))
				throw new XmlActionCompilerException("Unexpected missing endTime attribute for auto-route action");
         
            return new AutoRouteActionItem(device);
        }

		public XmlSchemaElement GetSchema(ServerRuleTypeEnum ruleType)
		{
			if (!ruleType.Equals(ServerRuleTypeEnum.AutoRoute))
				return null;

            XmlSchemaComplexType type = new XmlSchemaComplexType();

            XmlSchemaAttribute attrib = new XmlSchemaAttribute();
            attrib.Name = "device";
            attrib.Use = XmlSchemaUse.Required;
            attrib.SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
            type.Attributes.Add(attrib);

			attrib = new XmlSchemaAttribute();
			attrib.Name = "startTime";
			attrib.Use = XmlSchemaUse.Optional;
			attrib.SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
			type.Attributes.Add(attrib);

			attrib = new XmlSchemaAttribute();
			attrib.Name = "endTime";
			attrib.Use = XmlSchemaUse.Optional;
			attrib.SchemaTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
			type.Attributes.Add(attrib);

            XmlSchemaElement element = new XmlSchemaElement();
            element.Name = "auto-route";
            element.SchemaType = type;

            return element;
        }

        #endregion
    }
}