using System;
using System.IO;
using System.Diagnostics;

namespace SerialMonitor
{
	public partial class MainForm
	{	
		string GetMbedDriveLetter()
		{
			string retVal = String.Empty;
			DriveInfo[] drives = DriveInfo.GetDrives();
			Console.WriteLine("Detected Drives: ");
			try
			{
				for(int i = 0; i < drives.GetLength(0); i++)
				{
					if (drives[i].IsReady == true) 
					{
						if (drives[i].VolumeLabel.IndexOf("MBED") != -1)
						{
							retVal = drives[i].Name;
							break;
						}
						//Debug.WriteLine("Drive " + i + ": " + drives[i].Name + " (" + drives[i].VolumeLabel + ")");
					}
				}				
			}
			catch (Exception ex)
			{
				ExceptionHandler(ex);
			}			
			
			return retVal;
		}	
		
		double ValidateNumber(string pText)
		{
			double retVal = 0.0;
				
			try
			{	
				retVal = Double.Parse(pText);
			}
			catch (FormatException)
			{
				ExceptionHandler(new Exception("You have entered non-numeric characters"));
			}
			
			return retVal;
		}
	}
}
