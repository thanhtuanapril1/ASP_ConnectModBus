using ASP_ConnectModBus.Models;
using EasyModbus;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Xml.Linq;

namespace ASP_ConnectModBus.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		//Init Global Value--------------------
		static string _COM = "COM4";
		static int _Baudrate = 115200;
		static string _Parity = "None";
		public ModbusClient ModClient;    //Initial Modbus
		static string ModbusStatus = "Disconnected"; 



		//SUB PROGRAM------------------------
		public void Init_Modbus(string COM_Port, int Baurate, string Parity)
		{
			ModClient = new ModbusClient(COM_Port);
			ModClient.ConnectionTimeout = 100;
			ModClient.Baudrate = Baurate;
			if (Parity == "None") ModClient.Parity = System.IO.Ports.Parity.None;
			if (Parity == "Odd") ModClient.Parity = System.IO.Ports.Parity.Odd;
			if (Parity == "Even") ModClient.Parity = System.IO.Ports.Parity.Even;
			if (Parity == "Mark") ModClient.Parity = System.IO.Ports.Parity.Mark;
			if (Parity == "Space") ModClient.Parity = System.IO.Ports.Parity.Space;

		}
		
		//END SUBPROGRAM---------------------

		public IActionResult Index()
		{
			ViewBag.ModStatus = ModbusStatus;
			return View();
		}


		//Get Modbus parameter form input in View: ModbusConnection.cshtml
		public IActionResult ModbusConnection()
		{
			return View();
		}

		[HttpPost]
		public IActionResult ModbusConnection(string com, int baudrate, string parity)
		{
			_COM = com;
			_Baudrate = baudrate;
			_Parity = parity;

			Init_Modbus(_COM, _Baudrate, _Parity);
			ModClient.UnitIdentifier = 1;

			try
			{
				ModClient.Connect();
				if (ModClient.Connected == true) ModbusStatus = "Connected";
				ModClient.Disconnect();
			}
			catch (Exception ex)
			{
				ModbusStatus = ex.Message;
				ModClient.Disconnect();
			}
			return RedirectToAction("Index", "Home");
		}

		//Method 1 : Interact with controller via "Form"
		//Get button click even from View : Index.cshtml via method [HttpPost] and send back data via "TempData"
		[HttpPost]
		public IActionResult btReadHolding_Click()
		{
			try
			{
				Init_Modbus(_COM, _Baudrate, _Parity);
				ModClient.Connect();
				if (ModClient.Connected == true) ModbusStatus = "Modbus Connected";
				else
				{
					ModbusStatus = "Disconnected";
					throw new Exception();
				}

				//Get data from Modbus RTU
				int[] vals;
				for (byte i = 1; i < 4; i++)
				{
					ModClient.UnitIdentifier = i;

					vals = ModClient.ReadHoldingRegisters(0, 1);
					TempData["ModValue" + i.ToString()] = vals[0];      //Send Read value from Modbus to view: Index
				}
				ModClient.Disconnect();
			}
			catch (Exception ex)
			{
				ModbusStatus = ex.Message;
				ModClient.Disconnect();
			}
			return RedirectToAction("Index", "Home");
		}

		//Method 2 : Interact with controller via "Jquery and Ajax"
		public IActionResult GetData()
		{
			ModbusData responseData = new ModbusData();     // Initialize an anonymous object to hold the data

			try
			{
				Init_Modbus(_COM, _Baudrate, _Parity);
				ModClient.Connect();
				if (ModClient.Connected == true) ModbusStatus = "Connected_Medthod2";
				else
				{
					ModbusStatus = "Disconnected_Medthod2";
					throw new Exception();
				}
				responseData.Mod_Data = new List<int[]>();
				int[] vals;
				for (byte i = 1; i < 4; i++)
				{
					ModClient.UnitIdentifier = i;
					vals = ModClient.ReadHoldingRegisters(0, 100);
					responseData.Mod_Data.Add(vals);
				}
				
				ModClient.Disconnect();
			}
			catch (Exception ex)
			{
				ModbusStatus = ex.Message;
				ModClient.Disconnect();
			}
			responseData.Mod_Status = ModbusStatus;
			
			return Json(responseData);		// Return data as JSON
		}
	}

}