using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASP_ConnectModBus.Models
{
	public class ModbusData
	{
		public string Mod_Status { get; set; }
		public List<int[]> Mod_Data { get; set; } = new List<int[]>();
	}
}