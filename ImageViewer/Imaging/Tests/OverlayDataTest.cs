﻿#region License

// Copyright (c) 2010, ClearCanvas Inc.
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

#if	UNIT_TESTS
#pragma warning disable 1591,0419,1574,1587

using System.Text;
using NUnit.Framework;

namespace ClearCanvas.ImageViewer.Imaging.Tests
{
	[TestFixture]
	public class OverlayDataTest
	{
		#region OverlayData Tests

		[Test]
		public void TestUnpack_SingleFrameA()
		{
			// 1 frame, 5x3
			// 11111
			// 10001
			// 11111
			// continuous bit stream: 11111100 0111111x
			// continuous LE byte stream: 00111111 x1111110
			// continuous BE word stream: x1111110 00111111

			const string expectedResult = "111111000111111";
			var packedBits = new byte[] {0x3f, 0x7e};

			// little endian test
			{
				var unpackedBits = new OverlayData(3, 5, false, packedBits).Unpack();
				var actualResult = FormatNonZeroBytes(unpackedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in unpacked bits for single frame case A (little endian)");
			}

			// big endian test
			{
				var unpackedBits = new OverlayData(3, 5, true, SwapBytes(packedBits)).Unpack();
				var actualResult = FormatNonZeroBytes(unpackedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in unpacked bits for single frame case A (big endian)");
			}
		}

		[Test]
		public void TestUnpack_SingleFrameB()
		{
			// 1 frame, 5x3
			// 11111
			// 10010
			// 00000
			// continuous bit stream: 11111100 1000000x
			// continuous LE byte stream: 00111111 x0000001

			const string expectedResult = "111111001000000";
			var packedBits = new byte[] {0x3f, 0x81};

			// little endian test
			{
				var unpackedBits = new OverlayData(3, 5, false, packedBits).Unpack();
				var actualResult = FormatNonZeroBytes(unpackedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in unpacked bits for single frame case B (little endian)");
			}

			// big endian test
			{
				var unpackedBits = new OverlayData(3, 5, true, SwapBytes(packedBits)).Unpack();
				var actualResult = FormatNonZeroBytes(unpackedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in unpacked bits for single frame case B (big endian)");
			}
		}

		[Test]
		public void TestUnpack_Multiframe()
		{
			// 7 frames, each 3x2 (cols, rows) = 42 bits = 6 bytes
			// 111 111 111 111 111 111 111
			// 000 001 010 011 100 101 110
			// continuous bit stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx
			// continuous LE byte stream: 11000111 01111001 11011101 11001111 11111011 xxxxxx01
			var expectedResult = new[] {"111000", "111001", "111010", "111011", "111100", "111101", "111110"};
			var packedBits = new byte[] {0xc7, 0x79, 0xdd, 0xcf, 0xfb, 0xf1};

			for (int frameIndex = 0; frameIndex < 7; frameIndex++)
			{
				var offset = 3*2*frameIndex;

				// little endian test
				{
					var unpackedBits = new OverlayData(offset, 2, 3, false, packedBits).Unpack();
					var actualResult = FormatNonZeroBytes(unpackedBits);
					Assert.AreEqual(expectedResult[frameIndex], actualResult, "Error in unpacked bits for multiframe (fr#{0}) (little endian)", frameIndex);
				}

				// big endian test
				{
					var unpackedBits = new OverlayData(offset, 2, 3, true, SwapBytes(packedBits)).Unpack();
					var actualResult = FormatNonZeroBytes(unpackedBits);
					Assert.AreEqual(expectedResult[frameIndex], actualResult, "Error in unpacked bits for multiframe (fr#{0}) (big endian)", frameIndex);
				}
			}
		}

		[Test]
		public void TestUnpackFromPixelData_8BitsAllocated()
		{
			// 1 frame, 5x3
			// 11111
			// 10010
			// 00000
			// continuous bit stream: 11111100 1000000x
			// continuous LE byte stream: 00111111 x0000001
			const string expectedResult = "111111001000000";

			// 8 bits allocated

			// some pixel data: 1st col is a random "pixel", 2nd col creates room for overlay bit, 3rd col is the overlay
			var pixelData = new byte[]
			                	{
			                		(0xC5 & 0xFE) | 1,
			                		(0x2D & 0xFE) | 1,
			                		(0x5B & 0xFE) | 1,
			                		(0xB3 & 0xFE) | 1,
			                		(0xFC & 0xFE) | 1,
			                		(0xBC & 0xFE) | 1,
			                		(0x4d & 0xFE) | 0,
			                		(0xbf & 0xFE) | 0,
			                		(0x86 & 0xFE) | 1,
			                		(0x75 & 0xFE) | 0,
			                		(0xA8 & 0xFE) | 0,
			                		(0x19 & 0xFE) | 0,
			                		(0xAC & 0xFE) | 0,
			                		(0xD4 & 0xFE) | 0,
			                		(0x79 & 0xFE) | 0
			                	};

			var extractedBits = OverlayData.UnpackFromPixelData(0, 8, false, pixelData);
			var actualResult = FormatNonZeroBytes(extractedBits);
			Assert.AreEqual(expectedResult, actualResult, "Error in extracted bits from pixel data (8-bit pixels)");
		}

		[Test]
		public void TestUnpackFromPixelData_16BitsAllocated()
		{
			// 1 frame, 5x3
			// 11111
			// 10010
			// 00000
			// continuous bit stream: 11111100 1000000x
			// continuous LE byte stream: 00111111 x0000001
			const string expectedResult = "111111001000000";

			// 16 bits allocated

			// some pixel data: 1st col is a random "pixel", 2nd col creates room for overlay bit, 3rd col is the overlay
			//                  4th col moves the overlay to bit position 14, 5th col is the lower byte of the random pixel
			var pixelData = SwapBytes(new byte[] // written in big endian form for ease of reading :P
			                          	{
			                          		(0xC5 & 0x0F) | (1 << 6), 0x93,
			                          		(0x2D & 0x0F) | (1 << 6), 0x9C,
			                          		(0x5B & 0x0F) | (1 << 6), 0x78,
			                          		(0xB3 & 0x0F) | (1 << 6), 0x17,
			                          		(0xFC & 0x0F) | (1 << 6), 0x0c,
			                          		(0xBC & 0x0F) | (1 << 6), 0xc4,
			                          		(0x4d & 0x0F) | (0 << 6), 0x45,
			                          		(0xbf & 0x0F) | (0 << 6), 0xcd,
			                          		(0x86 & 0x0F) | (1 << 6), 0xAE,
			                          		(0x75 & 0x0F) | (0 << 6), 0xA9,
			                          		(0xA8 & 0x0F) | (0 << 6), 0x29,
			                          		(0x19 & 0x0F) | (0 << 6), 0x11,
			                          		(0xAC & 0x0F) | (0 << 6), 0xDD,
			                          		(0xD4 & 0x0F) | (0 << 6), 0x01,
			                          		(0x79 & 0x0F) | (0 << 6), 0xF0
			                          	});

			// little endian
			{
				var extractedBits = OverlayData.UnpackFromPixelData(14, 16, false, pixelData);
				var actualResult = FormatNonZeroBytes(extractedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in extracted bits from pixel data (16-bit pixels, little endian)");
			}

			// big endian
			{
				var extractedBits = OverlayData.UnpackFromPixelData(14, 16, true, SwapBytes(pixelData));
				var actualResult = FormatNonZeroBytes(extractedBits);
				Assert.AreEqual(expectedResult, actualResult, "Error in extracted bits from pixel data (16-bit pixels, big endian)");
			}
		}

		#endregion

		#region Algorithm Tests

		[Test]
		public void TestBitUnpacking_SingleFrame()
		{
			byte[] testData;

			// 1 frame, 5x3
			// 11111
			// 10001
			// 11111
			// continuous bit stream: 11111100 0111111x
			// continuous LE byte stream: 00111111 x1111110
			// continuous BE word stream: x1111110 00111111
			testData = new byte[] {0x3f, 0x7e};
			TestFrame(testData, 5*3, 0, "111111000111111", "continuous stream: 11111100 0111111x");

			// 1 frame, 5x3
			// 11111
			// 10010
			// 00000
			// continuous bit stream: 11111100 1000000x
			// continuous LE byte stream: 00111111 x0000001
			testData = new byte[] {0x3f, 0x81};
			TestFrame(testData, 5*3, 0, "111111001000000", "continuous stream: 11111100 1000000x");
		}

		[Test]
		public void TestBitUnpacking_MultiFrame()
		{
			byte[] testData;

			// 7 frames, each 3x2 (cols, rows) = 42 bits = 6 bytes
			// 111 111 111 111 111 111 111
			// 000 001 010 011 100 101 110
			// continuous bit stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx
			// continuous LE byte stream: 11000111 01111001 11011101 11001111 11111011 xxxxxx01
			testData = new byte[] {0xc7, 0x79, 0xdd, 0xcf, 0xfb, 0xf1};
			TestFrame(testData, 3*2, 0, "111000", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 1, "111001", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 2, "111010", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 3, "111011", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 4, "111100", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 5, "111101", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
			TestFrame(testData, 3*2, 6, "111110", "continuous stream: 11100011 10011110 10111011 11110011 11011111 10xxxxxx");
		}

		private static void TestFrame(byte[] packedData, int outBufferSize, int frameNum, string expectedConcat, string datamsg)
		{
			byte[] result = new byte[outBufferSize];
			OverlayData.TestUnpack(packedData, result, frameNum*outBufferSize, false);
			Assert.AreEqual(expectedConcat, FormatNonZeroBytes(result), "LittleEndianWords Frame {0} of {1}", frameNum, datamsg);

			// you should get the exact same frame data (scanning horizontally from top left to bottom right) if you had packed data in little endian or big endian
			byte[] swappedPackedData = SwapBytes(packedData);
			result = new byte[outBufferSize];
			OverlayData.TestUnpack(swappedPackedData, result, frameNum*outBufferSize, true);
			Assert.AreEqual(expectedConcat, FormatNonZeroBytes(result), "BigEndianWords Frame {0} of {1}", frameNum, datamsg);
		}

		#endregion

		private static byte[] SwapBytes(byte[] swapBytes)
		{
			byte[] output = new byte[swapBytes.Length];
			for (int n = 0; n < swapBytes.Length; n += 2)
			{
				output[n + 1] = swapBytes[n];
				output[n] = swapBytes[n + 1];
			}
			return output;
		}

		/// <summary>
		/// Formats the bits of a byte array as a sequence of 1s and 0s.
		/// </summary>
		private static string FormatBits(byte[] data, bool bigEndianWords)
		{
			var sb = new StringBuilder(data.Length*8);
			if (bigEndianWords)
			{
				for (int n = 0; n < data.Length; n += 2)
				{
					// the lsb is the 2nd byte in each big endian word
					// list each bit of the byte from lsb to msb
					for (int bit = 0; bit < 8; bit++)
						sb.Append((data[n + 1] >> bit)%2 == 1 ? '1' : '0');
					for (int bit = 0; bit < 8; bit++)
						sb.Append((data[n] >> bit)%2 == 1 ? '1' : '0');
				}
			}
			else
			{
				for (int n = 0; n < data.Length; n++)
				{
					// list each bit of the byte from lsb to msb
					for (int bit = 0; bit < 8; bit++)
						sb.Append((data[n] >> bit)%2 == 1 ? '1' : '0');
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Formats a byte array as a sequence of 1s and 0s representing non-zero and zero bytes respectively.
		/// </summary>
		private static string FormatNonZeroBytes(byte[] data)
		{
			var sb = new StringBuilder(data.Length);
			for (int n = 0; n < data.Length; n++)
				sb.Append(data[n] > 0 ? '1' : '0');
			return sb.ToString();
		}
	}
}

#endif