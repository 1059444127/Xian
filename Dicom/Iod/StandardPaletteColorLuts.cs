﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Reflection;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Dicom.Utilities.Xml;

namespace ClearCanvas.Dicom.Iod
{
	partial class PaletteColorLut
	{
		private static PaletteColorLut _hotIron;
		private static PaletteColorLut _hotMetalBlue;
		private static PaletteColorLut _pet20Step;
		private static PaletteColorLut _pet;

		/// <summary>
		/// Gets the Hot Iron standard color palette.
		/// </summary>
		/// <remarks>As defined in the DICOM Standard 2009, Part 6, Section B.1.1</remarks>
		public static PaletteColorLut HotIron
		{
			get
			{
				if (_hotIron == null)
					_hotIron = CreateFromColorPaletteSopInstanceXml("Iod.Resources.HotIronStandardColorPalette.xml");
				return _hotIron;
			}
		}

		/// <summary>
		/// Gets the Hot Metal Blue standard color palette.
		/// </summary>
		/// <remarks>As defined in the DICOM Standard 2009, Part 6, Section B.1.3</remarks>
		public static PaletteColorLut HotMetalBlue
		{
			get
			{
				if (_hotMetalBlue == null)
					_hotMetalBlue = CreateFromColorPaletteSopInstanceXml("Iod.Resources.HotMetalBlueStandardColorPalette.xml");
				return _hotMetalBlue;
			}
		}

		/// <summary>
		/// Gets the PET 20 Step standard color palette.
		/// </summary>
		/// <remarks>As defined in the DICOM Standard 2009, Part 6, Section B.1.4</remarks>
		public static PaletteColorLut PET20Step
		{
			get
			{
				if (_pet20Step == null)
					_pet20Step = CreateFromColorPaletteSopInstanceXml("Iod.Resources.PET20StepStandardColorPalette.xml");
				return _pet20Step;
			}
		}

		/// <summary>
		/// Gets the PET standard color palette.
		/// </summary>
		/// <remarks>As defined in the DICOM Standard 2009, Part 6, Section B.1.2</remarks>
		public static PaletteColorLut PET
		{
			get
			{
				if (_pet == null)
					_pet = CreateFromColorPaletteSopInstanceXml("Iod.Resources.PETStandardColorPalette.xml");
				return _pet;
			}
		}

		private static PaletteColorLut CreateFromColorPaletteSopInstanceXml(string resourceName)
		{
			try
			{
				var resourceResolver = new ResourceResolver(Assembly.GetExecutingAssembly());
				using (var xmlStream = resourceResolver.OpenResource(resourceName))
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(xmlStream);
					var docRootNode = CollectionUtils.FirstElement(xmlDocument.GetElementsByTagName("ClearCanvasColorPaletteDefinition")) as XmlElement;
					if (docRootNode != null)
					{
						var instanceNode = CollectionUtils.FirstElement(docRootNode.GetElementsByTagName("Instance")) as XmlElement;
						if (instanceNode != null)
						{
							var instanceXml = new InstanceXml(instanceNode, null);
							return Create(instanceXml.Collection);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Platform.Log(LogLevel.Debug, ex, "Failed to load embedded standard color palette SOP from resource {0}", resourceName);
			}
			return null;
		}
	}
}