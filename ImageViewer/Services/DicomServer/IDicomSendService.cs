#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.ServiceModel;

namespace ClearCanvas.ImageViewer.Services.DicomServer
{
	//TODO: add IDicomRetrieveService
	//TODO: move to Dicom service model?
	[ServiceContract(ConfigurationName = "IDicomSendService")]
	public interface IDicomSendService
	{
		[OperationContract]
		SendOperationReference SendStudies(SendStudiesRequest request);
		
		[OperationContract]
		SendOperationReference SendSeries(SendSeriesRequest request);
		
		[OperationContract]
		SendOperationReference SendSopInstances(SendSopInstancesRequest request);

		[OperationContract]
		SendOperationReference SendFiles(SendFilesRequest request);

		//TODO: add cancel
	}
}
