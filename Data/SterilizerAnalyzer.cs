using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace VisualBigData.Data
{
	public sealed class SterilizerAnalyzer
	{
		private readonly CsvConfiguration config;
		private readonly Dictionary<string, SterilizerInfo> _stations;
        //public int Station { get; private set; } = 4;
		public int Chunks { get; set; } = 36;
		public float MinPressure { get; set; } = 0.03f;
        public int DataPoint { get; private set; }
        public int MaxDataProcess { get; set; }
		public string FilePath { get; private set; }

        private static readonly SterilizerAnalyzer instance = new();
		static SterilizerAnalyzer() { }
		public static SterilizerAnalyzer Instance { get { return instance; } }
        private SterilizerAnalyzer()
        {
			
			FilePath = "20221001_pressure_PKS_all.csv";

			config = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                IgnoreBlankLines = false,
            }; //setting csv config
			_stations = new();

			//Task.Run(LoadData);
			//Task.Run(Analyze);
        }

		public IReadOnlyDictionary<string, SterilizerInfo> Stations
		{
			get
			{
				return _stations;
			}
		}

		public void LoadData()
		{
			_stations.Clear();
			DataPoint = 0;

			using var reader = new StreamReader(FilePath);
			using var csv = new CsvReader(reader, config);

			csv.Read();
			csv.ReadHeader();

			var headers = csv.HeaderRecord;
			if (headers == null)
				Console.WriteLine("No headers found in the CSV file");
			else
				foreach (var header in headers)
				{
					if (header.Contains("STERILIZER_"))
					{
						_stations[header] = new SterilizerInfo();
					}
				}

			while (csv.Read())
			{
				DataPoint++;
				foreach (var station in _stations)
				{
					var header = station.Key;
					var info = station.Value;
					(DateTime, float) data = (csv.GetField<DateTime>("Date"), csv.GetField<float>(header));
					info.PressureRecords.Add(data);
				}
			}

			Console.WriteLine($"{DataPoint} from {FilePath} has been loaded");
		}
		public void LoadDataRange(DateTime start, DateTime end)
		{
			_stations.Clear();
			DataPoint = 0;
			using var reader = new StreamReader(FilePath);
			using var csv = new CsvReader(reader, config);

			csv.Read();
			csv.ReadHeader();

			var headers = csv.HeaderRecord;
			if (headers == null)
				Console.WriteLine("No headers found in the CSV file");
			else
				foreach (var header in headers)
				{
					if (header.Contains("STERILIZER_"))
					{
						_stations[header] = new SterilizerInfo();
					}
				}
			while (csv.Read())
			{
				
				foreach (var sterilizer in _stations)
				{
					var header = sterilizer.Key;
					var sterilizerinfo = sterilizer.Value;
					(DateTime timestamp, float pressure) data = (csv.GetField<DateTime>("Date"), csv.GetField<float>(header));
					if (DateTime.Compare(data.timestamp, start) >= 0 && DateTime.Compare(data.timestamp, end) <= 0)
					{
						DataPoint++;
						sterilizerinfo.PressureRecords.Add(data);
					}
				}
			}

			Console.WriteLine($"{DataPoint} from {FilePath} has been loaded");
		}
		public void Analyze()
		{
			foreach (var station in _stations)
			{
				var info = station.Value;
				var data = info.PressureRecords;
				var chunk = new List<(DateTime ts, float press)>();
				for (int i = 0; i < data.Count; i++)
				{
					chunk.Add(data[i]);
					if (chunk.Count == Chunks)
					{
						//AnalyzeChunk(chunk, info);
						var avgpress = chunk.Average(num => { return num.press; });
						if (avgpress > MinPressure && !info.LastRunningStatus)
						{
							info.LastRunningStatus = true;
							info.CycleStarted.Add(chunk[^1].ts);
						}
						else if (avgpress < MinPressure && info.LastRunningStatus)
						{
							info.LastRunningStatus = false;
							info.CycleStopped.Add(chunk[0].ts);
							var spanTime = info.CycleStopped[^1] - info.CycleStarted[^1];
							if (spanTime.TotalMinutes < 15d)
							{
								info.CycleStopped.RemoveAt(info.CycleStopped.Count - 1);
								info.CycleStarted.RemoveAt(info.CycleStarted.Count - 1);
							}
						}
						chunk.Clear();
					}
				}

				var deltas = info.GetTimeDeltas();
				info.CalculateCycle();

				Console.WriteLine(station.Key);
				info.Print();
			}
		}
    }
}
