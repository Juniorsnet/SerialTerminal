using System;
	public static class Gui
	{
		public static Gtk.Action ActionAbrirProyecto;
		public static Gtk.Action ActionGuardarProyecto;
		public static Gtk.Dialog DialogGuardarLog;
		public static Gtk.Button BtGuardarLogOK;
		public static Gtk.Button BtGuardarLogCancelar;
		public static Gtk.Table table5;
		public static Gtk.Label label101;
		public static Gtk.Entry EntryRutaLog;
		public static Gtk.Button BtExaminarGuardarLog;
		public static Gtk.CheckButton ChkBtSobreescribir;
		public static Gtk.CheckButton ChkBtHistorico;
		public static Gtk.ComboBox CbFormatoLog;
		public static Gtk.CellRendererText cellrenderertext5;
		public static Gtk.Label label99;
		public static Gtk.Label label100;
		public static Gtk.Dialog DialogNombreComando;
		public static Gtk.Button BtNombreComadoOk;
		public static Gtk.Button BtNombreComandoCancelar;
		public static Gtk.Entry EntryNombreComando;
		public static Gtk.ListStore ListaBaudrates;
		public static Gtk.ListStore ListaDePuertos;
		public static Gtk.ListStore ListaEnvios;
		public static Gtk.ListStore ListaFinLineas;
		public static Gtk.ListStore ListaFormatosLog;
		public static Gtk.ListStore ListaHandShaking;
		public static Gtk.Window MainWindow;
		public static Gtk.VBox vbox1;
		public static Gtk.MenuBar menubar1;
		public static Gtk.MenuItem menuitem1;
		public static Gtk.Menu menu1;
		public static Gtk.ImageMenuItem imagemenuitem2;
		public static Gtk.ImageMenuItem imagemenuitem3;
		public static Gtk.MenuItem menuitem2;
		public static Gtk.Menu menu2;
		public static Gtk.ImageMenuItem imagemenuitem6;
		public static Gtk.ImageMenuItem imagemenuitem7;
		public static Gtk.ImageMenuItem imagemenuitem8;
		public static Gtk.ImageMenuItem imagemenuitem9;
		public static Gtk.MenuItem menuitem3;
		public static Gtk.MenuItem menuitem4;
		public static Gtk.Menu menu3;
		public static Gtk.ImageMenuItem imagemenuitem10;
		public static Gtk.Toolbar toolbar1;
		public static Gtk.ToolButton ToolBarOpenSerialPort;
		public static Gtk.ToolButton ToolBarCloseSerialPort;
		public static Gtk.ToolItem ToolBarSelectPort;
		public static Gtk.ComboBox ComboSelecionaPuerto;
		public static Gtk.CellRendererText cellrenderertext1;
		public static Gtk.ToolItem ToolBoxBaudRate;
		public static Gtk.ComboBox ComboBaudRate;
		public static Gtk.CellRendererText cellrenderertext2;
		public static Gtk.ToolItem ToolBoxHandShaking;
		public static Gtk.ComboBox ComboHandShaking;
		public static Gtk.CellRendererText cellrenderertext3;
		public static Gtk.ToggleToolButton ToolButtonSaveLog;
		public static Gtk.ToolButton ToolButtonClearLog;
		public static Gtk.HBox hbox2;
		public static Gtk.VBox vbox2;
		public static Gtk.ScrolledWindow ScrolledWindowSerialTextView;
		public static Gtk.TextView SerialTextView;
		public static Gtk.HBox hbox1;
		public static Gtk.ComboBox CbTextoEnviado;
		public static Gtk.ComboBox CbTipoFinLinea;
		public static Gtk.CellRendererText cellrenderertext4;
		public static Gtk.Button BtEnviarSerialPort;
		public static Gtk.Button BtGuardarComando;
		public static Gtk.VBox vbox5;
		public static Gtk.HBox hbox4;
		public static Gtk.Label label98;
		public static Gtk.ScrolledWindow scrolledwindow4;
		public static Gtk.Viewport ViewPortComandos;
		public static Gtk.VBox VBoxComandos;
		public static Gtk.Statusbar StatusBar;

		public static void InitObjects(string gtkb){
			Gtk.Builder gbuild = new Gtk.Builder();
			gbuild.AddFromString(gtkb);
			ActionAbrirProyecto=(Gtk.Action)gbuild.GetObject("ActionAbrirProyecto");
			ActionGuardarProyecto=(Gtk.Action)gbuild.GetObject("ActionGuardarProyecto");
			DialogGuardarLog=(Gtk.Dialog)gbuild.GetObject("DialogGuardarLog");
			BtGuardarLogOK=(Gtk.Button)gbuild.GetObject("BtGuardarLogOK");
			BtGuardarLogCancelar=(Gtk.Button)gbuild.GetObject("BtGuardarLogCancelar");
			table5=(Gtk.Table)gbuild.GetObject("table5");
			label101=(Gtk.Label)gbuild.GetObject("label101");
			EntryRutaLog=(Gtk.Entry)gbuild.GetObject("EntryRutaLog");
			BtExaminarGuardarLog=(Gtk.Button)gbuild.GetObject("BtExaminarGuardarLog");
			ChkBtSobreescribir=(Gtk.CheckButton)gbuild.GetObject("ChkBtSobreescribir");
			ChkBtHistorico=(Gtk.CheckButton)gbuild.GetObject("ChkBtHistorico");
			CbFormatoLog=(Gtk.ComboBox)gbuild.GetObject("CbFormatoLog");
			cellrenderertext5=(Gtk.CellRendererText)gbuild.GetObject("cellrenderertext5");
			label99=(Gtk.Label)gbuild.GetObject("label99");
			label100=(Gtk.Label)gbuild.GetObject("label100");
			DialogNombreComando=(Gtk.Dialog)gbuild.GetObject("DialogNombreComando");
			BtNombreComadoOk=(Gtk.Button)gbuild.GetObject("BtNombreComadoOk");
			BtNombreComandoCancelar=(Gtk.Button)gbuild.GetObject("BtNombreComandoCancelar");
			EntryNombreComando=(Gtk.Entry)gbuild.GetObject("EntryNombreComando");
			ListaBaudrates=(Gtk.ListStore)gbuild.GetObject("ListaBaudrates");
			ListaDePuertos=(Gtk.ListStore)gbuild.GetObject("ListaDePuertos");
			ListaEnvios=(Gtk.ListStore)gbuild.GetObject("ListaEnvios");
			ListaFinLineas=(Gtk.ListStore)gbuild.GetObject("ListaFinLineas");
			ListaFormatosLog=(Gtk.ListStore)gbuild.GetObject("ListaFormatosLog");
			ListaHandShaking=(Gtk.ListStore)gbuild.GetObject("ListaHandShaking");
			MainWindow=(Gtk.Window)gbuild.GetObject("MainWindow");
			vbox1=(Gtk.VBox)gbuild.GetObject("vbox1");
			menubar1=(Gtk.MenuBar)gbuild.GetObject("menubar1");
			menuitem1=(Gtk.MenuItem)gbuild.GetObject("menuitem1");
			menu1=(Gtk.Menu)gbuild.GetObject("menu1");
			imagemenuitem2=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem2");
			imagemenuitem3=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem3");
			menuitem2=(Gtk.MenuItem)gbuild.GetObject("menuitem2");
			menu2=(Gtk.Menu)gbuild.GetObject("menu2");
			imagemenuitem6=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem6");
			imagemenuitem7=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem7");
			imagemenuitem8=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem8");
			imagemenuitem9=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem9");
			menuitem3=(Gtk.MenuItem)gbuild.GetObject("menuitem3");
			menuitem4=(Gtk.MenuItem)gbuild.GetObject("menuitem4");
			menu3=(Gtk.Menu)gbuild.GetObject("menu3");
			imagemenuitem10=(Gtk.ImageMenuItem)gbuild.GetObject("imagemenuitem10");
			toolbar1=(Gtk.Toolbar)gbuild.GetObject("toolbar1");
			ToolBarOpenSerialPort=(Gtk.ToolButton)gbuild.GetObject("ToolBarOpenSerialPort");
			ToolBarCloseSerialPort=(Gtk.ToolButton)gbuild.GetObject("ToolBarCloseSerialPort");
			ToolBarSelectPort=(Gtk.ToolItem)gbuild.GetObject("ToolBarSelectPort");
			ComboSelecionaPuerto=(Gtk.ComboBox)gbuild.GetObject("ComboSelecionaPuerto");
			cellrenderertext1=(Gtk.CellRendererText)gbuild.GetObject("cellrenderertext1");
			ToolBoxBaudRate=(Gtk.ToolItem)gbuild.GetObject("ToolBoxBaudRate");
			ComboBaudRate=(Gtk.ComboBox)gbuild.GetObject("ComboBaudRate");
			cellrenderertext2=(Gtk.CellRendererText)gbuild.GetObject("cellrenderertext2");
			ToolBoxHandShaking=(Gtk.ToolItem)gbuild.GetObject("ToolBoxHandShaking");
			ComboHandShaking=(Gtk.ComboBox)gbuild.GetObject("ComboHandShaking");
			cellrenderertext3=(Gtk.CellRendererText)gbuild.GetObject("cellrenderertext3");
			ToolButtonSaveLog=(Gtk.ToggleToolButton)gbuild.GetObject("ToolButtonSaveLog");
			ToolButtonClearLog=(Gtk.ToolButton)gbuild.GetObject("ToolButtonClearLog");
			hbox2=(Gtk.HBox)gbuild.GetObject("hbox2");
			vbox2=(Gtk.VBox)gbuild.GetObject("vbox2");
			ScrolledWindowSerialTextView=(Gtk.ScrolledWindow)gbuild.GetObject("ScrolledWindowSerialTextView");
			SerialTextView=(Gtk.TextView)gbuild.GetObject("SerialTextView");
			hbox1=(Gtk.HBox)gbuild.GetObject("hbox1");
			CbTextoEnviado=(Gtk.ComboBox)gbuild.GetObject("CbTextoEnviado");
			CbTipoFinLinea=(Gtk.ComboBox)gbuild.GetObject("CbTipoFinLinea");
			cellrenderertext4=(Gtk.CellRendererText)gbuild.GetObject("cellrenderertext4");
			BtEnviarSerialPort=(Gtk.Button)gbuild.GetObject("BtEnviarSerialPort");
			BtGuardarComando=(Gtk.Button)gbuild.GetObject("BtGuardarComando");
			vbox5=(Gtk.VBox)gbuild.GetObject("vbox5");
			hbox4=(Gtk.HBox)gbuild.GetObject("hbox4");
			label98=(Gtk.Label)gbuild.GetObject("label98");
			scrolledwindow4=(Gtk.ScrolledWindow)gbuild.GetObject("scrolledwindow4");
			ViewPortComandos=(Gtk.Viewport)gbuild.GetObject("ViewPortComandos");
			VBoxComandos=(Gtk.VBox)gbuild.GetObject("VBoxComandos");
			StatusBar=(Gtk.Statusbar)gbuild.GetObject("StatusBar");
	}

	public static int PopupMensaje(Gtk.Window Parent , Gtk.MessageType Tipo, Gtk.ButtonsType WButtonType, string Texto, string secondary)
	{
		int ret;
		Gtk.MessageDialog mdc = new Gtk.MessageDialog(Parent, Gtk.DialogFlags.Modal, Tipo, WButtonType, false, "{0}", Texto);
		mdc.SecondaryText = secondary;
		ret = mdc.Run();
		mdc.Destroy();
		return ret;
	}
	public static int PopupMensaje(Gtk.Window Parent , Gtk.MessageType Tipo, string Texto, string secondary)
	{
		int ret;
		Gtk.MessageDialog mdc = new Gtk.MessageDialog(Parent, Gtk.DialogFlags.Modal, Tipo, Gtk.ButtonsType.Ok, false, "{0}", Texto);
		mdc.SecondaryText = secondary;
		ret = mdc.Run();
		mdc.Destroy();
		return ret;
	}

	public static int PopupMensaje(Gtk.Window Parent, Gtk.MessageType Tipo, Gtk.ButtonsType WButtonType, string Texto)
	{
		return PopupMensaje(Parent, Tipo, WButtonType, Texto, null);
	}

	public static int PopupMensaje(Gtk.Window Parent, Gtk.MessageType Tipo, string Texto)
	{
		return PopupMensaje(Parent, Tipo, Texto, null);
	}

	public static void OneFunctionToRuleThemAll (GLib.UnhandledExceptionArgs args)
	{
		Gtk.MessageDialog md = new Gtk.MessageDialog(null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, Gtk.ButtonsType.Close, false, "{0}\n{1}",((System.Exception)args.ExceptionObject).Message,((System.Exception)args.ExceptionObject).InnerException);
		md.Run();
		md.Destroy();
		System.IO.File.WriteAllText("crashapp.log",((System.Exception)args.ExceptionObject).Message+((System.Exception)args.ExceptionObject).StackTrace+((System.Exception)args.ExceptionObject).InnerException);
		args.ExitApplication=true;
	}

	public static void Refresh ()
	{
		while(Gtk.Application.EventsPending()){Gtk.Application.RunIteration();}
	}
}
