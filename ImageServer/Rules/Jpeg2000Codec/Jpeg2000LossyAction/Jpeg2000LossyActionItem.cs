﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System;
using System.Xml;
using ClearCanvas.Common;
using ClearCanvas.Common.Specifications;
using ClearCanvas.Dicom;
using ClearCanvas.ImageServer.Model;
using ClearCanvas.ImageServer.Rules;

namespace ClearCanvas.ImageServer.Rules.Jpeg2000Codec.Jpeg2000LossyAction
{
	/// <summary>
	/// JPEG 2000 Lossy action item for <see cref="ServerRulesEngine"/>
	/// </summary>
	public class Jpeg2000LossyActionItem : ServerActionItemBase
	{
		private static readonly FilesystemQueueTypeEnum _queueType = FilesystemQueueTypeEnum.LossyCompress;
		private readonly Expression _exprScheduledTime;
		private readonly int _offsetTime;
		private readonly TimeUnit _units;
		private readonly float _ratio;

		public Jpeg2000LossyActionItem(int time, TimeUnit unit, float ratio)
			: this(time, unit, null, ratio)
		{
		}

		public Jpeg2000LossyActionItem(int time, TimeUnit unit, Expression exprScheduledTime, float ratio)
			: base("JPEG 2000 Lossy compression action")
		{
			_offsetTime = time;
			_units = unit;
			_exprScheduledTime = exprScheduledTime;
			_ratio = ratio;
		}
      

		protected override bool OnExecute(ServerActionContext context)
		{
			DateTime scheduledTime = Platform.Time;

			if (_exprScheduledTime != null)
			{
				scheduledTime = Evaluate(_exprScheduledTime, context, scheduledTime);
			}

			scheduledTime = CalculateOffsetTime(scheduledTime, _offsetTime, _units);
			XmlDocument doc = new XmlDocument();

			XmlElement element = doc.CreateElement("compress");
			doc.AppendChild(element);
			XmlAttribute syntaxAttribute = doc.CreateAttribute("syntax");
			syntaxAttribute.Value = TransferSyntax.Jpeg2000ImageCompressionUid;
			element.Attributes.Append(syntaxAttribute);

			syntaxAttribute = doc.CreateAttribute("ratio");
			syntaxAttribute.Value = _ratio.ToString();
			element.Attributes.Append(syntaxAttribute);

			Platform.Log(LogLevel.Debug, "Jpeg 2000 Lossy Compression Scheduling: This study will be compressed on {0}", scheduledTime);
			context.CommandProcessor.AddCommand(
				new InsertFilesystemQueueCommand(_queueType, context.FilesystemKey, context.StudyLocationKey,
				                                 scheduledTime, doc));

			return true;
		}
	}
}