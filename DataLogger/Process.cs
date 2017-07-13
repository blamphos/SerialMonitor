using System;
using System.IO;
using System.Diagnostics;
using System.Collections; 
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using ZedGraph;

namespace DataLogger
{
	public partial class MainForm : Form
	{	
		private void ProcessData(string pDataStr)
		{
			int i = 0;
			double x, y;

			//string[] buf = pDataStr.Split('\n');
			//string fileName = GetMbedDriveLetter() + @"\" + buf[0];
			string tempFileName = String.Empty;
			//Debug.WriteLine(fileName);
			
			try 
			{				
				//if (File.Exists(fileName) == false) return;
				
				//tempFileName = @"C:\Temp\" + Path.GetFileName(fileName);
				//File.Copy(fileName, tempFileName, true);
				
				//string line = File.ReadAllText(tempFileName);
				//using (StreamReader sr = File.OpenText(tempFileName)
				{
					string line = pDataStr.Replace(",out.txt", "");
					//string line = buf[0].Replace(",out.txt", "");
					//string line = String.Empty;
			        //while ((line = sr.ReadLine()) != null)
			        {
			        	i = 0;
			        	//list.Clear();		        
			        	
			        	string[] s_buf = line.Split(',');	
			        	Debug.WriteLine("Received: " + s_buf.Length.ToString() + " samples.");

			        	// Remove points from start if list "is full"
			        	list.Clear();
			        	/*if (list.Count >= s_buf.Length * 4)
			        	{
				        	if (s_buf.Length < list.Count)
				        	{
				        		list.RemoveRange(0, s_buf.Length);
				        	}			        	
			        	}*/
			        	
			        	//return;
			        	//x = list[list.Count - 1].X;
						foreach (string s in s_buf)
						{
							if (s.Length > 0)
							{
								x = i * sampleInterval * 1000.0;
								//y = Double.Parse(s) / 65535.0 * 3.3;
								y = Double.Parse(s) / 100.0;
								list.Add(x, y);
								i++;							
							}
							
							//if (i > 1000) break;
						}
						UpdataGraph();
						//AnalyzeReceivedData();
			        }
				}			
			}	
			catch (IOException) 
			{
				Debug.WriteLine(tempFileName + " not accessible");
			}
			catch (Exception ex)
			{
				ExceptionHandler(ex);
			}
		}		

		void AnalyzeReceivedData()
		{	
			//lblCount.Text = "";
			StringBuilder sb = new StringBuilder();
			double[] input = list.Select(pointPair => pointPair.Y).ToArray();
			double[] samples = new Double[input.Length];
			int decimateFactor = (int)numDecimateFactor.Value;
			
			// Calculate moving average with SimpleRunningAverage class
			//SimpleRunningAverage avg = new SimpleRunningAverage(4);			
			for(int i=0; i<input.Length; i++) 
			{
				//samples[i] = avg.Add(input[i]);
				samples[i] = input[i];
	        }				

			// Build list2 with filtered values to plot a new curve in graph pane
			int j = 0;
			list2.Clear();
			foreach (PointPair p in list)
			{	
				// Decimate result data
				if (j % decimateFactor == 0)
					list2.Add(p.X, samples[j]);
				j++;
			}						
			
			// Loop to detect a drop from measurement data
			///////////////////////////////////////////////////
			// Threshold is 0.05 V (992 / 65535 * 3.3 V = 0.04995 V)
			int thresh = 794; // 794 = 0.04 V 
  			int dropCount = 0;
  			StringBuilder sb2 = new StringBuilder();
			samples = list2.Select(pointPair => pointPair.Y).ToArray();
			double xScale = (sampleInterval * decimateFactor) * 1000.0;
			Int16 dxsum = 0;
			
			list3.Clear();
			
			// Iterate sample buffer to detect drops
  			for(int i = 1; i < samples.Length; i++)
			{
				// Calculate delta between previous and current sample
	  			dxsum += (Int16)((samples[i-1]-samples[i])*65535.0/3.3);  				
	  			//sb2.AppendLine(i.ToString() + ": " + dx.ToString());
					  			
	  			if ((i % 2) == 0)
	  			{
	  				//sb2.AppendLine(i.ToString() + ": " + dxsum.ToString());
	  				// If sum exceeds threshold -> drop detected
	  				if (dxsum < (thresh * -2))
	  				{
	  					dropCount++;
	  					list3.Add( (i-1) * xScale, samples[i-1]);
	  					i += (10 / decimateFactor); // discard next samples to avoid double detect
	  					if (i < samples.Length)
	  						list3.Add( i * xScale, 0.0);
		  				//sb2.AppendLine(i.ToString() + ": " + dxsum.ToString());
	  				}
	  				dxsum = 0;
	  			}	  			
	  			else
	  			{
	  				list3.Add( i * xScale, samples[i]);
	  			}
			}
  			Debug.WriteLine(sb2.ToString());
  			
  			// Update drop count
  			dropCount += int.Parse(lblCount.Text);
  			lblCount.Text = dropCount.ToString();
  					
			// Plot curves in graph control
			myPane.CurveList.Clear();
			myAnalCurve = myPane.AddCurve( "filtered", list2, Color.Red, SymbolType.Circle);	
			myCurve = myPane.AddCurve( "p20(in)", list, Color.Blue, SymbolType.XCross);			
			myDropCurve = myPane.AddCurve( "drops", list3, Color.Black, SymbolType.Star);	
			
			zg1.AxisChange();
			zg1.Invalidate();
			zg1.Refresh();			
		}		
	}
   class SimpleRunningAverage
   {
      int _size;
      double[] _values = null;
      int _valuesIndex = 0;
      int _valueCount = 0;
      double _sum = 0;

      public SimpleRunningAverage(int size)
      {
         System.Diagnostics.Debug.Assert(size > 0);
         _size = Math.Max(size, 1);
         _values = new double[_size];
      }

      public double Add(double newValue)
      {
         // calculate new value to add to sum by subtracting the
         // value that is replaced from the new value;
         double temp = newValue - _values[_valuesIndex];
         _values[_valuesIndex] = newValue;
         _sum += temp;

         _valuesIndex++;
         _valuesIndex %= _size;
        
         if (_valueCount < _size)
            _valueCount++;

         return _sum / _valueCount;
      }      
   } 	
}

