#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

using ClearCanvas.Dicom;

namespace ClearCanvas.ImageViewer.Utilities.StudyFilters.Columns
{
	public class IntegerDicomTagColumn : DicomTagColumn<DicomArray<int>>, INumericSortableColumn
	{
		public IntegerDicomTagColumn(DicomTag dicomTag) : base(dicomTag) {}

		public override DicomArray<int> GetTypedValue(IStudyItem item)
		{
			DicomAttribute attribute = item[base.Tag];

			if (attribute == null)
				return null;
			if (attribute.IsNull)
				return new DicomArray<int>();

			int?[] result;
			try
			{
				result = new int?[CountValues(attribute)];
				for (int n = 0; n < result.Length; n++)
				{
					int value;
					if (attribute.TryGetInt32(n, out value))
						result[n] = value;
				}
			}
			catch (DicomException)
			{
				return null;
			}
			return new DicomArray<int>(result);
		}

		public override bool Parse(string input, out DicomArray<int> output)
		{
			return DicomArray<int>.TryParse(input, int.TryParse, out output);
		}

		public override int Compare(IStudyItem x, IStudyItem y)
		{
			return this.CompareNumerically(x, y);
		}

		public int CompareNumerically(IStudyItem x, IStudyItem y)
		{
			return DicomArray<int>.Compare(this.GetTypedValue(x), this.GetTypedValue(y));
		}
	}
}