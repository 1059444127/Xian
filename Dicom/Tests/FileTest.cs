#region License

// Copyright (c) 2011, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This software is licensed under the Open Software License v3.0.
// For the complete license, see http://www.clearcanvas.ca/OSLv3.0

#endregion

#if UNIT_TESTS

using System;
using NUnit.Framework;

namespace ClearCanvas.Dicom.Tests
{
    [TestFixture]
    public class FileTest : AbstractTest
    {
        [Test]
        public void ConstructorTests()
        {
            DicomFile file = new DicomFile(null);

            file = new DicomFile("filename");

            file = new DicomFile(null, new DicomAttributeCollection(), DicomAttributeCollection.ToProvider(new DicomAttributeCollection()));


        }

        public void WriteOptionsTest(DicomFile sourceFile, DicomWriteOptions options)
        {
            bool result = sourceFile.Save(options);

            Assert.AreEqual(result, true);

            DicomFile newFile = new DicomFile(sourceFile.Filename);

            DicomReadOptions readOptions = DicomReadOptions.Default;
            newFile.Load(readOptions);

            Assert.AreEqual(sourceFile.DataSet.Equals(newFile.DataSet), true);

            // update a tag, and make sure it shows they're different
            newFile.DataSet[DicomTags.PatientsName].Values = "NewPatient^First";
            Assert.AreEqual(sourceFile.DataSet.Equals(newFile.DataSet), false);

            // Now update the original file with the name, and they should be the same again
            sourceFile.DataSet[DicomTags.PatientsName].Values = "NewPatient^First";
            Assert.AreEqual(sourceFile.DataSet.Equals(newFile.DataSet), true);

            // Return the original string value.
            sourceFile.DataSet[DicomTags.PatientsName].SetStringValue("Patient^Test");

        }

        [Test]
        public void MultiframeCreateFileTest()
        {
            DicomFile file = new DicomFile("MultiframeCreateFileTest.dcm");

            DicomAttributeCollection dataSet = file.DataSet;

            SetupMultiframeXA(dataSet, 511, 511, 5);

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            DicomWriteOptions writeOptions = DicomWriteOptions.Default;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence | DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.None;
            WriteOptionsTest(file, writeOptions);

            // Big Endian Tests
            file.Filename = "MultiframeBigEndianCreateFileTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            writeOptions = DicomWriteOptions.Default;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence | DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.None;
            WriteOptionsTest(file, writeOptions);

        }

        [Test]
        public void CreateFileTest()
        {
            DicomFile file = new DicomFile("CreateFileTest.dcm");

            DicomAttributeCollection dataSet = file.DataSet;

            SetupMR(dataSet);


            string studyId = file.DataSet[DicomTags.StudyId].GetString(0, "");
            Assert.AreEqual(studyId, "1933");

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian; 

            DicomWriteOptions writeOptions = DicomWriteOptions.Default;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence | DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.None;
            WriteOptionsTest(file, writeOptions);

            // Big Endian Tests
            file.Filename = "BigEndianCreateFileTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            writeOptions = DicomWriteOptions.Default;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.ExplicitLengthSequence | DicomWriteOptions.ExplicitLengthSequenceItem;
            WriteOptionsTest(file, writeOptions);

            writeOptions = DicomWriteOptions.None;
            WriteOptionsTest(file, writeOptions);

        }

        public void ReadOptionsTest(DicomFile sourceFile, DicomReadOptions options, bool areEqual)
        {
            bool result = sourceFile.Save(DicomWriteOptions.Default);

            Assert.AreEqual(result, true);

            DicomFile newFile = new DicomFile(sourceFile.Filename);

            newFile.Load(options);

            if (areEqual)
                Assert.AreEqual(sourceFile.DataSet.Equals(newFile.DataSet), true);
            else
                Assert.AreNotEqual(sourceFile.DataSet.Equals(newFile.DataSet), true);
        }

        [Test]
        public void FileReadTest()
        {
            DicomFile file = new DicomFile("LittleEndianReadFileTest.dcm");

            DicomAttributeCollection dataSet = file.DataSet;

            SetupMR(dataSet);

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            DicomReadOptions readOptions = DicomReadOptions.Default;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.StorePixelDataReferences;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.DoNotStorePixelDataInDataSet;
            ReadOptionsTest(file, readOptions, false);

            readOptions = DicomReadOptions.None;
            ReadOptionsTest(file, readOptions, true);


            // Big Endian Tests
            file.Filename = "BigEndianReadTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            readOptions = DicomReadOptions.Default;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.StorePixelDataReferences;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.DoNotStorePixelDataInDataSet;
            ReadOptionsTest(file, readOptions, false);

            readOptions = DicomReadOptions.None;
            ReadOptionsTest(file, readOptions, true);

        }
        [Test]
        public void MultiframeReadTest()
        {
            DicomFile file = new DicomFile("LittleEndianMultiframeTest.dcm");

            DicomAttributeCollection dataSet = file.DataSet;

            SetupMultiframeXA(dataSet, 511, 511, 5);

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            DicomReadOptions readOptions = DicomReadOptions.Default;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.StorePixelDataReferences;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.DoNotStorePixelDataInDataSet;
            ReadOptionsTest(file, readOptions, false);

            readOptions = DicomReadOptions.None;
            ReadOptionsTest(file, readOptions, true);


            // Big Endian Tests
            file.Filename = "BigEndianMultiframeTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            readOptions = DicomReadOptions.Default;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.StorePixelDataReferences;
            ReadOptionsTest(file, readOptions, true);

            readOptions = DicomReadOptions.DoNotStorePixelDataInDataSet;
            ReadOptionsTest(file, readOptions, false);

            readOptions = DicomReadOptions.None;
            ReadOptionsTest(file, readOptions, true);

        }

        private void CheckPixels(string filename)
        {
            DicomReadOptions readOptions = DicomReadOptions.StorePixelDataReferences;
            DicomFile newFile = new DicomFile(filename);

            newFile.Load(readOptions);
            DicomUncompressedPixelData pd = new DicomUncompressedPixelData(newFile.DataSet);

            for (int frame = 0; frame < pd.NumberOfFrames; frame++)
            {
                byte[] data = pd.GetFrame(frame);
                uint pdVal = (uint)frame + 1;
                for (int i = 0; i < pd.UncompressedFrameSize; i++, pdVal++)
                {
                    if (data[i] != pdVal%255)
                    {
                        string val = String.Format("Value bad: frame: {0}, pixel: {1}, val1: {2}, val2: {3}", frame, i, data[i], pdVal%255);
                        Console.Write(val);
                    }
                    Assert.AreEqual(data[i], pdVal%255);
                }
            }            
        }

        [Test]
        public void ReadPixelsFromDisk()
        {

            // Check odd dimension file w/ odd frames
            DicomFile file = new DicomFile("LittlePixelTest.dcm");

            DicomAttributeCollection dataSet = file.DataSet;

            SetupMultiframeXA(dataSet,511,511,5);

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            bool result = file.Save(DicomWriteOptions.Default);
            Assert.AreEqual(result, true);

            CheckPixels(file.Filename);

            // Big Endian Tests
            file.Filename = "BigEndianMultiframeTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            result = file.Save(DicomWriteOptions.Default);
            Assert.AreEqual(result, true);

            CheckPixels(file.Filename);

            // Now check even size w/ even number of frames

            // Setup the file
            file = new DicomFile("LittlePixelTest.dcm");

            dataSet = file.DataSet;

            SetupMultiframeXA(dataSet, 512, 512, 6);

            SetupMetaInfo(file);

            // Little Endian Tests
            file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

            result = file.Save(DicomWriteOptions.Default);
            Assert.AreEqual(result, true);

            CheckPixels(file.Filename);

            // Big Endian Tests
            file.Filename = "BigEndianMultiframeTest.dcm";
            file.TransferSyntax = TransferSyntax.ExplicitVrBigEndian;

            result = file.Save(DicomWriteOptions.Default);
            Assert.AreEqual(result, true);

            CheckPixels(file.Filename);

        }


		[Test]
		public void SimpleFileTest()
		{

			DicomFile file = new DicomFile("LittleEndianReadFileTest.dcm");

			DicomAttributeCollection dataSet = file.DataSet;

			SetupMR(dataSet);

			SetupMetaInfo(file);

			dataSet[DicomTags.StudyDescription].SetNullValue();

			// Little Endian Tests
			file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

			DicomReadOptions readOptions = DicomReadOptions.Default;

			bool result = file.Save(DicomWriteOptions.Default);

			Assert.AreEqual(result, true);

			DicomFile newFile = new DicomFile(file.Filename);

			newFile.Load(readOptions);

			Assert.IsTrue(newFile.DataSet[DicomTags.StudyDescription].IsNull);

			Assert.AreEqual(file.DataSet.Equals(newFile.DataSet), true);

			DicomAttributeCollection dicomAttributeCollection = new DicomAttributeCollection();
			dicomAttributeCollection[DicomTags.PatientId].SetNullValue();
			Assert.IsFalse(dicomAttributeCollection[DicomTags.PatientId].IsEmpty, "Dicom Tag is empty, won't be written in DicomStreamWriter.Write()");
		}

		[Test]
		public void ReadPixelDataReferencesTest()
		{
			DicomFile file = new DicomFile("LittleEndianReadFileTest2.dcm");

			DicomAttributeCollection dataSet = file.DataSet;

			SetupMR(dataSet);

			SetupMetaInfo(file);

			// Little Endian Tests
			file.TransferSyntax = TransferSyntax.ExplicitVrLittleEndian;

			DicomReadOptions readOptions = DicomReadOptions.StorePixelDataReferences;
			bool result = file.Save(DicomWriteOptions.Default);

			Assert.AreEqual(result, true);

			DicomFile newFile = new DicomFile(file.Filename);

			newFile.Load(readOptions);

			DicomAttribute attrib = newFile.DataSet[DicomTags.PixelData];

			Assert.IsFalse(attrib.IsEmpty);
			Assert.IsFalse(attrib.IsNull);
			Assert.AreEqual(attrib.StreamLength, dataSet[DicomTags.PixelData].StreamLength);

			// Set the pixel data to null and re-read
			dataSet[DicomTags.PixelData].SetNullValue();
			
			result = file.Save(DicomWriteOptions.Default);
			Assert.AreEqual(result, true);

			newFile = new DicomFile(file.Filename);

			newFile.Load(readOptions);

			attrib = newFile.DataSet[DicomTags.PixelData];

			Assert.IsFalse(attrib.IsEmpty);
			Assert.IsTrue(attrib.IsNull);
			Assert.AreEqual(attrib.StreamLength, dataSet[DicomTags.PixelData].StreamLength);
		}
    }
}
#endif