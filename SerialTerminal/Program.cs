using System;
using Gtk;
using Glade;
using System.IO.Ports;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Linq;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SerialTerminal
{
	public partial class MainClass
	{
		enum SerialLogFormat{
			TEXTO,
			CSV,
			HTML
		};
		public struct AppConfigStruct
		{
			public PortConfigStruct PortConfig;
			public string ProgramFontConfig;
			public string ConsoleFontConfig;
			public List<string> SendComands;
		};
		AppConfigStruct AppConfig;
		SerialLogFormat UserLogFormat;
		TextViewMejorado ReceiveSerialTextView;
		StreamWriter SerialLogFile, UserLogFile;
		ProgramConfigClass ProgramConfig;

		public struct PortConfigStruct
		{
			public string PortName;
			public UInt32 BaudRate;
			public UInt32 Handshaking;
		}

		public static string DumpObject(object obj)
		{
			return JsonConvert.SerializeObject(obj);
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
			if (System.Environment.OSVersion.Platform == PlatformID.Win32Windows ||
				System.Environment.OSVersion.Platform == PlatformID.Win32NT ||
				System.Environment.OSVersion.Platform == PlatformID.Win32S) {
				Gtk.Rc.Parse(string.Format("{0}/theme/gtkrc",System.IO.Directory.GetCurrentDirectory()));
			}
			#region SeteoDirectorioTrabajo
			ProgramConfig = new ProgramConfigClass();
			#endregion
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
			#region Load Config
			ProgramConfig.LoadProgramConfig(ref AppConfig);
			if(AppConfig.ConsoleFontConfig==null){
				AppConfig.ConsoleFontConfig=Gui.FontButtonConsola.FontName=ReceiveSerialTextView.OriginalTextView.PangoContext.FontDescription.ToString();
			}else{
				Gui.FontButtonConsola.FontName=AppConfig.ConsoleFontConfig;
				ReceiveSerialTextView.OriginalTextView.ModifyFont(Pango.FontDescription.FromString(Gui.FontButtonConsola.FontName));
			}
			if(AppConfig.ProgramFontConfig==null){
				AppConfig.ProgramFontConfig=Gui.FontButtonProgram.FontName=Gtk.Rc.GetStyle(Gui.MainWindow).FontDescription.ToString();
			}else{
				Gui.FontButtonProgram.FontName=AppConfig.ProgramFontConfig;
				Gtk.Rc.ParseString(string.Format(@"style ""font""
								{{
								font_name = ""{0}""
								}}
								widget_class ""*"" style ""font""
								gtk-font-name = ""{0}""", Gui.FontButtonProgram.FontName));
				Gtk.Rc.ResetStyles(Settings.Default);
			}
			#endregion
			Gui.ToolButtonConfigurar.Clicked+= Gui_ToolButtonConfigurar_Clicked;
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
			Gui.MainWindow.KeyPressEvent+= Gui_MainWindow_KeyPressEvent;
			Gui.EntrySearchBox.Changed+= Gui_EntrySearchBox_Changed;
			Gui.BtSearchForward.Clicked+= Gui_BtSearchForward_Clicked;
			Gui.BtSearchBackward.Clicked+= Gui_BtSearchBackward_Clicked;
			Gui.BtHideSearchBox.Clicked+= delegate {
				LimpiaResaltadoBusqueda();
				Gui.HbSearchBox.HideAll();
			};
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
			string[] EnumNames = Enum.GetNames(typeof(Environment.SpecialFolder));
			Array Datos = Enum.GetValues(typeof(Environment.SpecialFolder));
			for (int h = 0; h < EnumNames.Length; h++) {
				//Console.WriteLine("Def: {0}, Loc: {1}",EnumNames[h],Environment.GetFolderPath((Environment.SpecialFolder)Datos.GetValue(h)));
			}
			#region AppConfig
			Utiles.ListaPuertos(Gui.ComboSelecionaPuerto);
			int i = Utiles.GetIndexOfComboBoxByString(Gui.ComboSelecionaPuerto,AppConfig.PortConfig.PortName);
			if(i!=-1){
				Gui.ComboSelecionaPuerto.Active=i;
			}
			i = Utiles.GetIndexOfComboBoxByString(Gui.ComboBaudRate,AppConfig.PortConfig.BaudRate.ToString());
			if(i!=-1){
				Gui.ComboBaudRate.Active=i;
			}
			Gui.ComboHandShaking.Active=(int)AppConfig.PortConfig.Handshaking;
			#endregion

			#region AppLog
			if(AppConfig.SendComands==null || AppConfig.SendComands.Count==0){
				AppConfig.SendComands = new List<string>();
			} else {
				foreach(string s in AppConfig.SendComands){
					Gui.CbTextoEnviado.AppendText(s);
				}
			}

			if (!System.IO.File.Exists("SerialLog")) {
				SerialLogFile = new StreamWriter(System.IO.File.Create("SerialLog"));
			} else {
				Gtk.Application.Invoke(delegate {
					System.IO.FileStream Ifs = new System.IO.FileStream("SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
					StreamReader SerialLogReader = new StreamReader(Ifs);
					Gui.SerialTextView.Buffer.Clear();
					Utiles.AppendTextToBuffer(Gui.SerialTextView,SerialLogReader.ReadToEnd());
					SerialLogReader.Close();
					Ifs = new System.IO.FileStream("SerialLog", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
					Ifs.Seek(0, SeekOrigin.End);
					SerialLogFile = new StreamWriter(Ifs) {
						AutoFlush = true
					};
				});
			}
			#endregion
			/*
			IDictionary vars = Environment.GetEnvironmentVariables();
			foreach (DictionaryEntry e in vars) {
				Console.WriteLine("{0} : {1}", e.Key, e.Value);
			}*/
			Gui.MainWindow.ShowAll();
			Gui.HbSearchBox.HideAll();
			Gtk.Application.Run();
		}

		void Gui_ToolButtonConfigurar_Clicked (object sender, EventArgs e)
		{
			Gtk.ResponseType ret = (Gtk.ResponseType)Gui.DialogConfigApp.Run();
			if (ret == ResponseType.Ok) {
				Console.WriteLine("Letra para el programa: {0}\nLetra para la consola: {1}", Gui.FontButtonProgram.FontName, Gui.FontButtonConsola.FontName);
				if (Gtk.Rc.GetStyle(Gui.MainWindow).FontDescription.ToString() != Gui.FontButtonProgram.FontName) {
					Gtk.Rc.ParseString(string.Format(@"style ""font""
								{{
								font_name = ""{0}""
								}}
								widget_class ""*"" style ""font""
								gtk-font-name = ""{0}""", Gui.FontButtonProgram.FontName));
					Gtk.Rc.ResetStyles(Settings.Default);
					AppConfig.ProgramFontConfig = Gui.FontButtonProgram.FontName;
				}
				if (ReceiveSerialTextView.OriginalTextView.PangoContext.FontDescription.ToString() != Gui.FontButtonConsola.FontName) {
					ReceiveSerialTextView.OriginalTextView.ModifyFont(Pango.FontDescription.FromString(Gui.FontButtonConsola.FontName));
					AppConfig.ConsoleFontConfig = Gui.FontButtonConsola.FontName;
				}
			}
			Gui.DialogConfigApp.Hide();
		}

		void Gui_EntrySearchBox_Changed (object sender, EventArgs e)
		{
			Gtk.Entry entry = (Gtk.Entry)sender;
			Gtk.TextIter StartIter = ReceiveSerialTextView.OriginalTextView.Buffer.StartIter;
			Gtk.TextIter EndIter = ReceiveSerialTextView.OriginalTextView.Buffer.EndIter;
			if (entry.Text.Length == 0) {
				return;
			}
			Gtk.TextIter MatchStart;
			Gtk.TextIter MatchEnd;
			LimpiaResaltadoBusqueda();
			if (StartIter.ForwardSearch(entry.Text, TextSearchFlags.TextOnly, out MatchStart, out MatchEnd, EndIter)) {
				ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado, StartIter, EndIter);
				ReceiveSerialTextView.OriginalTextView.Buffer.ApplyTag(ReceiveSerialTextView.Resaltado, MatchStart, MatchEnd);
				if (ReceiveSerialTextView.BeginMarkLastSearch.Buffer == null) {///condicion inicial
					ReceiveSerialTextView.OriginalTextView.Buffer.AddMark(ReceiveSerialTextView.BeginMarkLastSearch, MatchStart);
				} else {
					ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.BeginMarkLastSearch, MatchStart);
				}
				if (ReceiveSerialTextView.EndMarkLastSearch.Buffer == null) {///condicion inicial
					ReceiveSerialTextView.OriginalTextView.Buffer.AddMark(ReceiveSerialTextView.EndMarkLastSearch, MatchEnd);
				} else {
					ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.EndMarkLastSearch, MatchEnd);
				}
				ReceiveSerialTextView.OriginalTextView.ScrollMarkOnscreen(ReceiveSerialTextView.BeginMarkLastSearch);
				Gui.StatusBar.Pop(0);
			} else {
				Gui.StatusBar.Pop(0);
				Gui.StatusBar.Push(0, "Sin coincidencias");
			}
		}
		void Gui_BtSearchForward_Clicked (object sender, EventArgs e)
		{
			Gtk.Entry entry = Gui.EntrySearchBox;
			if (ReceiveSerialTextView.EndMarkLastSearch.Buffer == null || ReceiveSerialTextView.BeginMarkLastSearch.Buffer == null) {///condicion inicial
				return;///no hacemos nada
			}
			Gtk.TextIter StartIter = ReceiveSerialTextView.OriginalTextView.Buffer.GetIterAtMark(ReceiveSerialTextView.EndMarkLastSearch);
			Gtk.TextIter EndIter = ReceiveSerialTextView.OriginalTextView.Buffer.EndIter;
			if (entry.Text.Length == 0) {
				ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado,StartIter,EndIter);
				return;
			}
			Gtk.TextIter MatchStart;
			Gtk.TextIter MatchEnd;
			if (StartIter.ForwardSearch(entry.Text, TextSearchFlags.TextOnly, out MatchStart, out MatchEnd, EndIter)) {
				LimpiaResaltadoBusqueda();
				ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado, StartIter, EndIter);
				ReceiveSerialTextView.OriginalTextView.Buffer.ApplyTag(ReceiveSerialTextView.Resaltado, MatchStart, MatchEnd);
				ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.BeginMarkLastSearch, MatchStart);
				ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.EndMarkLastSearch, MatchEnd);
				ReceiveSerialTextView.OriginalTextView.ScrollMarkOnscreen(ReceiveSerialTextView.BeginMarkLastSearch);
			}
		}
		void Gui_BtSearchBackward_Clicked (object sender, EventArgs e)
		{
			Gtk.Entry entry = Gui.EntrySearchBox;
			if (ReceiveSerialTextView.EndMarkLastSearch.Buffer == null || ReceiveSerialTextView.BeginMarkLastSearch.Buffer == null) {///condicion inicial
				return;///no hacemos nada
			}
			Gtk.TextIter StartIter = ReceiveSerialTextView.OriginalTextView.Buffer.GetIterAtMark(ReceiveSerialTextView.BeginMarkLastSearch);///comenzamos desde el inicio de la busqueda anterior pq buscamos pa'tras
			Gtk.TextIter EndIter = ReceiveSerialTextView.OriginalTextView.Buffer.StartIter;///El enditer es el inicio porque buscamos pa'tras
			if (entry.Text.Length == 0) {
				ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado,StartIter,EndIter);
				return;
			}
			Gtk.TextIter MatchStart;
			Gtk.TextIter MatchEnd;
			if (StartIter.BackwardSearch(entry.Text, TextSearchFlags.TextOnly, out MatchStart, out MatchEnd, EndIter)) {
				LimpiaResaltadoBusqueda();
				ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado,StartIter,EndIter);
				ReceiveSerialTextView.OriginalTextView.Buffer.ApplyTag(ReceiveSerialTextView.Resaltado,MatchStart,MatchEnd);
				ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.BeginMarkLastSearch, MatchStart);///La marca begin siempre esta antes de la marca end
				ReceiveSerialTextView.OriginalTextView.Buffer.MoveMark(ReceiveSerialTextView.EndMarkLastSearch, MatchEnd);///La marca begin siempre esta antes de la marca end
				ReceiveSerialTextView.OriginalTextView.ScrollMarkOnscreen(ReceiveSerialTextView.BeginMarkLastSearch);

			}
		}
		void LimpiaResaltadoBusqueda()
		{
			Gtk.TextIter StartIter = ReceiveSerialTextView.OriginalTextView.Buffer.StartIter;
			Gtk.TextIter EndIter = ReceiveSerialTextView.OriginalTextView.Buffer.EndIter;
			ReceiveSerialTextView.OriginalTextView.Buffer.RemoveTag(ReceiveSerialTextView.Resaltado,StartIter,EndIter);
		}
		void Gui_MainWindow_KeyPressEvent (object o, KeyPressEventArgs args)
		{
			if ( (args.Event.Key == Gdk.Key.f && (args.Event.State&Gdk.ModifierType.ControlMask)!=0) || ((args.Event.State&Gdk.ModifierType.ControlMask)!=0 && args.Event.Key == Gdk.Key.F)){
				if (Gui.HbSearchBox.Visible) {
					Gui.HbSearchBox.HideAll();
				} else {
					Gui.HbSearchBox.ShowAll();
					Gui.EntrySearchBox.GrabFocus();
				}
			}
			args.RetVal = true;			
		}

		void PonerEncabezado()
		{
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
		}
		void Gui_TbSaveLog_Toggled (object sender, EventArgs e)
		{
			if (((Gtk.ToggleToolButton)sender).Active) {
				Gui.DialogGuardarLog.ShowAll();
				int ret = Gui.DialogGuardarLog.Run();
				if (ret == (int)Gtk.ResponseType.Ok) {
					UserLogFile = new StreamWriter(Gui.EntryRutaLog.Text, !Gui.ChkBtSobreescribir.Active, Encoding.GetEncoding(1252)) {
						AutoFlush = true,
					};
					UserLogFormat = (SerialLogFormat)Gui.CbFormatoLog.Active;
					if (Gui.ChkBtHistorico.Active) {
						UserLogFile.Write(Gui.SerialTextView.Buffer.Text);
						UserLogFormat = (SerialLogFormat)(Gui.CbFormatoLog.Active = 0);
					}
					if (UserLogFormat == SerialLogFormat.CSV) {
						UserLogFile.WriteLine("\nIniciando log {0} Timestamp;SerialData;", DateTime.Now);
					} else if (UserLogFormat == SerialLogFormat.HTML) {
						if (Gui.ChkBtSobreescribir.Active) {
							///Ponemos encabezado
							PonerEncabezado();
						} else {
							UserLogFile.Close();
							FileStream fs = new FileStream(Gui.EntryRutaLog.Text, FileMode.Open, FileAccess.ReadWrite);
							if (fs.Length > 128) {
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
								UserLogFile = new StreamWriter(Gui.EntryRutaLog.Text, !Gui.ChkBtSobreescribir.Active, Encoding.GetEncoding(1252)) {
									AutoFlush = true,
								};
								UserLogFile.WriteLine(string.Format(@"<table style=""width:100%"">
							  <caption>Iniciando Log {0}</caption>
							  <tr id=""Cab"">
							    <th>Timestamp</th>
							    <th>Serial Data</th>
							  </tr>", DateTime.Now));
							} else {
								fs.Close();
								UserLogFile = new StreamWriter(Gui.EntryRutaLog.Text, !Gui.ChkBtSobreescribir.Active, Encoding.GetEncoding(1252)) {
									AutoFlush = true,
								};
								PonerEncabezado();
							}
						}
					} else {
						((Gtk.ToggleToolButton)sender).Active = false;
					}
				} else {
					if (UserLogFile != null) {
						if (UserLogFormat == SerialLogFormat.HTML) {
							UserLogFile.Write("\n</table></body></html>");
						}
						UserLogFile.Close();
						UserLogFile = null;
					}
					((Gtk.ToggleToolButton)sender).Active = false;
				}
				Gui.DialogGuardarLog.Hide();
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
				AppConfig.SendComands.Add(Gui.CbTextoEnviado.ActiveText);
			}
		}

		void Gui_ToolButtonClearLog_Clicked (object sender, EventArgs e)
		{
			MessageDialog ms = new MessageDialog(Gui.MainWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, "¿Está realmente seguro?");
			ResponseType ret = (ResponseType)ms.Run();
			if (ret == ResponseType.Yes) {
				SerialLogFile.Close();
				Gui.SerialTextView.Buffer.Clear();
				SerialLogFile = new StreamWriter(System.IO.File.Create("SerialLog"));
				AppConfig.SendComands.Clear();
				((Gtk.ListStore)Gui.CbTextoEnviado.Model).Clear();
			}
			ms.Destroy();
		}

		void Gui_ComboHandShaking_Changed (object sender, EventArgs e)
		{
			AppConfig.PortConfig.Handshaking = (UInt32)((ComboBox)sender).Active;
		}

		Gdk.Color Rojo = new Gdk.Color(255, 0, 0);
		Gdk.Color Negro = new Gdk.Color(0, 0, 0);
		void Gui_ComboBaudRate_Changed (object sender, EventArgs e)
		{
			UInt32 baudrate;
			Gtk.Entry entry =(Gtk.Entry)((ComboBox)sender).Child;
			if (UInt32.TryParse(((ComboBox)sender).ActiveText, out baudrate)) {
				AppConfig.PortConfig.BaudRate = baudrate;
				entry.ModifyText(StateType.Normal, Negro);
			} else {
				entry.ModifyText(StateType.Normal, Rojo);
			}
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
				int baudrate;
				if (int.TryParse(Gui.ComboBaudRate.ActiveText, out baudrate)) {
					sport = new SerialPort(Gui.ComboSelecionaPuerto.ActiveText, baudrate, Parity.None, 8, StopBits.One);
					sport.Handshake = (Handshake)Gui.ComboHandShaking.Active;
					AppConfig.PortConfig.PortName = Gui.ComboSelecionaPuerto.ActiveText;
				} else {
					Gui.PopupMensaje(Gui.MainWindow, MessageType.Error, "Baudrate inválido");
					sport = null;
					return;
				}
			}
			if (!sport.IsOpen) {
				try{
					sport.Open ();
				}catch(Exception ex){
					Gui.PopupMensaje(Gui.MainWindow, MessageType.Error, ex.Message);
					sport = null;
					return;
				}
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
		/// Escanea el string para buscar "palabras claves" en el log y hacer algo como en el docklight
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
				
			}
		}

		void MainWindow_delete_event_cb (object o, DeleteEventArgs args)
		{
			ProgramConfig.SaveProgramConfig(AppConfig);
			SaveCommandList("ListaComandos.conf");
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
