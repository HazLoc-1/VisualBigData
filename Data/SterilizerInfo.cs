namespace VisualBigData.Data
{
	public class SterilizerInfo
	{
		public List<DateTime> CycleStarted
		{
			get
			{
				_CycleStarted ??= new List<DateTime>();
				return _CycleStarted;
			}
			set { _CycleStarted = value; }
		}
		private List<DateTime>? _CycleStarted;

		public List<DateTime> CycleStopped
		{
			get
			{
				_CycleStopped ??= new List<DateTime>();
				return _CycleStopped;
			}
			set { _CycleStopped = value; }
		}
		private List<DateTime>? _CycleStopped;

        public List<(DateTime TimeStamp, float Pressure)> PressureRecords 
		{
			get
			{
				_PressureRecords ??= new List<(DateTime TimeStamp, float Pressure)>();
				return _PressureRecords;
			}
			set { _PressureRecords = value; }
		}
		private List<(DateTime TimeStamp, float Pressure)> _PressureRecords;
        
		public bool LastRunningStatus { get; set; }
		public double AverageCycleTime { get; set; }
		public double MaxCycleTime { get; set; }
		public double MinCycleTime { get; set; }

		private List<TimeSpan> time_deltas;
		public List<TimeSpan> GetTimeDeltas()
		{
			time_deltas ??= new List<TimeSpan>();
			for (int i = 0; i < Math.Min(CycleStarted.Count, CycleStopped.Count); i++)
			{
				time_deltas.Add(CycleStopped[i] - CycleStarted[i]);
			}

			return time_deltas;
		}
		public void CalculateCycle()
		{
			if (time_deltas.Count == 0) return;
			AverageCycleTime = time_deltas.Average(i => i.TotalMinutes);
			MaxCycleTime = time_deltas.Max().TotalMinutes;
			MinCycleTime = time_deltas.Min().TotalMinutes;
		}
		public void Print()
		{
			var x = GetTimeDeltas();
			//Console.WriteLine($"Sterilizer #{station}------------------");
			Console.WriteLine($"Cycle Started: {CycleStarted.Count}");
			Console.WriteLine($"Cycle Completed: {CycleStopped.Count}");
			if (time_deltas.Count > 0)
			{

				Console.WriteLine($"Average Cycle Time: {time_deltas.Average(i => i.TotalMinutes)} Minutes");
				Console.WriteLine($"Longest Cycle Time: {time_deltas.Max().TotalMinutes} Minutes");
				Console.WriteLine($"Shortest Cycle Time: {time_deltas.Min().TotalMinutes} Minutes");
			}
			Console.WriteLine($"---------------------------------------");
			Console.WriteLine("");
		}

	}
}
