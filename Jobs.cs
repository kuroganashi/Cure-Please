using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurePlease
{
	public class Jobs : List<JobInfo>
	{
		private static Jobs instance = null;

		public static Jobs Instance
		{
			get { return instance ?? (instance = Initialize()); }
		}

		private Jobs() { }

		private static Jobs Initialize()
		{
			var jobNames = new Jobs();
			jobNames.Add(new JobInfo(1, "WAR"));
			jobNames.Add(new JobInfo(2, "MNK"));
			jobNames.Add(new JobInfo(3, "WHM"));
			jobNames.Add(new JobInfo(4, "BLM"));
			jobNames.Add(new JobInfo(5, "RDM"));
			jobNames.Add(new JobInfo(6, "THF"));
			jobNames.Add(new JobInfo(7, "PLD"));
			jobNames.Add(new JobInfo(8, "DRK"));
			jobNames.Add(new JobInfo(9, "BST"));
			jobNames.Add(new JobInfo(10, "BRD"));
			jobNames.Add(new JobInfo(11, "RNG"));
			jobNames.Add(new JobInfo(12, "SAM"));
			jobNames.Add(new JobInfo(13, "NIN"));
			jobNames.Add(new JobInfo(14, "DRG"));
			jobNames.Add(new JobInfo(15, "SMN"));
			jobNames.Add(new JobInfo(16, "BLU"));
			jobNames.Add(new JobInfo(17, "COR"));
			jobNames.Add(new JobInfo(18, "PUP"));
			jobNames.Add(new JobInfo(19, "DNC"));
			jobNames.Add(new JobInfo(20, "SCH"));
			jobNames.Add(new JobInfo(21, "GEO"));
			jobNames.Add(new JobInfo(22, "RUN"));
			return jobNames;
		}
	}

	public class JobInfo
	{
		public int Number { get; set; }
		public string Name { get; set; }

		public JobInfo(int number, string name)
		{
			Number = number;
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}
	}
}
