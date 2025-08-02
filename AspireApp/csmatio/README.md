# Read / Write MATLAB File (*.MAT)

A module for reading / writing MATLAB file.

+ Original csmatio can be find in GitHub. We only use its Tag ID definition and MatFileReader class.
+ Found a bug in csmatio. it is a 8 bytes padding problem. We fixed it. 
+ Use seek() instead of read() to move file pointer, to make it speed up.
+ Re-write MatFileReader.Inflate() to make it speed up. (orignal : 84000 mmSec, now : 891 mmSec)

## 1. FATRawData class performance.
Here has a comparation chart for reading time. (did not check on .NET7)
| **MAT file size** | **Python scipy (sec)** | csmatio ( Orignal ) (sec)  | csmatio ( modified by Gyomei ) (sec)  | .NET 6 (sec)  | FATRawData() (sec)  | ReadBytes() (sec)  |
|:----:|:----:|:----:|:----:|:----:|:----:|:----:|
| 80MB | 9 | 82.959 | 0.891 | 0.625 | 2.4974382-->1.351 | 27.0608855 |
| 242MB | 32 | null | 3.233 | 2.482 | 12.9483854-->5.078 | 151.4647643 |
| 670MB | 68 | null | 8.852 | 5.679 | 25.6902107-->13.424 | 457.0657113 |

Note: 670MB is a most biggest file found from X: driver

## 2. FATRawData class

FATRawData Class is a class for handling FAT raw data. it can read any FAT raw data file, and also can write a FAT raw data into a file by using creating a Header property and a Data property of this class.

### 2.1 Read FAT raw data
It is easy to read FAT raw data by using this class. Here is a sample code.

	try
	{
		byte[] data = File.ReadAllBytes(strMatFile);
		//read as FAT raw data
		FATRawData rawdata = new FATRawData(data);
	}
	catch (Exception e)
	{
		Console.WriteLine(e.Message);
	}


Using this class we can have a object that can access any data directly as follow. This is because we are using dynamic of C#, each property name is using the data key inside of MAT file, but make the first letter be an uppercase to avoid use of C# keyword.


	var cc_struct = rawdata.Data.Cc_struct;	//a struct contains sum data memeber
	var oSensor = rawdata.Data.Cc_struct.Info.Sensor;	//"M46888-A0"
	var energy_cal = rawdata.Data.Cc_struct.Params.Energy_cal;	// 0
	var cc_data = rawdata.Data.Cc_struct.Data.Cc_data;	//int[101, 13, 1, 24, 36]
	var elapsed_time = rawdata.Data.Cc_struct.Data.Elapsed_time;	//68.060680866241455


### 2.2 Write a Raw Data file
Write MAT file support most of the data type, but only int, string, double, int[,...] ware tested. cause we only have those data.
Here has a sample code for write a Raw data file. also how set data into Cc_struct.

	try
	{
		byte[] data = File.ReadAllBytes(strMatFile);
		//read as FAT raw data
		FATRawData rawdata = new FATRawData(data);

		// use memory stream
		byte[] bytesData = null;
		using (MemoryStream ms = new MemoryStream())
		{
			// write data with same header, and no compress
			rawdata.WriteData(ms, false, false);
			// write data with same header, and compress it
			//rawdata.WriteData(ms, false);
			bytesData = ms.ToArray();
		}
		if (bytesData != null)
		{
			File.WriteAllBytes(strOutputFile, bytesData);
		}

		//build raw data from scrach
		var rawdata2 = new FATRawData();
		//you can change header data as follow. but in most case you do not have to change
		//rawdata.Hearder.Version = 20
		//create data part
		//1. create cc_struct
		dynamic oFatData = new DynamicDictionary();
		rawdata2.Data = oFatData;
		dynamic Cc_struct = new DynamicDictionary();
		//oFatData.Set("Cc_struct", Cc_struct);
		oFatData.Cc_struct = Cc_struct;
		//create sub data of cc_struct
		dynamic Info = new DynamicDictionary();
		//Cc_struct.Set("Info", Info);
		Cc_struct.Info = Info;
		Info.Set("Sensor", "M46888-A0");
		Info.Sensor = "M46888-A0";
		//create other sub data
		dynamic oData = new DynamicDictionary();
		Cc_struct.Data = oData;
		//create a array under Cc_struct.Data
		int[] dims = new int[4] { 1, 2, 3, 4 };
		Array oArray = Array.CreateInstance(typeof(int), dims);
		oData.Cc_data = oArray;
		//set data to array (this is using our ArrayExtensions)
		oArray.SetValue(1, new int[] {0,0,0,0 });
		oArray.SetValue(2, new int[] {0,0,0,1 });

		//write out the data
		using (MemoryStream ms = new MemoryStream())
		{
			// write data without compress
			rawdata2.WriteData(ms, true, false);
			// write data with compress
			//rawdata.WriteData(ms);
			bytesData = ms.ToArray();
		}
		if (bytesData != null)
		{
			File.WriteAllBytes(strOutputFile, bytesData);
		}
	}
	catch (Exception e)
	{
		Console.WriteLine(e.Message);
	}

## 3. Notice
### 3.1 Text support for other languages: Currently only suppourt ASCII code. All the text code should be UTF8. Use Encoding class we can read those text code in UTF8. Once we did it we can support any languages that .net core supported.
### 3.2 Currently this mudule support Little-Endian only. If we want suppport Big-Endian OS ( or CPU), we have to change following code in this module.
| **Methode** | **Remark** |
|:----:|:----|
| BinaryReader.ReadInt32() | It read data in Little-Endian. May use Stream.Read(byte[],int,int) and BitConverter.ToInt32().|
| BitConverter | Here is an example for using this class.<br>byte[] btCRC = new byte[4];<br>if(buf.Read(btCRC,0,4)==4)<br>{<br>&nbsp;&nbsp;&nbsp;&nbsp;if(!BitConverter.IsLittleEndian)<br>&nbsp;&nbsp;&nbsp;&nbsp;{//Big-Endian<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Array.Reverse(btCRC);<br>&nbsp;&nbsp;&nbsp;&nbsp;}<br>&nbsp;&nbsp;&nbsp;&nbsp;CrcOfCompressedData = BitConverter.ToUInt32(btCRC);<br>} |
