#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using System.Collections.Generic;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.Tools.Synchronization
{
	public partial class StackingSynchronizationTool
	{
		private class FrameOfReferenceCalibrator
		{
			#region Plane class

			private class Plane
			{
				public Plane(string studyInstanceUid, string frameOfReferenceUid, Vector3D normal)
				{
					FrameOfReferenceUid = frameOfReferenceUid;
					StudyInstanceUid = studyInstanceUid;
					Normal = normal;
				}

				public readonly string FrameOfReferenceUid;
				public readonly string StudyInstanceUid;
				public readonly Vector3D Normal;

				public override bool Equals(object obj)
				{
					if (obj == this)
						return true;

					if (obj is Plane)
					{
						Plane other = (Plane)obj;
						return other.FrameOfReferenceUid == FrameOfReferenceUid &&
							other.StudyInstanceUid == StudyInstanceUid &&
							other.Normal.Equals(Normal);
					}

					return false;
				}

				public override int GetHashCode()
				{
					return 3 * FrameOfReferenceUid.GetHashCode() + 5 * StudyInstanceUid.GetHashCode() + 7 * Normal.GetHashCode();
				}
			}

			#endregion

			private readonly Dictionary<Plane, Dictionary<Plane, Vector3D>> _calibrationMatrix;
			private readonly float _angleTolerance;

			public FrameOfReferenceCalibrator(float angleTolerance)
			{
				_calibrationMatrix = new Dictionary<Plane, Dictionary<Plane, Vector3D>>();
				_angleTolerance = angleTolerance;
			}

			private static Plane FromDicomImagePlane(DicomImagePlane plane)
			{
				string frameOfReferenceUid = plane.FrameOfReferenceUid;
				string studyInstanceUid = plane.StudyInstanceUid;
				Vector3D normal = plane.Normal;

				return new Plane(studyInstanceUid, frameOfReferenceUid, normal);
			}

			private Dictionary<Plane, Vector3D> GetOffsetDictionary(Plane referencePlane)
			{
				if (_calibrationMatrix.ContainsKey(referencePlane))
					return _calibrationMatrix[referencePlane];
				else
					_calibrationMatrix[referencePlane] = new Dictionary<Plane, Vector3D>();

				return _calibrationMatrix[referencePlane];
			}

			private Vector3D ExtrapolateOffset(Plane referencePlane, Plane targetPlane, List<Plane> eliminatedPlanes)
			{
				Vector3D relativeOffset = null;

				foreach (Plane relatedPlane in _calibrationMatrix[referencePlane].Keys)
				{
					if (eliminatedPlanes.Contains(relatedPlane))
						continue;

					Vector3D offset = GetOffset(relatedPlane, targetPlane, eliminatedPlanes);
					if (offset != null)
					{
						//again, find the smallest of all possible offsets.
						offset += _calibrationMatrix[referencePlane][relatedPlane];
						if (relativeOffset == null || offset.Magnitude < relativeOffset.Magnitude)
							relativeOffset = offset;
					}
				}

				return relativeOffset;
			}

			private Vector3D GetOffset(Plane referencePlane, Plane targetPlane, List<Plane> eliminatedPlane)
			{
				if (referencePlane.Equals(targetPlane))
					return null;

				// This 'reference plane' has now been checked against 'target plane', so whether it 
				// has a direct dependency or not, it should not be considered again,
				// otherwise, we could end up in infinite recursion.
				eliminatedPlane.Add(referencePlane);

				if (_calibrationMatrix[referencePlane].ContainsKey(targetPlane))
					return _calibrationMatrix[referencePlane][targetPlane];
				else
					return ExtrapolateOffset(referencePlane, targetPlane, eliminatedPlane);
			}
			
			public Vector3D GetOffset(DicomImagePlane referenceImagePlane, DicomImagePlane targetImagePlane)
			{
				Vector3D offset = null;

				Plane referencePlane = FromDicomImagePlane(referenceImagePlane);
				if (_calibrationMatrix.ContainsKey(referencePlane))
				{
					Plane targetPlane = FromDicomImagePlane(targetImagePlane);
					if (!referencePlane.Equals(targetPlane))
						offset = GetOffset(referencePlane, targetPlane, new List<Plane>());
				}

				return offset;
			}
			

			public void Calibrate(DicomImagePlane referenceImagePlane, DicomImagePlane targetImagePlane)
			{
				if (!referenceImagePlane.IsInSameFrameOfReference(targetImagePlane) && referenceImagePlane.IsParallelTo(targetImagePlane, _angleTolerance))
				{
					Plane referencePlane = FromDicomImagePlane(referenceImagePlane);
					Plane targetPlane = FromDicomImagePlane(targetImagePlane);

					Dictionary<Plane, Vector3D> referenceOffsets = GetOffsetDictionary(referencePlane);
					Dictionary<Plane, Vector3D> targetOffsets = GetOffsetDictionary(targetPlane);

					Vector3D offset = targetImagePlane.PositionPatientCenterOfImage - referenceImagePlane.PositionPatientCenterOfImage;
					
					referenceOffsets[targetPlane] = offset;
					targetOffsets[referencePlane] = -offset;
				}
			}
		}
	}
}
