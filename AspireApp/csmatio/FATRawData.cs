using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CSharp;	//for dynamic var

using AspireApp.Libraries; // for C# extensions

using csmatio.common;
using csmatio.io;
using csmatio.types;

namespace csmatio
{
	/// <summary>
	/// FAT Raw Data object
	/// This class was designed for read a MAT in the first stage, so it deleived from MatFileReader.
	/// And then add write to MAT function.
	/// How to use:
	/// 1. Read FAT Raw Data (MAT file)
	///		byte[] data = File.ReadAllBytes(strMatFile);
	///		FATRawData rawdata = new FATRawData(data);
	///		For detail, see ReadMe.md in GitLab for the project
	///	2. Write FAT Raw Data (MAT file)
	///		1.1 create MatFileHeader
	///		1.2 create Data property
	///		1.3 Write()
	/// </summary>
	public class FATRawData : MatFileReader
	{
		#region inner classes
		/// <summary>
		/// a MatTag for writing
		/// </summary>
		protected class OSMatTag : ISMatTag
		{
			/// <summary>
			/// a new tab for write data
			/// </summary>
			/// <param name="Type">tag type, all are defined in MatDataType </param>
			/// <param name="Size">data size</param>
			/// <param name="bCompressed">true:use 16 bit for tag, false:32 bit for tag</param>
			public OSMatTag(int Type, int Size, bool bCompressed = false) :
				base(Type, Size)
			{
				IsCompressed = bCompressed;
				Padding = GetPadding(_size, IsCompressed);
			}

			#region Public Func
			/// <summary>
			/// write tag data only (type and size)
			/// </summary>
			/// <param name="oStream">stream</param>
			/// <param name="pos">this tag position, if it is a null, the position should be current position</param>
			public void Write(Stream oStream, long? pos = null)
			{
				long? posCurrent = null;
				if (pos != null)
				{//need to change the data position.
					posCurrent = oStream.Position;
					oStream.Seek(pos.Value, SeekOrigin.Begin);
				}
				byte[]? buffer = null;
				if (IsCompressed)
				{//using 4 byte for the tag type & size
					buffer = new byte[sizeof(short) * 2];
					var temp = BitConverter.GetBytes((short)_type);
					if (!BitConverter.IsLittleEndian)
					{//current CPU work with Big endian, need to reverse the array, make sure it must be little endian
						Array.Reverse(temp);
					}
					Buffer.BlockCopy(temp, 0, buffer, 0, sizeof(short));
					temp = BitConverter.GetBytes((short)_size);
					if (!BitConverter.IsLittleEndian)
					{//current CPU work with Big endian, need to reverse the array, make sure it must be little endian
						Array.Reverse(temp);
					}
					Buffer.BlockCopy(temp, 0, buffer, sizeof(short), sizeof(short));
				}
				else
				{
					buffer = new byte[sizeof(int) * 2];
					var temp = BitConverter.GetBytes(_type);
					if (!BitConverter.IsLittleEndian)
					{//current CPU work with Big endian, need to reverse the array, make sure it must be little endian
						Array.Reverse(temp);
					}
					Buffer.BlockCopy(temp, 0, buffer, 0, sizeof(int));
					temp = BitConverter.GetBytes(_size);
					if (!BitConverter.IsLittleEndian)
					{//current CPU work with Big endian, need to reverse the array, make sure it must be little endian
						Array.Reverse(temp);
					}
					Buffer.BlockCopy(temp, 0, buffer, sizeof(int), sizeof(int));
				}
				oStream.Write(buffer);
				if (posCurrent != null)
				{//move back the position
					oStream.Seek(posCurrent.Value, SeekOrigin.Begin);
				}
			}
			#endregion

			#region properties
			/// <summary>
			/// Is Compressed
			/// </summary>
			public bool IsCompressed { get; private set; } = false;
			#endregion
		}
		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		public FATRawData()
		{
			//create header, it can be checked by uing Header property
			_matFileHeader = MatFileHeader.CreateHeader();
		}
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="data"></param>
		public FATRawData(byte[] data) : base(data)
		{
		}

		#region Properties
		/// <summary>
		/// Header of MAT file
		/// </summary>
		public MatFileHeader? Header { get=> _matFileHeader; }

		/// <summary>
		/// Data part of this MAT file
		/// </summary>
		public new dynamic? Data { get; set; } = null;
		#endregion

		#region Public methodes
		/// <summary>
		/// write out all data into a MAT file stream
		/// before call this, Hearder and Data must be set.
		/// </summary>
		/// <param name="oStream">a file stream, can be a <c>MemoryStream</c></param>
		/// <param name="bRenewHeader">true : renew the header data, especailly the time stamp. when you read a mat data, want to save it by using same header, use bRenewHeader=false</param>
		/// <param name="bCompress">true if want to compress the data part</param>
		public void WriteData(Stream oStream, bool bRenewHeader = true, bool bCompress = true)
		{
			if ((Header != null) && (Data != null))
			{
				WriteHeader(oStream, bRenewHeader);

				//Write all data
				byte[]? oData = null;
				//Serialize data into a MemmoryStram. 200MB for maximum data
				using (MemoryStream ms = new MemoryStream(204800000))
				{
					WriteMatrix(ms, Data!.Cc_struct, "cc_struct");
					oData = ms.ToArray();
				}
				if (oData != null)
				{
					if (bCompress)
					{//to compress the data part
						WriteCompressedData(oStream, oData);
					}
					else
					{
						oStream.Write(oData);
					}
				}
			}
		}
		#endregion

		#region protected methodes
		#region overrode methodes
		/// <summary>
		/// read data as FAT Raw data (2021/9/28 by Gyomei)
		/// </summary>
		/// <param name="buf"></param>
		protected override void ReadData(Stream buf)
		{
			// read data
			var tag = new ISMatTag(buf);
			switch (tag.Type)
			{
				case MatDataTypes.miCOMPRESSED:
					// inflate and recur
					{
						var uncompressed = Inflate(buf, tag.Size);
						ReadData(uncompressed);
						uncompressed.Close();
					}
					break;
				case MatDataTypes.miMATRIX:
					// read in the matrix
					var pos = (int)buf.Position;
					int red, toread;

					var element = ReadMatrix(buf, true);

					if (element != null)
					{
						Data = element;
					}
					else
					{
						red = (int)buf.Position - pos;
						toread = tag.Size - red;
						// for speed up, must use seek() by Gyomei 2021/9/29
						//buf.Position = buf.Position + toread;
						buf.Seek(buf.Position + toread, SeekOrigin.Begin);
					}
					red = (int)buf.Position - pos;

					toread = tag.Size - red;

					if (toread != 0)
					{
						throw new MatlabIOException("Matrix was not read fully! " + toread + " remaining in the buffer.");
					}
					break;
				default:
					throw new MatlabIOException("Incorrect data tag: " + tag);

			}
		}

		/// <summary>
		/// read matrix as Raw Data
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="isRoot"></param>
		/// <returns></returns>
		protected override object? ReadMatrix(Stream buf, bool isRoot)
		{
			// result
			dynamic? oRcd = null;
			ISMatTag tag;

			// read flags
			var flags = ReadFlags(buf);
			var attributes = (flags.Length != 0) ? flags[0] : 0;
			var nzmax = (flags.Length != 0) ? flags[1] : 0;
			var type = attributes & 0xff;

			// read Array dimension
			var dims = ReadDimension(buf);

			// read Array name
			var name = ReadName(buf);

			// If this array is filtered out return immediately
			if (isRoot && !_filter.Matches(name))
			{
				return null;
			}

			//number of elements of this Matrix
			long nLength = GetArrayLengthFromDimensions(dims);

			// read data
			switch (type)
			{
				case MLArray.mxSTRUCT_CLASS:	//Ex. Data.cc_struct[0,0].info[0,0].sensor, Data.cc_struct[0,0].Data[0,3,2,5]
					if (nLength > 0)// this means we have at leat one element in this matrix
					{
						var structArray = Array.CreateInstance(typeof(object), dims);
						var oStruct = new DynamicDictionary();
						object oUse = nLength == 1 ? (object)oStruct : (object)structArray;
						if (string.IsNullOrEmpty(name))
						{// return array if no name
							oRcd = oUse;
						}
						else
						{//return a clase with sub property of the name, and the property value will be a Array
							var oStruct1 = new DynamicDictionary();
							oStruct1.Set(name, oUse);
							oRcd = oStruct1;
						}

						var br = new BinaryReader(buf);

						// field name length - this subelement always uses the compressed data element format
						tag = new ISMatTag(br.BaseStream);
						var maxlen = br.ReadInt32();

						// Read fields data as Int8
						tag = new ISMatTag(br.BaseStream);
						// calculate number of fields
						var numOfFields = tag.Size / maxlen;

						//get names
						var fieldNames = new string[numOfFields];
						for (var i = 0; i < numOfFields; i++)
						{
							var names = new byte[maxlen];
							br.Read(names, 0, names.Length);
							fieldNames[i] = ZeroEndByteArrayToString(names);
						}
						// seek to next position
						if (tag.Padding > 0)
						{
							br.BaseStream.Seek(br.BaseStream.Position + tag.Padding, SeekOrigin.Begin);
						}

						// read fields
						for (var index = 0; index < nLength; index++)
						{
							DynamicDictionary element = oStruct;
							if(nLength>1)
							{
								element = new DynamicDictionary();
								structArray.SetValue(index, element, false);
							}
							foreach (string strName in fieldNames)
							{
								// read matrix recursively
								tag = new ISMatTag(br.BaseStream);
								if (tag.Size > 0)
								{
									dynamic? fieldValue = ReadMatrix(br.BaseStream, false);
									element.Set(strName, fieldValue);
								}
							}
						}
					}
					break;
				case MLArray.mxCELL_CLASS:
					if(nLength>0)
					{
						var cellArray = Array.CreateInstance(typeof(object), dims);
						if (string.IsNullOrEmpty(name))
						{// return array if no name
							oRcd = cellArray;
						}
						else
						{//return a clase with sub property of the name, and the property value will be a Array
							var oCell = new DynamicDictionary();
							oCell.Set(name, (object)cellArray);
							oRcd = oCell;
						}
						for (var i = 0; i < nLength; i++)
						{
							tag = new ISMatTag(buf);
							if (tag.Size > 0)
							{
								var cellmatrix = ReadMatrix(buf, false);
								cellArray.SetValue(i, cellmatrix, false);
							}
						}
					}
					break;
				case MLArray.mxDOUBLE_CLASS:    //double array
					oRcd = ReadData<double>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxSINGLE_CLASS:    //single (float) array
					oRcd = ReadData<float>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxUINT8_CLASS:	//byte array
					oRcd = ReadData<byte>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxINT8_CLASS:	//sbyte[]
					oRcd = ReadData<sbyte>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxUINT16_CLASS:	//ushort[]
					oRcd = ReadData<ushort>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxINT16_CLASS:	//short[]
					oRcd = ReadData<short>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxUINT32_CLASS:	//uint[]
					oRcd = ReadData<uint>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxINT32_CLASS:	//int[]
					oRcd = ReadData<int>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxUINT64_CLASS:	//ulong[]
					oRcd = ReadData<ulong>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxINT64_CLASS:	//long[]
					oRcd = ReadData<long>(nLength, buf, name, dims, attributes);
					break;
				case MLArray.mxCHAR_CLASS:	//string
					if(nLength>0)
					{
						tag = new ISMatTag(buf);
						string strTemp = tag.Read();
						if (string.IsNullOrEmpty(name))
						{
							oRcd = strTemp;
						}
						else
						{//return a clase with sub property of the name, and the property value will be a string
							oRcd = new DynamicDictionary();
							oRcd.Set(name, (object)strTemp);
						}
					}
					break;
				case MLArray.mxSPARSE_CLASS:	// sparse matrix (most of the elements are 0), 
					if(nLength>0)
					{
						if (dims.Length == 2)
						{//MATLAB only support two-dimesion sparse arrys
							dynamic root = new DynamicDictionary();
							if (string.IsNullOrEmpty(name))
							{//no name
								oRcd = root;
							}
							else
							{//return a clase with sub property of the name
								oRcd = new DynamicDictionary();
								oRcd.Set(name, (object)root);
							}

							//the root should contans a index list, a dictionary for reals, and a dictionary for imaginaries if need
							// read ir (row indices)
							tag = new ISMatTag(buf);
							var ir = tag.ReadToIntArray();
							// read jc (column indices)
							tag = new ISMatTag(buf);
							var jc = tag.ReadToIntArray();

							// read pr (real part)
							tag = new ISMatTag(buf);
							double[] reals = tag.Read<double>();
							Dictionary<IndexMN, double> Reals = new Dictionary<IndexMN, double>();
							int N = dims[1];
							var n = 0;
							for (var i = 0; i < ir.Length; i++)
							{
								if (i < N)
								{
									n = jc[i];
								}
								Reals.TryAdd(new IndexMN(ir[i], n), reals[i]);
							}
							root.Reals = Reals;
							//read pi (imaginary part)
							if (IsComplex(attributes))
							{
								tag = new ISMatTag(buf);
								double[] imaginaries = tag.Read<double>();
								Dictionary<IndexMN, double> Imaginaries = new Dictionary<IndexMN, double>();

								var n1 = 0;
								for (var i = 0; i < ir.Length; i++)
								{
									if (i < N)
									{
										n1 = jc[i];
									}
									Imaginaries.TryAdd(new IndexMN(ir[i], n1), imaginaries[i]);
								}
								root.Imaginaries = Imaginaries;
							}
						}
						else
						{
							throw new MatlabIOException($"Incorrect dimensions{dims.Length} of Matlab sparse array class, must be 2 dimensions");
						}
					}
					break;
				default:
					throw new MatlabIOException("Incorrect Matlab array class: " + MLArray.TypeToString(type));
			}
			return oRcd;
		}
		#endregion
		#region inner methode
		/// <summary>
		/// read different type data in array
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nLength"></param>
		/// <param name="buf"></param>
		/// <param name="name"></param>
		/// <param name="dims"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		protected object? ReadData<T>(long nLength, Stream buf, string name, int[] dims, int attributes)
		{
			dynamic? oRcd = null;
			if (nLength >= 0)
			{//0: need skip the tag
				Type tempType = typeof(T);
				bool isComplex = IsComplex(attributes);
				//read real
				ISMatTag tag = new ISMatTag(buf);
				T[] Reals = tag.Read<T>();
				if ((nLength > 0) && (Reals.Length > 0))
				{
					if (isComplex)
					{//oRcd.Complex.Reals[2,3,1,0]. In FAT software we do not find complex data
						tag = new ISMatTag(buf);
						T[] Imaginaries = tag.Read<T>();
						if (Imaginaries.Length > 0)
						{
							oRcd = new DynamicDictionary();
							oRcd.Complex = new DynamicDictionary();
							if (nLength == 1)
							{//only one member
								oRcd.Complex.Reals = Reals[0];
								oRcd.Complex.Imaginaries = Imaginaries[0];
							}
							else
							{
								dims = ToNoLength1Dimensions(dims);
								if ((dims != null) && dims.Length > 0)
								{//checked dims is right
									oRcd.Complex.Reals = Array.CreateInstance(tempType, dims);
									oRcd.Complex.Imaginaries = Array.CreateInstance(tempType, dims);
									for (var i = 0; i < nLength; i++)
									{
										if (i < Reals.Length)
										{
											oRcd.Complex.Reals.SetValue(i, Reals[i], false);
										}
										if (i < Reals.Length)
										{
											oRcd.Complex.Imaginaries.SetValue(i, Imaginaries[i], false);
										}
									}
								}
							}
						}
					}
					else
					{//Ex. oRcd[1,3,5]
						if (nLength == 1)
						{//only one elements
							oRcd = Reals[0];
						}
						else
						{
							dims = ToNoLength1Dimensions(dims);
							if ((dims != null) && dims.Length > 0)
							{//checked dims is right
								var temp = Array.CreateInstance(tempType, dims);
								oRcd = temp;
								GCHandle gchArray = GCHandle.Alloc(Reals, GCHandleType.Pinned);
								GCHandle gchNew = GCHandle.Alloc(temp, GCHandleType.Pinned);
								IntPtr ptrArray = gchArray.AddrOfPinnedObject();
								IntPtr ptrNew = gchNew.AddrOfPinnedObject();
								int nElementSize = Marshal.SizeOf(Reals[0]);
								if (dims.Length == 1)
								{//use Buffer.Copy(),
									long nCopySize = nElementSize * nLength;
									unsafe
									{
										//fist data point for each array
										byte* pArray = (byte*)ptrArray.ToPointer();
										byte* pNew = (byte*)ptrNew.ToPointer();
										Buffer.MemoryCopy(pArray, pNew, nCopySize, nCopySize);
									}
								}
								else
								{
									//The data sequnce from MAT is defferent from C# multi demension array
									// Ex. for csArray[2,3,2]
									//	from MAT data		stream		normal array
									// csArray[0,0,0] <-- readin[0]	--> csArray[0,0,0]
									// csArray[1,0,0] <-- readin[1]	--> csArray[0,0,1]
									// csArray[0,1,0] <-- readin[2]	--> csArray[0,1,0]
									// csArray[1,1,0] <-- readin[3]	--> csArray[0,1,1]
									// csArray[0,2,0] <-- readin[4]	--> csArray[0,2,0]
									// csArray[1,2,0] <-- readin[5]	--> csArray[0,2,1]
									// csArray[0,0,1] <-- readin[6]	--> csArray[1,0,0]
									// csArray[1,0,1] <-- readin[7]	--> csArray[1,0,1]
									// csArray[0,1,1] <-- readin[8]	--> csArray[1,1,0]
									// csArray[1,1,1] <-- readin[9]	--> csArray[1,1,1]
									// csArray[0,2,1] <-- readin[10]--> csArray[1,2,0]
									// csArray[1,2,1] <-- readin[11]--> csArray[1,2,1]

									//try to use use Array.IncreamentIndex(,,bool bMemorySequnce = false), but it take more cpu time then using temp.SetValue(i, Reals[i], false)
									// 3.5sec
									//var indices = new int[dims.Length];
									//Array.Clear(indices, 0, dims.Length);
									//var rangs = new SliceIndex[dims.Length];
									//for (var i = 0; i < rangs.Length; i++)
									//{
									//	rangs[i] = new SliceIndex()
									//	{
									//		Start = 0,
									//		Stop = dims[i],
									//		OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
									//	};
									//}
									//for (var i = 0; i < nLength; i++)
									//{
									//	if (i < Reals.Length)
									//	{
									//		temp.SetValue(Reals[i], indices);
									//		ArrayExtensions.IncreamentIndex(indices, rangs, false);
									//	}
									//	else
									//	{//should not be here
									//		break;
									//	}
									//}

									//use temp.SetValue(i, Reals[i], false) --> 2.742sec
									//for (var i = 0; i < nLength; i++)
									//{
									//	if (i < Reals.Length)
									//	{
									//		temp.SetValue(i, Reals[i], false);
									//	}
									//	else
									//	{//should not be here
									//		break;
									//	}
									//}

									//increase the indeces directlly --> 1.597sec
									//var indices = new int[dims.Length];
									//Array.Clear(indices, 0, dims.Length);
									//for (var i = 0; i < nLength; i++)
									//{
									//	if (i < Reals.Length)
									//	{
									//		temp.SetValue(Reals[i], indices);
									//		//increase indices
									//		for (int j = 0; j < indices.Length; j++)
									//		{
									//			indices[j] ++;
									//			if (indices[j] >= dims[j])
									//			{
									//				indices[j] = 0;
									//			}
									//			else
									//			{
									//				break;
									//			}
									//		}
									//	}
									//	else
									//	{//should not be here
									//		break;
									//	}
									//}

									//increase the indeces directlly --> 1.36sec
									var indices = new int[dims.Length];
									Array.Clear(indices, 0, dims.Length);
									unsafe
									{
										//fist data point for each array
										byte* pArray = (byte*)ptrArray.ToPointer();
										byte* pNew = (byte*)ptrNew.ToPointer();
										int nOffsetNew = 0;
										for (var i = 0; i < nLength; i++)
										{
											if (i < Reals.Length)
											{
												Buffer.MemoryCopy(pArray + i * nElementSize, pNew + nOffsetNew * nElementSize, nElementSize, nElementSize);
												//increase indices, and get the offset in temp array
												for (int j = 0; j < indices.Length; j++)
												{
													indices[j]++;
													if (indices[j] >= dims[j])
													{
														indices[j] = 0;
													}
													else
													{
														break;
													}
												}
												//calculate offset in memory for this indices
												nOffsetNew = 0;
												for (int j = 0; j < indices.Length - 1; j++)
												{
													nOffsetNew = (nOffsetNew + indices[j]) * dims[j + 1];
												}
												nOffsetNew += indices[^1];
											}
											else
											{//should not be here
												break;
											}
										}
									}
								}
								//unlock those memroy from garbage collection
								gchArray.Free();
								gchNew.Free();
							}
						}
					}
				}
				else
				{
					oRcd = default(T);
				}
			}
			return oRcd;
		}

		/// <summary>
		/// number of elements of the matrix
		/// </summary>
		/// <param name="dims"></param>
		/// <returns></returns>
		protected long GetArrayLengthFromDimensions(int[] dims)
		{
			return dims.Aggregate(1, (a, b) => a * b);
		}

		/// <summary>
		/// is Complex
		/// </summary>
		/// <param name="nAtribute"></param>
		/// <returns></returns>
		protected bool IsComplex(int nAtribute)
		{
			const int mtFLAG_COMPLEX = 0x0800;
			return (nAtribute & mtFLAG_COMPLEX) != 0;
		}

		/// <summary>
		/// is Logical
		/// </summary>
		/// <param name="nAtribute"></param>
		/// <returns></returns>
		protected bool IsLogical(int nAtribute)
		{
			const int mtFLAG_LOGICAL = 0x0200;
			return (nAtribute & mtFLAG_LOGICAL) != 0;
		}

		/// <summary>
		/// remove length 1 dimension  if less then 3 ranks
		/// </summary>
		/// <param name="dims">dimensions</param>
		/// <returns>dimesions with no legnth 1 dimension</returns>
		protected int[]? ToNoLength1Dimensions(int[] dims)
		{
			int[]? rcd = dims;
			if (rcd != null)
			{
				if (rcd.Length <= 2)
				{
					rcd = dims?.Where(val => val > 1).ToArray();
				}
			}
			return rcd;
		}

		/// <summary>
		/// Writes MAT-file header into <c>Stream</c>
		/// </summary>
		/// <param name="stream">The output stream</param>
		/// <param name="bRenewHeader">true: renew the header data, especailly the time stamp</param>
		protected void WriteHeader(Stream stream, bool bRenewHeader = true)
		{
			//write descriptive text
			var header = MatFileHeader.CreateHeader();
			if ((!bRenewHeader) && (_matFileHeader != null))
			{//write curren header data. when you read a mat data, want to save it by using same header, use bRenewHeader=false
				header = _matFileHeader;
			}
			byte[] oDescription = Encoding.ASCII.GetBytes(header.Description);
			byte[] oDesciptionPart = new byte[116]; //116: is the length of Description part of MAT file
			Array.Clear(oDesciptionPart);
			Buffer.BlockCopy(oDescription, 0, oDesciptionPart, 0, oDescription.Length < oDesciptionPart.Length ? oDescription.Length : oDesciptionPart.Length);
			stream.Write(oDesciptionPart);

			//write vesion
			stream.Seek(stream.Position + 8, SeekOrigin.Begin);
			byte[] Version = BitConverter.GetBytes((short)header.Version);
			stream.Write(Version);
			stream.Write(header.EndianIndicator);
		}

		/// <summary>
		/// Writes MATRIX into <c>BinaryWriter</c> stream
		/// </summary>
		/// <param name="oOutput"><c>Stream</c> stream</param>
		/// <param name="oData"><c>object</c> oData, an object based data, should be <c>DaynamicDictionary</c> or <c>string</c>,<c>int</c>...</param>
		/// <param name="strName">a name for this data, only first one need this name, otherwice should be null</param>
		protected void WriteMatrix(Stream oOutput, object? oData, string? strName = null)
		{
			//byte[] result = new byte[intArray.Length * sizeof(int)];
			//Buffer.BlockCopy(intArray, 0, result, 0, result.Length);
			if (oData != null)
			{
				//this Matrix TAG, will be complited after whole data was written. we need its size
				long posMatrixTag = oOutput.Position;   // start pos for writing Matrix Tag
				oOutput.Seek(oOutput.Position + 8, SeekOrigin.Begin);   // seek to the start position of flags

				//write data base on the data type
				switch (oData)
				{
					case DynamicDictionary oStruct: //it is a structure contains other members
						{
							//write flags with struct type
							WriteFlags(oOutput, MLArray.mxSTRUCT_CLASS);
							//write demensions
							WriteDimensions(oOutput, new int[] { 1, 1 });
							//write this struct name
							WriteName(oOutput, strName);

							//get the sub data name and its value
							string[] Names = new string[oStruct.Count];
							object?[] Values = new object[oStruct.Count];
							int nIndex = 0;
							int nMaxLengthOfName = 0;
							foreach (KeyValuePair<string, object?> oMemberData in oStruct.DataSet)
							{
								if (!string.IsNullOrEmpty(oMemberData.Key))
								{   //only the data that have name can be written
									//change the first letter to lower case. when we read the Mat file we alwasy cheng the first letter into upper case
									Names[nIndex] = char.ToLower(oMemberData.Key[0]) + oMemberData.Key.Substring(1);
									;
									if (Names[nIndex].Length > nMaxLengthOfName)
									{//keep the max length of the name
										nMaxLengthOfName = Names[nIndex].Length;
									}
									Values[nIndex] = oMemberData.Value;
									nIndex++;
								}
							}

							//for the last a zero byte
							nMaxLengthOfName++;
							//a tag of maximum length of sub name list, allways compressed (don't know why!)
							// and in this case Padding is always 0
							OSMatTag MaxLenghtOfSubNameListTag = new OSMatTag(MatDataTypes.miINT32, 4, true);
							MaxLenghtOfSubNameListTag.Write(oOutput);
							//write max length
							oOutput.Write(BitConverter.GetBytes(nMaxLengthOfName));

							//sub name list tag
							int nSubNamesSize = nMaxLengthOfName * nIndex;
							OSMatTag SubNameListTag = new OSMatTag(MatDataTypes.miINT8, nSubNamesSize, nSubNamesSize <= 4);
							SubNameListTag.Write(oOutput);
							//write all member name inside of this strurct
							byte[] SubNames = new byte[nSubNamesSize];
							Array.Clear(SubNames, 0, nSubNamesSize);
							for (int i = 0; i < nIndex; i++)
							{
								byte[] ASubName = Encoding.ASCII.GetBytes(Names[i]);
								Buffer.BlockCopy(ASubName, 0, SubNames, i * nMaxLengthOfName, ASubName.Length);
							}
							oOutput.Write(SubNames);
							if (SubNameListTag.Padding != 0)
							{
								oOutput.Seek(oOutput.Position + SubNameListTag.Padding, SeekOrigin.Begin);
							}

							//write all values for each above name
							for (int i = 0; i < nIndex; i++)
							{
								WriteMatrix(oOutput, Values[i]);
							}
						}
						break;
					case Array oArray:  //array data
						{
							if (oArray.Length > 0)
							{
								Type? oElementType = oArray.GetType().GetElementType();
								if (oElementType == typeof(object))
								{// object[]
									WriteFlags(oOutput, MLArray.mxCELL_CLASS);
									WriteDimensions(oOutput, oArray.GetDimensions());
									//write this struct name
									WriteName(oOutput, strName);
									foreach (var item in oArray)
									{
										WriteMatrix(oOutput, item);
									}
								}
								else
								{
									int nMLArrayType = MLArray.mxUNKNOWN_CLASS;
									int nMatType = MatDataTypes.miUNKNOWN;
									int[] dims = oArray.GetDimensions();
									if (dims.Length == 1)
									{//dims.length must 
										dims = new int[] { 1, dims[0] };
									}
									int nLength = dims.Aggregate(1, (a, b) => a * b);
									int nElementSize = 0;

									if (oElementType == typeof(double))
									{//double[,...]
										nMLArrayType = MLArray.mxDOUBLE_CLASS;
										nMatType = MatDataTypes.miDOUBLE;
										nElementSize = sizeof(double);
									}
									else if (oElementType == typeof(float))
									{//float[,...]
										nMLArrayType = MLArray.mxSINGLE_CLASS;
										nMatType = MatDataTypes.miSINGLE;
										nElementSize = sizeof(float);
									}
									else if (oElementType == typeof(ulong))
									{//ulong[,...]
										nMLArrayType = MLArray.mxUINT64_CLASS;
										nMatType = MatDataTypes.miUINT64;
										nElementSize = sizeof(ulong);
									}
									else if (oElementType == typeof(long))
									{//long[,...]
										nMLArrayType = MLArray.mxINT64_CLASS;
										nMatType = MatDataTypes.miINT64;
										nElementSize = sizeof(long);
									}
									else if (oElementType == typeof(uint))
									{//uint[,...]
										nMLArrayType = MLArray.mxUINT32_CLASS;
										nMatType = MatDataTypes.miUINT32;
										nElementSize = sizeof(uint);
									}
									else if (oElementType == typeof(int))
									{//int[,...], only this data was tested, used by FAT Raw data 
										nMLArrayType = MLArray.mxINT32_CLASS;
										nMatType = MatDataTypes.miINT32;
										nElementSize = sizeof(int);
									}
									else if (oElementType == typeof(ushort))
									{//ushort[,...]
										nMLArrayType = MLArray.mxUINT16_CLASS;
										nMatType = MatDataTypes.miUINT16;
										nElementSize = sizeof(ushort);
									}
									else if (oElementType == typeof(short))
									{//short[,...]
										nMLArrayType = MLArray.mxINT16_CLASS;
										nMatType = MatDataTypes.miINT16;
										nElementSize = sizeof(short);
									}
									else if (oElementType == typeof(byte))
									{//byte[,....]
										nMLArrayType = MLArray.mxUINT8_CLASS;
										nMatType = MatDataTypes.miUINT16;
										nElementSize = sizeof(byte);
									}
									else if (oElementType == typeof(sbyte))
									{//sbyte[,...]
										nMLArrayType = MLArray.mxINT8_CLASS;
										nMatType = MatDataTypes.miINT8;
										nElementSize = sizeof(sbyte);
									}
									else
									{//unsupported data, case
										Debug.Write($"Unsupport array data type {oElementType?.Name} for FAT Raw data");
									}
									if (nMatType != MatDataTypes.miUNKNOWN)
									{//can write the data
										byte[] btTemp = new byte[nLength * nElementSize];
										WriteFlags(oOutput, nMLArrayType);
										WriteDimensions(oOutput, dims);
										//write this struct name
										WriteName(oOutput, strName);
										OSMatTag ArrayDataFlag = new OSMatTag(nMatType, btTemp.Length);
										ArrayDataFlag.Write(oOutput);
										//write all data of the arry, sequence of each data is different from the memeory sequence
										GCHandle gchArray = GCHandle.Alloc(oArray, GCHandleType.Pinned);
										GCHandle gchTemp = GCHandle.Alloc(btTemp, GCHandleType.Pinned);
										IntPtr ptrArray = gchArray.AddrOfPinnedObject();
										IntPtr ptrTemp = gchTemp.AddrOfPinnedObject();
										var indices = new int[dims.Length];
										Array.Clear(indices, 0, dims.Length);
										unsafe
										{
											//fist data point of the array
											byte* pArray = (byte*)ptrArray.ToPointer();
											byte* pTemp = (byte*)ptrTemp.ToPointer();
											int nOffsetNew = 0;
											for (var i = 0; i < nLength; i++)
											{
												Buffer.MemoryCopy(pArray + nOffsetNew * nElementSize, pTemp + i * nElementSize, nElementSize, nElementSize);
												//increase indices, and get the offset in temp array
												for (int j = 0; j < indices.Length; j++)
												{
													indices[j]++;
													if (indices[j] >= dims[j])
													{
														indices[j] = 0;
													}
													else
													{
														break;
													}
												}
												//calculate offset in memory for this indices
												nOffsetNew = 0;
												for (int j = 0; j < indices.Length - 1; j++)
												{
													nOffsetNew = (nOffsetNew + indices[j]) * dims[j + 1];
												}
												nOffsetNew += indices[^1];
											}
										}
										oOutput.Write(btTemp);
										if (ArrayDataFlag.Padding > 0)
										{
											oOutput.Seek(oOutput.Position + ArrayDataFlag.Padding, SeekOrigin.Begin);
										}
									}
								}
							}
						}
						break;
					default:
						{
							int nMLArrayType = MLArray.mxUNKNOWN_CLASS;
							int nMatType = MatDataTypes.miUNKNOWN;
							byte[]? btTemp = null;
							bool bDefaultData = false;
							switch (oData)
							{
								case string strData:    // a string data, save as a UTF8 data for the string
									nMLArrayType = MLArray.mxCHAR_CLASS;
									nMatType = MatDataTypes.miUTF8;
									bDefaultData = string.IsNullOrEmpty(strData);
									btTemp = Encoding.UTF8.GetBytes(strData);
									break;
								case double dData:  // int data (tested, FAT has this data type)
									nMLArrayType = MLArray.mxDOUBLE_CLASS;
									nMatType = MatDataTypes.miDOUBLE;
									bDefaultData = dData == default(double);
									btTemp = BitConverter.GetBytes(dData);
									break;
								case float fData:   // float data, did not test
									nMLArrayType = MLArray.mxSINGLE_CLASS;
									nMatType = MatDataTypes.miSINGLE;
									bDefaultData = fData == default(float);
									btTemp = BitConverter.GetBytes(fData);
									break;
								case ulong ulData: // ulong data, did not test
									nMLArrayType = MLArray.mxUINT64_CLASS;
									nMatType = MatDataTypes.miUINT64;
									bDefaultData = ulData == default(ulong);
									btTemp = BitConverter.GetBytes(ulData);
									break;
								case long lData: // ulong data, did not test
									nMLArrayType = MLArray.mxINT64_CLASS;
									nMatType = MatDataTypes.miINT64;
									bDefaultData = lData == default(long);
									btTemp = BitConverter.GetBytes(lData);
									break;
								case uint unData: // int data, did not test
									nMLArrayType = MLArray.mxUINT32_CLASS;
									nMatType = MatDataTypes.miUINT32;
									bDefaultData = unData == default(uint);
									btTemp = BitConverter.GetBytes(unData);
									break;
								case int nData: // int data (tested, FAT has this data type)
									nMLArrayType = MLArray.mxINT32_CLASS;
									nMatType = MatDataTypes.miINT32;
									bDefaultData = nData == default(int);
									btTemp = BitConverter.GetBytes(nData);
									break;
                                case byte btData: // byte data, did not test
                                    nMLArrayType = MLArray.mxUINT8_CLASS;
                                    nMatType = MatDataTypes.miUINT8;
                                    bDefaultData = btData == default(byte);
                                    //from .NET 7, it do not allow use BitConverter.GetBytes(byte or sbyte). so use new byte[]{} directly
                                    //btTemp = BitConverter.GetBytes(btData);
                                    btTemp = new byte[] { btData };
                                    break;
                                case sbyte sbtData: // sbyte data, did not test
                                    nMLArrayType = MLArray.mxINT8_CLASS;
                                    nMatType = MatDataTypes.miINT8;
                                    bDefaultData = sbtData == default(sbyte);
                                    //from .NET 7, it do not allow use BitConverter.GetBytes(byte or sbyte). so use new byte[]{} directly
                                    //btTemp = BitConverter.GetBytes(sbtData);
                                    btTemp = new byte[] { (byte)sbtData };
                                    break;
                                default:    //unsupported data
									Debug.Write($"Unsupported data type {oData!.GetType().Name} for FAT Raw data");
									break;
							}
							if (nMatType != MatDataTypes.miUNKNOWN)    //int this case btTemp should be set
							{
								// Write flag
								WriteFlags(oOutput, nMLArrayType);
								//Write demension. dims[1] == string length, don't know it is right???
								WriteDimensions(oOutput, bDefaultData ? new int[] { 0, 0 } : new int[] { 1, 1 });
								//write this struct name
								WriteName(oOutput, strName);
								//write string as UTF8
								OSMatTag Utf8Tag = new OSMatTag(nMatType, bDefaultData ? 0 : btTemp!.Length, btTemp!.Length <= 4);
								Utf8Tag.Write(oOutput);
								if (!bDefaultData)
								{//if data is default data, don't write the data
									oOutput.Write(btTemp);
								}
								if (Utf8Tag.Padding > 0)
								{
									oOutput.Seek(oOutput.Position + Utf8Tag.Padding, SeekOrigin.Begin);
								}
							}
						}
						break;
				}
				//Matrix data size (the size does not contains this tag size (8))
				int nMatrixSize = (int)(oOutput.Position - posMatrixTag - 8);
				if (nMatrixSize >= 0)
				{
					long nCurrentPos = oOutput.Position;
					//seek to the tag pos
					oOutput.Seek(posMatrixTag, SeekOrigin.Begin);
					// write this matrix tag. 
					OSMatTag MatrixTag = new OSMatTag(MatDataTypes.miMATRIX, nMatrixSize);
					MatrixTag.Write(oOutput, posMatrixTag);
					// seek to the next data pos (include padding)
					nCurrentPos += MatrixTag.Padding;
					oOutput.Seek(nCurrentPos, SeekOrigin.Begin);
				}
			}
		}

		/// <summary>
		/// Write the flag values to current position of the <c>Stream</c> oOutput.
		/// </summary>
		/// <param name="oOutput"><c>Stream</c> oOutput</param>
		/// <param name="nArrayType">this array (a unit of MAT data file). all type definition are in <c>MLArray</c>. Ex. <c>MLArray.mxSTRUCT_CLASS</c></param>
		protected void WriteFlags(Stream oOutput, int nArrayType)
		{
			// Tag of this flags
			OSMatTag oTag = new OSMatTag(MatDataTypes.miUINT32, 2 * sizeof(int));
			oTag.Write(oOutput);
			uint[] flags = new uint[] { (uint)nArrayType, 0 };
			oOutput.Write(MemoryMarshal.AsBytes(flags.AsSpan()));
		}

		/// <summary>
		/// Write the Dimensions for the part.
		/// </summary>
		/// <param name="oOutput"><c>Stream</c> oOutput</param>
		/// <param name="dims">a dimension array</param>
		protected void WriteDimensions(Stream oOutput, int[] dims)
		{
			if ((dims != null) && (dims.Length > 0))
			{
				// Tag of this flags
				OSMatTag oTag = new OSMatTag(MatDataTypes.miINT32, dims.Length * sizeof(int));
				oTag.Write(oOutput);
				oOutput.Write(MemoryMarshal.AsBytes(dims.AsSpan()));
				if (oTag.Padding > 0)
				{
					oOutput.Seek(oOutput.Position + oTag.Padding, SeekOrigin.Begin);
				}
			}
		}

		/// <summary>
		/// Write the Name values from the data to the <c>Stream</c> oOutput.
		/// all the character in the name should be ASIC code
		/// </summary>
		/// <param name="oOutput"><c>Stream</c> oOutput</param>
		/// <param name="strName">the data name</param>
		protected void WriteName(Stream oOutput, string? strName)
		{
			if (string.IsNullOrEmpty(strName))
			{
				// Tag of this flags
				OSMatTag oTag = new OSMatTag(MatDataTypes.miINT8, 0);   //that means no name, in this case normaly the name apears out side of this structure
				oTag.Write(oOutput);
			}
			else
			{
				//make fist letter lower case. when read this the first charater will be changed to uper case in DynamicDictionary.Set()
				strName = char.ToLower(strName[0]) + strName.Substring(1);
				//charater data in byte array
				byte[] cData = Encoding.ASCII.GetBytes(strName);    //length should be strName.Length
																	// Tag of this flags
				OSMatTag oTag = new OSMatTag(MatDataTypes.miINT8, cData.Length);
				oTag.Write(oOutput);
				oOutput.Write(cData);
				if (oTag.Padding != 0)
				{//skep the padding part
					oOutput.Seek(oOutput.Position + oTag.Padding, SeekOrigin.Begin);
				}
			}
		}

		/// <summary>
		/// write a compressed data with its Tag and CRC
		/// </summary>
		/// <param name="oOutput"><c>Stream</c> oOutput</param>
		/// <param name="btData">data before compressed</param>
		/// <returns></returns>
		/// <exception cref="MatlabIOException"></exception>
		protected void WriteCompressedData(Stream oOutput, byte[]? btData)
		{
			if (btData != null)
			{
				try
				{
					using (MemoryStream msSource = new MemoryStream(btData))
					{
						using (MemoryStream msCompressed = new MemoryStream(409600000))
						{
							using (DeflateStream compressor = new DeflateStream(msCompressed, CompressionMode.Compress))
							{
								msSource.CopyTo(compressor);
							}
							byte[] btStartBytes = new byte[] { 0x78, 0x9c };
							byte[] btCompressed = msCompressed.ToArray();
							byte[] btCrc = BitConverter.GetBytes(CalCRC(btData));
							//Tag for compressed data
							OSMatTag compressedTag = new OSMatTag(MatDataTypes.miCOMPRESSED, btStartBytes.Length + btCompressed.Length + btCrc.Length);
							compressedTag.Write(oOutput);
							oOutput.Write(btStartBytes);
							oOutput.Write(btCompressed);
							oOutput.Write(btCrc);
						}
					}
				}
				catch (Exception e)
				{
					throw new MatlabIOException("Could not decompress data: " + e);
				}
			}
		}

		/// <summary>
		/// create a CRC from byte[]. it is Adler-32 CRC
		/// </summary>
		/// <param name="btData">all the byte data</param>
		/// <returns>CRC</returns>
		protected uint CalCRC(byte[]? btData)
		{
			uint crc = 0;
			if (btData != null)
			{
				uint s1 = 1, s2 = 0; // Adler-32 CRC
				foreach (byte btTemp in btData)
				{
					s1 = (s1 + btTemp) % 0xFFF1;
					s2 = (s2 + s1) % 0xFFF1;
				}
				crc = (s2 << 16) | s1;
			}
			return crc;
		}
		#endregion
		#endregion
	}
}
