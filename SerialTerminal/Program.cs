using System;
using Gtk;
using Glade;
using System.IO.Ports;
using System.IO.IsolatedStorage;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;

namespace SerialTerminal
{
	public partial class MainClass
	{
		enum SerialLogFormat{
			TEXTO,
			CSV,
			HTML
		};
		SerialLogFormat UserLogFormat;
		TextViewMejorado ReceiveSerialTextView;
		IsolatedStorageFile MyStorage;
		StreamWriter SerialLogFile, UserLogFile;
		PortConfigStruct PortConfig;
		public List<string> SendComands;

		[Serializable()]
		public struct PortConfigStruct
		{
			public string PortName;
			public UInt32 BaudRate;
			public UInt32 Handshaking;
		}

		void OneFunctionToRuleThemAll (GLib.UnhandledExceptionArgs args)
		{
			MessageDialog md = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, false, "{0}",((System.Exception)args.ExceptionObject).Message+((System.Exception)args.ExceptionObject).StackTrace+((System.Exception)args.ExceptionObject).InnerException);
			md.Run();
			md.Destroy();
			System.IO.File.WriteAllText("crashapp.log",((System.Exception)args.ExceptionObject).Message+((System.Exception)args.ExceptionObject).StackTrace+((System.Exception)args.ExceptionObject).InnerException);
			//args.ExitApplication=true;
		}
		public static void Main(string[] args)
		{
			new MainClass(args);
		}
		public MainClass(string[] args){

			if(System.Environment.OSVersion.Platform == PlatformID.Win32Windows ||
				System.Environment.OSVersion.Platform == PlatformID.Win32NT ||
				System.Environment.OSVersion.Platform == PlatformID.Win32S){
				CheckSystem.Get45PlusFromRegistry();
				if (!CheckSystem.CheckWindowsGtk()) {
					Console.WriteLine("No se ha detectado GTK Sharp");
					CheckSystem.MessageBox(System.IntPtr.Zero, "No se ha detectado GTK Sharp, este programa no funcionará sino tiene GTK Sharp instalado", "Error", 0);
				}
			}
			Gtk.Application.Init();
			GLib.ExceptionManager.UnhandledException+= OneFunctionToRuleThemAll;
			System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SerialTerminal.gui.glade"));
			Gui.InitObjects(sr.ReadToEnd());
			sr.Close();
			Gui.StatusBar.Push(0,"Listo!");
			Gui.MainWindow.Title="Serial Terminal V: " + System.Reflection.Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			ReceiveSerialTextView= new TextViewMejorado(Gui.SerialTextView);
			ReceiveSerialTextView.AppendTextTag("Serial Terminal\n",ReceiveSerialTextView.SubRayado,ReceiveSerialTextView.Cursiva);
			ReceiveSerialTextView.AppendTextTag("Esto es un terminal serial, escrito en C# usando GTK#");
			Gui.ComboSelecionaPuerto.Changed+= ComboSelecionaPuertoHandleChange;
			Gui.ToolBarOpenSerialPort.Clicked+= OpenSerialPortClicked;
			Gui.ToolBarCloseSerialPort.Clicked+= CloseSerialPortClicked;
			Gui.ComboBaudRate.Changed+= Gui_ComboBaudRate_Changed;
			Gui.ComboHandShaking.Changed+= Gui_ComboHandShaking_Changed;
			Gui.MainWindow.DeleteEvent+= MainWindow_delete_event_cb;
			Gui.ToolButtonSaveLog.Toggled += Gui_TbSaveLog_Toggled;
			Gui.ToolButtonClearLog.Clicked+= Gui_ToolButtonClearLog_Clicked;
			Gui.BtEnviarSerialPort.Clicked+= Gui_BtEnviarSerialPort_Clicked;
			Gui.CbTextoEnviado.KeyPressEvent+= Gui_CbTextoEnviado_KeyPressEvent;
			Gui.BtGuardarComando.Clicked += Gui_BtGuardarComando_Clicked;
			Gui.ActionGuardarProyecto.Activated+= (object sender, EventArgs e) => {
				Gtk.ResponseType ret = 0;
				Gtk.FileChooserDialog fch = new Gtk.FileChooserDialog("Escoja donde guardar el archivo", Gui.MainWindow, Gtk.FileChooserAction.Save, "OK", Gtk.ResponseType.Ok, "Cancelar", Gtk.ResponseType.Cancel);
				ret = (Gtk.ResponseType)fch.Run();
				if (ret == Gtk.ResponseType.Ok) {
					SaveCommandList(fch.Filename);
				}
				fch.Destroy();
			};
			Gui.ActionAbrirProyecto.Activated+= (object sender, EventArgs e) => {
				Gtk.ResponseType ret = 0;
				Gtk.FileChooserDialog fch = new Gtk.FileChooserDialog("Escoja el archivo", Gui.MainWindow, Gtk.FileChooserAction.Open, "OK", Gtk.ResponseType.Ok, "Cancelar", Gtk.ResponseType.Cancel);
				ret = (Gtk.ResponseType)fch.Run();
				if (ret == Gtk.ResponseType.Ok) {
					LoadCommandList(fch.Filename);
					RenderComandList();
				}
				fch.Destroy();
			};
			((Gtk.Entry)Gui.CbTextoEnviado.Child).Activated+= CbTextoEnviadoEntry_Activated;
			CreateComandList();
			LoadCommandList("ListaComandos.conf");
			RenderComandList();
			LastMark = ReceiveSerialTextView.OriginalTextView.Buffer.CreateMark("Escaneo", ReceiveSerialTextView.OriginalTextView.Buffer.EndIter, true);
			Gui.BtExaminarGuardarLog.Clicked+= (object sender, EventArgs e) => {
				Gtk.ResponseType ret = 0;
				Gtk.FileChooserDialog fch = new Gtk.FileChooserDialog("Escoja donde guardar el archivo", Gui.MainWindow, Gtk.FileChooserAction.Save, "OK", Gtk.ResponseType.Ok, "Cancelar", Gtk.ResponseType.Cancel);
				ret = (Gtk.ResponseType)fch.Run();
				if (ret == Gtk.ResponseType.Ok) {
					Gui.EntryRutaLog.Text=fch.Filename;
				}
				fch.Destroy();
			};
			Gui.EntryNombreComando.Activated+= (object sender, EventArgs e) => {
				Gui.BtNombreComadoOk.Click();
			};
			Gui.scrolledwindow4.MapEvent+= (object o, MapEventArgs margs) => {
				Console.WriteLine(margs.RetVal);
			};
			Gui.MainWindow.ShowAll();

			MyStorage = IsolatedStorageFile.GetStore(IsolatedStorageScope.User|IsolatedStorageScope.Assembly|IsolatedStorageScope.Domain,null,null);
			Console.WriteLine("Storage Creado, espacio disponible : {0} Mb", MyStorage.AvailableFreeSpace / 1000 * 1000);
			if(!MyStorage.DirectoryExists("Config")){
				MyStorage.CreateDirectory("Config");
			}
			#region AppConfig
			IsolatedStorageFileStream Ifs = null;
			if (!MyStorage.FileExists("Config/PortConfig")) {
				Ifs = MyStorage.CreateFile("Config/PortConfig");
			} else {
				Ifs = new IsolatedStorageFileStream("Config/PortConfig", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
			}

			StreamReader PortConfigFileReader = new StreamReader(Ifs);
			XmlSerializer ser = new XmlSerializer(typeof(PortConfigStruct));
			try{
				Utiles.ListaPuertos(Gui.ComboSelecionaPuerto);
				PortConfig = (PortConfigStruct)ser.Deserialize(PortConfigFileReader);
				int i = Utiles.GetIndexOfComboBoxByString(Gui.ComboSelecionaPuerto,PortConfig.PortName);
				if(i!=-1){
					Gui.ComboSelecionaPuerto.Active=i;
				}
				i = Utiles.GetIndexOfComboBoxByString(Gui.ComboBaudRate,PortConfig.BaudRate.ToString());
				if(i!=-1){
					Gui.ComboBaudRate.Active=i;
				}
				Gui.ComboHandShaking.Active=(int)PortConfig.Handshaking;
			}catch(Exception){
				if (Ifs.Length>0) {
					Gui.PopupMensaje(Gui.MainWindow, MessageType.Error, "Error al cargar el arhivo", "Al parecer el archivo de configuracion esta dañado");
				} else {
					Gui.PopupMensaje(Gui.MainWindow, MessageType.Error, "Error al cargar el arhivo", "Al parecer el archivo de configuracion esta vacio");
				}
			}finally{
				PortConfigFileReader.Close();
			}
			#endregion

			#region AppLog
			if (!MyStorage.FileExists("Config/SendSerial")) {
				SendComands = new List<string>();
			} else {
				try{
					ser = new XmlSerializer(typeof(List<string>));
					Ifs = new IsolatedStorageFileStream("Config/SendSerial", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
					SendComands = (List<string>)ser.Deserialize(Ifs);
					foreach(string s in SendComands){
						Gui.CbTextoEnviado.AppendText(s);
					}
					Ifs.Close();
				}catch(Exception ex){
					Console.WriteLine(ex);
					SendComands = new List<string>();
				}
			}

			if (!MyStorage.FileExists("Config/SerialLog")) {
				SerialLogFile = new StreamWriter(MyStorage.CreateFile("Config/SerialLog"));
			} else {
				Ifs = new IsolatedStorageFileStream("Config/SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
				StreamReader SerialLogReader = new StreamReader(Ifs);
				Gui.SerialTextView.Buffer.Clear();
				Utiles.AppendTextToBuffer(Gui.SerialTextView,SerialLogReader.ReadToEnd());
				SerialLogReader.Close();
				Ifs = new IsolatedStorageFileStream("Config/SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
				Ifs.Seek(0, SeekOrigin.End);
				SerialLogFile = new StreamWriter(Ifs) {
					AutoFlush = true
				};
			}
			#endregion
			Gtk.Application.Run();
		}

		void Gui_TbSaveLog_Toggled (object sender, EventArgs e)
		{
			if (((Gtk.ToggleToolButton)sender).Active) {
				Gui.DialogGuardarLog.ShowAll();
				int ret = Gui.DialogGuardarLog.Run();
				if (ret == (int)Gtk.ResponseType.Ok) {
					UserLogFile = new StreamWriter(Gui.EntryRutaLog.Text, !Gui.ChkBtSobreescribir.Active,Encoding.GetEncoding(1252)) {
						AutoFlush = true,
					};
					UserLogFormat = (SerialLogFormat)Gui.CbFormatoLog.Active;
					if (Gui.ChkBtHistorico.Active) {
						UserLogFile.Write(Gui.SerialTextView.Buffer.Text);
						UserLogFormat = (SerialLogFormat)(Gui.CbFormatoLog.Active=0);
					}
					if (UserLogFormat == SerialLogFormat.CSV) {
						UserLogFile.WriteLine("\nIniciando log {0} Timestamp;SerialData;", DateTime.Now);
					} else if (UserLogFormat == SerialLogFormat.HTML) {
						if (Gui.ChkBtSobreescribir.Active) {
							///Ponemos encabezado
							UserLogFile.WriteLine(string.Format(@"<!DOCTYPE html>
							<html>
							<head>
							<script>
							$('.search').keyup(function(){{
							 var val=$(this).val();    
							         $('table tbody tr').hide();
							         var trs=$('tabla tr').filter(function(d){{
							         return $(this).text().toLowerCase().indexOf(val)!=-1;
							         }});
							         trs.show();   
							}});
							</script>
							<style>
							table, th, td {{
							    border: 1px solid black;
							    border-collapse: collapse;
							}}
							th, td {{
							    padding: 5px;
							    text-align: left;
							}}
							tr#Cab {{
								background-color: #ccccff;
							}}
							tr#TX {{
								background-color: #CCFFFF;
							}}
							tr#RX {{
								background-color: #F0F0F0;
							}}
							</style>
							</head>
							<body>
							<input type=""text"" class=""search""/>
							<table style=""width:100%"" id=""tabla"">
							  <caption>Iniciando Log {0}</caption>
							  <tr id=""Cab"">
							    <th>Timestamp</th>
							    <th>Serial Data</th>
							  </tr>",DateTime.Now));

						} else {
							UserLogFile.Close();
							FileStream fs = new FileStream(Gui.EntryRutaLog.Text, FileMode.Open, FileAccess.ReadWrite);
							fs.Seek(-128, SeekOrigin.End);
							byte[] buf = new byte[128];
							fs.Read(buf, 0, buf.Length);
							string s = Encoding.GetEncoding(1252).GetString(buf);
							if (s.Contains("</body></html>")) {
								int index = s.IndexOf("</body></html>");
								fs.Seek(-128 + index, SeekOrigin.End);
								fs.SetLength(fs.Position);
							} else {
							}
							fs.Close();
							UserLogFile = new StreamWriter(Gui.EntryRutaLog.Text, !Gui.ChkBtSobreescribir.Active,Encoding.GetEncoding(1252)) {
								AutoFlush = true,
							};
							UserLogFile.WriteLine(string.Format(@"<table style=""width:100%"">
							  <caption>Iniciando Log {0}</caption>
							  <tr id=""Cab"">
							    <th>Timestamp</th>
							    <th>Serial Data</th>
							  </tr>", DateTime.Now));
						}
					}
				} else {
					((Gtk.ToggleToolButton)sender).Active = false;
				}
				Gui.DialogGuardarLog.Hide();
			} else {
				if (UserLogFile != null) {
					if (UserLogFormat == SerialLogFormat.HTML) {
						UserLogFile.Write("\n</table></body></html>");
					}
					UserLogFile.Close();
					UserLogFile = null;
				}
			}
		}

		void Gui_BtGuardarComando_Clicked (object sender, EventArgs e)
		{
			if (Gui.CbTextoEnviado.ActiveText.Length > 0) {
				Gtk.ResponseType ret = (Gtk.ResponseType)Gui.DialogNombreComando.Run();
				if (ret == ResponseType.Ok) {
					ComandoSerial comando = new ComandoSerial();
					if (ListaComandos.Count > 0) {
						comando.ID = ListaComandos.Max((c) => c.ID) + 1;
					} else {
						comando.ID = 1;
					}
					comando.Nombre = Gui.EntryNombreComando.Text;
					comando.Comando = Gui.CbTextoEnviado.ActiveText;
					comando.FindeLinea = (FinDeLinea)Gui.CbTipoFinLinea.Active;
					ListaComandos.Add(comando);
					RenderComandList();
					SaveCommandList("ListaComandos.conf");
				}
				Gui.DialogNombreComando.Hide();
			}
		}

		enum DockFormat{
			HEADER,
			ID,
			NAME,
			SERIAL_DATA,
			NOSE,
			TAMPOCO
		};
		bool ParseDockligthFile(string Filename)
		{
			bool ArchivoDockligth = false;
			DockFormat ParseState=DockFormat.HEADER;
			List<ComandoSerial> ListaComandosDock = new List<ComandoSerial>();
			ComandoSerial comando = new ComandoSerial();
			foreach (string linea in File.ReadLines(Filename)) {
				if (linea == "SEND") {
					ParseState = DockFormat.ID;
					continue;
				} else if (linea == "VERSION") {
					ArchivoDockligth = true;
				}
				switch (ParseState) {
					case DockFormat.ID:
						comando.ID = int.Parse(linea);
						ParseState++;
						break;
					case DockFormat.NAME:
						comando.Nombre = linea;
						ParseState++;
						break;
					case DockFormat.SERIAL_DATA:
						SoapHexBinary shb = SoapHexBinary.Parse(linea.Replace(" ", ""));
						if (shb.Value.Contains<byte>(0x0D)) {
							if (shb.Value.Contains<byte>(0x0A)) {
								comando.FindeLinea = FinDeLinea.CRLF;
							} else {
								comando.FindeLinea = FinDeLinea.CR;
							}
						} else if (shb.Value.Contains<byte>(0x0A)) {
							comando.FindeLinea = FinDeLinea.LF;
						} else {
							comando.FindeLinea = FinDeLinea.HEXA;
						}
						if (comando.FindeLinea == FinDeLinea.HEXA) {
							comando.Comando = linea.Replace(" ", "");
						} else {
							comando.Comando = Encoding.GetEncoding(1252).GetString(shb.Value).Replace("\r","").Replace("\n","");
						}
						ParseState++;
						ListaComandosDock.Add(comando);
						break;
					case DockFormat.NOSE:
						ParseState++;
						break;
					case DockFormat.TAMPOCO:
						ParseState++;
						break;
					default:
						break;
				}
			}
			if (ArchivoDockligth) {
				ListaComandos = ListaComandosDock;
				return true;
			} else {
				return false;
			}
		}
		void LoadCommandList(string Filename)
		{
			try{
				if(File.Exists(Filename)){
					ListaComandos=Newtonsoft.Json.JsonConvert.DeserializeObject<List<ComandoSerial>>(File.ReadAllText(Filename));
				}
			}catch(Newtonsoft.Json.JsonReaderException){
				Console.WriteLine("No es archivo Json, sera docklight?");
				if (!ParseDockligthFile(Filename)) {
					Gui.PopupMensaje(Gui.MainWindow,MessageType.Error,"Error al leer el archivo de comandos");
				}
			}catch(Exception ex){
				Gui.PopupMensaje(Gui.MainWindow,MessageType.Error,"Error al leer el archivo de comandos", ex.Message);
			}
		}
		void SaveCommandList(string FileName)
		{
			try {
				File.WriteAllText(FileName, Newtonsoft.Json.JsonConvert.SerializeObject(ListaComandos, Newtonsoft.Json.Formatting.Indented));
			} catch (Exception) {
				//Log.logger.Error(ex, "Error al guardar la lista de comandos");
			}
		}

		void CbTextoEnviadoEntry_Activated (object sender, EventArgs e)
		{
			Gui.BtEnviarSerialPort.Click();
		}

		[GLib.ConnectBefore]
		void Gui_CbTextoEnviado_KeyPressEvent (object o, KeyPressEventArgs args)
		{
			if ((args.Event.Key == Gdk.Key.KP_Enter)||(args.Event.Key == Gdk.Key.Return)) {
				Gui.BtEnviarSerialPort.Click();
			}
			args.RetVal = true;
		}

		void EnviarTextoSerialPort(string text, FinDeLinea FinLinea)
		{
			if ( (text != null)&& (sport!=null) && (sport.IsOpen) ) {
				lock (sport) {
					byte[] DataParaEnviar = null;
					if (FinLinea==FinDeLinea.HEXA) {
						try {
							SoapHexBinary shb = SoapHexBinary.Parse(text);
							DataParaEnviar = shb.Value;
						} catch (Exception) {
							Gui.PopupMensaje(Gui.MainWindow, Gtk.MessageType.Error, "Formato HEXA invalido");
							return;
						}
					} else {
						DataParaEnviar = Encoding.ASCII.GetBytes(text);
					}
					PrintTimeStamp(DateTime.Now, true);
					ReceiveSerialTextView.AppendTextTag(text);
					ReceiveSerialTextView.AppendTextTag("\n");
					sport.BaseStream.Write(DataParaEnviar, 0, DataParaEnviar.Length);
					WriteSerialLog(Encoding.Default.GetString(DataParaEnviar));
					if (FinLinea==FinDeLinea.CRLF) {
						sport.Write("\r\n");
						WriteSerialLog("\r\n");
					} else if (FinLinea==FinDeLinea.CR) {
						sport.Write("\r");
						WriteSerialLog("\r");
					} else if (FinLinea==FinDeLinea.LF) {
						sport.Write("\n");
						WriteSerialLog("\n");
					} else if (FinLinea==FinDeLinea.Nada) {

					}
				}
			}
		}

		void Gui_BtEnviarSerialPort_Clicked (object sender, EventArgs e)
		{
			/*
			if ( (Gui.CbTextoEnviado.ActiveText != null)&& (sport!=null) && (sport.IsOpen) ) {
				lock (sport) {
					byte[] DataParaEnviar = null;

					if (Gui.CbTipoFinLinea.Active==(int)FinDeLinea.HEXA) {
						try {
							SoapHexBinary shb = SoapHexBinary.Parse(Gui.CbTextoEnviado.ActiveText);
							DataParaEnviar = shb.Value;
						} catch (Exception) {
							Gui.PopupMensaje(Gui.MainWindow, Gtk.MessageType.Error, "Formato HEXA invalido");
							return;
						}
					} else {
						DataParaEnviar = Encoding.ASCII.GetBytes(Gui.CbTextoEnviado.ActiveText);
					}
					PrintTimeStamp(DateTime.Now, true);
					ReceiveSerialTextView.AppendTextTag(Gui.CbTextoEnviado.ActiveText);
					ReceiveSerialTextView.AppendTextTag("\n");
					sport.BaseStream.Write(DataParaEnviar, 0, DataParaEnviar.Length);
					WriteSerialLog(Encoding.Default.GetString(DataParaEnviar));
					if (Gui.CbTipoFinLinea.Active==(int)FinDeLinea.CRLF) {
						sport.Write("\r\n");
						WriteSerialLog("\r\n");
					} else if (Gui.CbTipoFinLinea.Active==(int)FinDeLinea.CR) {
						sport.Write("\r");
						WriteSerialLog("\r");
					} else if (Gui.CbTipoFinLinea.Active==(int)FinDeLinea.LF) {
						sport.Write("\n");
						WriteSerialLog("\n");
					} else if (Gui.CbTipoFinLinea.Active==(int)FinDeLinea.Nada) {

					}
				}
			}*/
			EnviarTextoSerialPort(Gui.CbTextoEnviado.ActiveText, (FinDeLinea)Gui.CbTipoFinLinea.Active);

			if (Utiles.GetIndexOfComboBoxByString(Gui.CbTextoEnviado, Gui.CbTextoEnviado.ActiveText) == -1) {
				Gui.CbTextoEnviado.AppendText(Gui.CbTextoEnviado.ActiveText);
				SendComands.Add(Gui.CbTextoEnviado.ActiveText);
			}
		}

		void Gui_ToolButtonClearLog_Clicked (object sender, EventArgs e)
		{
			MessageDialog ms = new MessageDialog(Gui.MainWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "¿Está realmente seguro?");
			ResponseType ret = (ResponseType)ms.Run();
			if (ret == ResponseType.Yes) {
				SerialLogFile.Close();
				Gui.SerialTextView.Buffer.Clear();
				SerialLogFile = new StreamWriter(MyStorage.CreateFile("Config/SerialLog"));
				SendComands.Clear();
				((Gtk.ListStore)Gui.CbTextoEnviado.Model).Clear();
			}
			ms.Destroy();
		}

		void Gui_ToolButtonSaveLog_Clicked (object sender, EventArgs e)
		{
			Gtk.FileChooserDialog fch = new Gtk.FileChooserDialog("Escoja donde guardar el archivo", Gui.MainWindow, Gtk.FileChooserAction.Save, "OK", Gtk.ResponseType.Ok, "Nah!", Gtk.ResponseType.Cancel);
			Gtk.ResponseType ret = (Gtk.ResponseType)fch.Run();
			if (ret == ResponseType.Ok) {
				SerialLogFile.Close();
				IsolatedStorageFileStream Ifs = new IsolatedStorageFileStream("Config/SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
				FileStream sw = new FileStream(fch.Filename,FileMode.Create,FileAccess.Write);
				Ifs.CopyTo(sw);
				Ifs.Close();
				sw.Close();

				Ifs = new IsolatedStorageFileStream("Config/SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
				Ifs.Seek(0, SeekOrigin.End);
				SerialLogFile = new StreamWriter(Ifs) {
					AutoFlush = true
				};

			}
			fch.Destroy();
		}

		void Gui_ComboHandShaking_Changed (object sender, EventArgs e)
		{
			PortConfig.Handshaking = (UInt32)((ComboBox)sender).Active;
		}

		void Gui_ComboBaudRate_Changed (object sender, EventArgs e)
		{
			PortConfig.BaudRate = UInt32.Parse(((ComboBox)sender).ActiveText);
		}

		void CloseSerialPortClicked (object sender, EventArgs e)
		{
			if ((sport != null) && (sport.IsOpen == true)) {
				sport.Close();
				Gui.ToolBarOpenSerialPort.Sensitive=true;
				Gui.ToolBarCloseSerialPort.Sensitive=false;
				sport=null;
				GLib.Source.Remove(TimerID);
			}
		}
		SerialPort sport;
		uint TimerID;
		void OpenSerialPortClicked (object sender, EventArgs e)
		{
			if (sport == null) {
				int baudrate = int.Parse (Gui.ComboBaudRate.ActiveText);
				sport = new SerialPort (Gui.ComboSelecionaPuerto.ActiveText, baudrate , Parity.None, 8, StopBits.One);
				sport.Handshake = (Handshake)Gui.ComboHandShaking.Active;
			}
			if (!sport.IsOpen) {
				sport.Open ();
			}
			Gui.ToolBarOpenSerialPort.Sensitive=false;
			Gui.ToolBarCloseSerialPort.Sensitive=true;
			TimerID = GLib.Timeout.Add (200, new GLib.TimeoutHandler (SerialDataReceived));
		}

		void PrintTimeStamp(DateTime Timestamp,bool Send)
		{
			if (Send) {
				ReceiveSerialTextView.AppendTextTag("\n[TX] " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + " -", ReceiveSerialTextView.SubRayado, ReceiveSerialTextView.Negrita, ReceiveSerialTextView.ColorVerdeClaro);
				if (UserLogFile != null) {
					if (UserLogFile.BaseStream.CanWrite) {
						if (UserLogFormat == SerialLogFormat.CSV) {
							UserLogFile.Write("\n[TerminalTimeStamp (TX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "];");
						} else if (UserLogFormat == SerialLogFormat.HTML) {
							UserLogFile.Write("\n<tr id=\"TX\"><td>[TS (TX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]</td>");
						} else {
							UserLogFile.Write("\n[TerminalTimeStamp (TX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]-");
						}
					}
				}
				if (SerialLogFile != null) {
					SerialLogFile.Write("\n[TerminalTimeStamp (TX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]-");
				}
			} else {
				ReceiveSerialTextView.AppendTextTag("\n[RX] " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + " -", ReceiveSerialTextView.SubRayado, ReceiveSerialTextView.Negrita, ReceiveSerialTextView.ColorVerde);
				if (UserLogFile != null) {
					if (UserLogFile.BaseStream.CanWrite) {
						if (UserLogFormat == SerialLogFormat.CSV) {
							UserLogFile.Write("\n\"[TerminalTimeStamp (RX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]\";");
						} else if (UserLogFormat == SerialLogFormat.HTML) {
							UserLogFile.Write("\n<tr id=\"RX\"><td>[TS (RX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]</td>");
						} else {
							UserLogFile.Write("\n[TerminalTimeStamp (RX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]-");
						}
					}
				}
				if (SerialLogFile != null) {
					SerialLogFile.Write("\n[TerminalTimeStamp (RX) " + Timestamp.ToString("dd-MM-yy HH:mm:ss.fff") + "]-");
				}
			}
		}
		void WriteSerialLog(string s)
		{
			if (SerialLogFile != null) {
				SerialLogFile.Write(s);
			}
			if (UserLogFile != null) {
				if (UserLogFile.BaseStream.CanWrite) {
					if (UserLogFormat == SerialLogFormat.CSV) {
						UserLogFile.Write("\""+s.Replace("\"","\"\"")+"\";");
					} else if (UserLogFormat == SerialLogFormat.HTML) {
						UserLogFile.Write("<td><pre>"+s+"</pre></td></tr>");
					} else {
						UserLogFile.Write(s);
					}
				}
			}

		}
		System.Text.StringBuilder stb = new System.Text.StringBuilder(102400);
		Gtk.TextMark LastMark;
		bool PrintCrLfs=false;
		bool SerialDataReceived()
		{
			if(Monitor.TryEnter(sport)) {
				if (sport.BytesToRead > 0) {
					DateTime Timestamp = DateTime.Now.AddMilliseconds(-50);
					ReceiveSerialTextView.DeleteTail();
					stb.Clear();
					if (sport == null || !sport.IsOpen) {
						Monitor.Exit(sport);
						return false;
					}
					PrintTimeStamp(Timestamp, false);
					//stb.AppendLine();
					while (sport.BytesToRead > 0) {
						int i = sport.ReadByte();
						if (((i < 32) || (i > 126)) && ((i != 0x0a) && (i != 0x0d))) {
							stb.AppendFormat("0x{0:X2}", i);
						} else {
							char c = (char)i;
							if (PrintCrLfs) {
								if (c == '\n') {
									stb.Append("<LF>");
								} else if (c == '\r') {
									stb.Append("<CR>");
								}
							}
							stb.Append(c);
						}
					}
					ReceiveSerialTextView.AppendTextTag(stb.ToString());
					WriteSerialLog(stb.ToString());
					Gtk.TextIter LastIter = ReceiveSerialTextView.OriginalTextView.Buffer.GetIterAtMark(LastMark);
					string LastTextBlock = ReceiveSerialTextView.OriginalTextView.Buffer.GetText(LastIter, ReceiveSerialTextView.OriginalTextView.Buffer.EndIter,false);
					ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(LastMark, ReceiveSerialTextView.OriginalTextView.Buffer.GetIterAtLine(ReceiveSerialTextView.OriginalTextView.Buffer.LineCount-1));
					EscaneaString(LastTextBlock);
				}
				Monitor.Exit(sport);
			}
			return true;
		}
		/// <summary>
		/// Escanea el string para buscar "palabras claves" en el log y hacer algo
		/// </summary>
		/// <param name="data">El string</param>
		void EscaneaString(string data)
		{
			
		}

		void ComboSelecionaPuertoHandleChange (object o, EventArgs args)
		{
			if (Gui.ComboSelecionaPuerto.ActiveText.CompareTo("Actualizar Puertos") == 0) {
				Console.WriteLine("Listando Puertos");
				foreach (string s in Enum.GetNames(typeof(Handshake))) {
					Console.WriteLine("   {0}", s);
				}
				Gui.ComboSelecionaPuerto.Changed -= ComboSelecionaPuertoHandleChange;
				((ListStore)Gui.ComboSelecionaPuerto.Model).Clear();
				Gui.ComboSelecionaPuerto.Changed += ComboSelecionaPuertoHandleChange;
				Gui.ComboSelecionaPuerto.AppendText("Actualizar Puertos");
				string[] Puertos = SerialPort.GetPortNames();
				foreach (string st in Puertos) {
					Gui.ComboSelecionaPuerto.AppendText(st);
				}
			} else {
				PortConfig.PortName = Gui.ComboSelecionaPuerto.ActiveText;
			}
		}

		void MainWindow_delete_event_cb (object o, DeleteEventArgs args)
		{
			XmlSerializer ser = new XmlSerializer(typeof(PortConfigStruct));
			IsolatedStorageFileStream Ifs = new IsolatedStorageFileStream("Config/PortConfig", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
			ser.Serialize(Ifs, PortConfig);
			Ifs.Close();
			SerialLogFile.Close();

			ser = new XmlSerializer(typeof(List<string>));
			Ifs = new IsolatedStorageFileStream("Config/SendSerial", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,MyStorage);
			ser.Serialize(Ifs, SendComands);
			Ifs.Close();

			MyStorage.Close();
			Application.Quit();
		}


	}
	static class CheckSystem
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
		[return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
		static extern bool SetDllDirectory (string lpPathName);

		[System.Runtime.InteropServices.DllImport("user32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto)]
		public static extern int MessageBox(IntPtr hWnd, String text, String caption, int options);

		public static bool CheckWindowsGtk ()
		{
			string location = null;
			Version version = null;
			Version minVersion = new Version (2, 12, 22);

			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Xamarin\GtkSharp\InstallFolder")) {
				if (key != null)
					location = key.GetValue (null) as string;
			}
			using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey (@"SOFTWARE\Xamarin\GtkSharp\Version")) {
				if (key != null)
					Version.TryParse (key.GetValue (null) as string, out version);
			}

			//TODO: check build version of GTK# dlls in GAC
			if (version == null || version < minVersion || location == null || !File.Exists (Path.Combine (location, "bin", "libgtk-win32-2.0-0.dll"))) {
				return false;
			}

			var path = Path.Combine (location, @"bin");
			try {
				if (SetDllDirectory (path)) {
					return true;
				}
			} catch (EntryPointNotFoundException) {
			}
			// this shouldn't happen unless something is weird in Windows
			return true;
		}

		public static void Get45PlusFromRegistry()
		{
			const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
			using (Microsoft.Win32.RegistryKey ndpKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry32).OpenSubKey(subkey))
			{
				if (ndpKey != null && ndpKey.GetValue("Release") != null) {
					Console.WriteLine(".NET Framework Version: " + CheckFor45PlusVersion((int) ndpKey.GetValue("Release")));
				}
				else {
					Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
				} 
			}
		}

		// Checking the version using >= will enable forward compatibility.
		private static string CheckFor45PlusVersion(int releaseKey)
		{
			if (releaseKey >= 394802)
				return "4.6.2 or later";
			if (releaseKey >= 394254) {
				return "4.6.1";
			}
			if (releaseKey >= 393295) {
				return "4.6";
			}
			if ((releaseKey >= 379893)) {
				return "4.5.2";
			}
			if ((releaseKey >= 378675)) {
				return "4.5.1";
			}
			if ((releaseKey >= 378389)) {
				return "4.5";
			}
			// This code should never execute. A non-null release key should mean
			// that 4.5 or later is installed.
			return "No 4.5 or later version detected";
		}
	}
}
