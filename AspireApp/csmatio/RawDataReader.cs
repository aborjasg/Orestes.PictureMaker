using csmatio.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace csmatio
{
	/// <summary>
	/// 
	/// </summary>
    public class RawDataReader
    {
		private string MatFilePath { get; set; }
		private string MatFileName { get; set; }

        private string MatFile => Path.Combine(MatFilePath, MatFileName);

		/// <summary>
		/// 
		/// </summary>
		public FATRawData RawData { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public dynamic Info { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public dynamic Params { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public dynamic Data { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="fileName"></param>
		public RawDataReader(string filePath, string fileName)
		{
			this.MatFilePath = filePath;
			this.MatFileName = fileName;

			try
			{
				byte[] data = File.ReadAllBytes(MatFile);
				//check proccess time
				DateTime dtStart = DateTime.Now;
				var mfr = new MatFileReader(data);
								
				try
				{
					//read as FAT raw data
					RawData = new FATRawData(data);
					var ccstruct = RawData.Data.Cc_struct;
					this.Info = ccstruct.Info;
					this.Params = ccstruct.Params;
					this.Data = ccstruct.Data.Cc_data;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
    }
}
