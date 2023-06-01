using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using StormLibSharp;


namespace ClassicUO.Grpc
{
	internal partial class UoServiceImpl
    {
    	public byte[] ConcatByteArrays(byte[] array1, byte[] array2)
    	{
    		byte[] result = new byte[array1.Length + array2.Length];
			Array.Copy(array1, result, array1.Length);
			Array.Copy(array2, 0, result, array1.Length, array2.Length);
			return result;
    	}

    	static byte[] ConvertIntListToByteArray(List<int> intList)
	    {
	        List<byte> byteList = new List<byte>();

	        foreach (int value in intList)
	        {
	            byte[] bytes = BitConverter.GetBytes(value);
	            byteList.AddRange(bytes);
	        }

	        return byteList.ToArray();
	    }

	    static List<int> ConvertByteArrayToIntList(byte[] byteArray)
	    {
	        List<int> intList = new List<int>();

	        for (int i = 0; i < byteArray.Length; i += sizeof(int))
	        {
	            int value = BitConverter.ToInt32(byteArray, i);
	            intList.Add(value);
	        }

	        return intList;
	    }

    	public void CreateMpqArchive(string mpqArchiveName)
        {
            using (MpqArchive archive = MpqArchive.CreateNew(mpqArchiveName, MpqArchiveVersion.Version4))
            {   
                Console.WriteLine("MpqArchive is created");
            }
        }

		public void WrtieToMpqArchive(string mpqArchiveName, string fileName, byte[] grpcArr)
		{
			//Console.WriteLine("WrtieToMpqArchive()");
			//Console.WriteLine("fileName: {0}", fileName);

			uint arr_size = (uint) grpcArr.Length;
			//Console.WriteLine("arr_size: {0}, ", arr_size);

			if (arr_size == 0)
			{
				return;
			}

		    using (MpqArchive archive = new MpqArchive(mpqArchiveName, FileAccess.ReadWrite))
		    {
		        //Console.WriteLine("MpqArchive is opened");

		        using (MpqFileStream fs = archive.CreateFile(fileName, arr_size))
		        {
		            var arr = new List<byte>();
		            for (int i = 0; i < arr_size; i++) 
		            {
		                arr.Add((byte) grpcArr[i]);
		            }

		            byte[] buffer = arr.ToArray();
		            fs.Write(arr.ToArray(), 0, (int) arr_size);
		        }
		    }
		}
	}
}


namespace ClassicUO.Grpc
{
	internal partial class UoServiceReplayImpl
    {
	    static List<int> ConvertByteArrayToIntList(byte[] byteArray)
	    {
	        List<int> intList = new List<int>();

	        for (int i = 0; i < byteArray.Length; i += sizeof(int))
	        {
	            int value = BitConverter.ToInt32(byteArray, i);
	            intList.Add(value);
	        }

	        return intList;
	    }

		public byte[] ReadFromMpqArchive(string mpqArchiveName, string fileName)
        {
        	using (MpqArchive archive = new MpqArchive(mpqArchiveName, FileAccess.Read))
            {
            	Console.WriteLine("MpqArchive is opened");
            	Console.WriteLine("fileName: {0}", fileName);

                MpqArchiveVerificationResult archive_verify_result = archive.VerifyArchive();
                //Console.WriteLine("archive_verify_result: {0}", archive_verify_result);

                MpqFileVerificationResults file_verify_result = archive.VerifyFile(fileName);
                //Console.WriteLine("file_verify_result: {0}", file_verify_result);

                if (file_verify_result == MpqFileVerificationResults.Error)
                {	
                	Console.WriteLine("file_verify_result == MpqFileVerificationResults.Error");
                	Console.WriteLine("\n");
                	return null;
                }

                Console.WriteLine("\n");

                using (MpqFileStream fs = archive.OpenFile(fileName))
                {
                    //Console.WriteLine("fs.Length: {0}", fs.Length);

                	byte[] arr_message = new byte[1024];

                	int iter_num = 0;
                	if (fs.Length < 1024) 
                	{
                		//Console.WriteLine("fs.Length < 1024");
                		arr_message = new byte[fs.Length];
                        fs.Read(arr_message, 0, (int) fs.Length);
                    }
                    else 
                    {
                    	//Console.WriteLine("fs.Length >= 1024");
                    	iter_num = (int) fs.Length / (int) 1024;
                    	//Console.WriteLine("iter_num: {0}: ", iter_num);

                    	byte[] arr;
	                    for (int i = 0; i < iter_num; i++) 
	                    {
	                    	if (i == 0) 
	                    	{
	                    		fs.Read(arr_message, 0, 1024);
	                    	}
	                    	else
	                    	{
	                    		arr = new byte[1024];
	                    		fs.Read(arr, 0, 1024);
	                    		arr_message = arr_message.Concat(arr).ToArray();
	                    	}
	                    }

	                    int remaining_num = (int) fs.Length - 1024 * iter_num;
	                    //Console.WriteLine("remaining_num: {0}, ", remaining_num);

                    	arr = new byte[remaining_num];
                        fs.Read(arr, 0, remaining_num);

                        arr_message = arr_message.Concat(arr).ToArray();
                    }

                    return arr_message;
                }
            }
        }
	}
}