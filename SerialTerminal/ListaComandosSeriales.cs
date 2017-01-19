using System;
using System.Collections.Generic;
using Gtk;

namespace SerialTerminal
{
	partial class MainClass
	{
		List<ComandoSerial> ListaComandos;

		void CreateComandList()
		{
			if (ListaComandos == null) {
				ListaComandos = new List<ComandoSerial>();
			} else {
				ListaComandos.Clear();
			}
			/*
			for (int i = 0; i < 100; i++) {
				ComandoSerial comando = new ComandoSerial();
				comando.ID = i;
				comando.Nombre = string.Format("N:{0}", i);
				comando.Comando = string.Format("AT+{0} asdf asdf asdf4 asdf5 asdf6 asdf7", i);
				comando.FindeLinea = FinDeLinea.CRLF;
				comando.Enviar = new Gtk.Button(new Gtk.Image(Stock.MediaPlay, IconSize.Button));
				comando.Eliminar = new Gtk.Button(new Gtk.Image(Stock.Remove, IconSize.Button));
				comando.Enviar.Clicked+= (sender, e) => {
					Console.WriteLine("Boton Enviar {0} cliked!",comando.ID);
				};
				comando.Eliminar.Clicked+= (sender, e) => {
					Console.WriteLine("Boton Eliminar {0} cliked!",comando.ID);
					for(int j=0;j<ListaComandos.Count;j++) {
						if(ListaComandos[j].ID==comando.ID){
							ListaComandos.Remove(ListaComandos[j]);
							RenderComandList();
							break;
						}
					}
				};
				ListaComandos.Add(comando);
			}
			*/
		}
		void RenderComandList()
		{
			Gui.VBoxComandos.Destroy();
			Gui.VBoxComandos = new VBox(false, 2);
			Gui.ViewPortComandos.Add(Gui.VBoxComandos);
			for(int h=0;h<ListaComandos.Count;h++){
				Gtk.HBox hbox = new Gtk.HBox(){Homogeneous=false,Spacing=2};
				ComandoSerial comando = ListaComandos[h];
				hbox.PackStart(new Gtk.Label(comando.ID.ToString()){Xalign=0.0f},false,true,0);
				hbox.PackStart(new Gtk.Label(comando.Nombre){Xalign=0.0f},true,true,0);
				hbox.PackStart(new Gtk.Entry(comando.Comando){IsEditable=false},false,true,0);
				hbox.PackStart(new Gtk.Label(comando.FindeLinea.ToString()){Xalign=0.5f},false,true,0);
				Gtk.Button Enviar = new Gtk.Button(new Gtk.Image(Stock.MediaPlay, IconSize.Button));
				Gtk.Button Eliminar = new Gtk.Button(new Gtk.Image(Stock.Remove, IconSize.Button));
				Enviar.Clicked+= (sender, e) => {
					Console.WriteLine("Boton Enviar {0} cliked!",comando.ID);
					EnviarTextoSerialPort(comando.Comando,comando.FindeLinea);
				};
				Eliminar.Clicked+= (sender, e) => {
					Console.WriteLine("Boton Eliminar {0} cliked!",comando.ID);
					for(int j=0;j<ListaComandos.Count;j++) {
						if(ListaComandos[j].ID==comando.ID){
							ListaComandos.Remove(ListaComandos[j]);
							RenderComandList();
							SaveCommandList("ListaComandos.conf");
							break;
						}
					}
				};
				hbox.PackStart(Enviar,false,true,0);
				hbox.PackStart(Eliminar,false,true,0);
				Gui.VBoxComandos.PackStart(hbox,false,true,0);
				ListaComandos[h] = comando;
			}
			Gui.VBoxComandos.ShowAll();
		}

		public enum FinDeLinea
		{
			Nada,
			CR,
			LF,
			CRLF,
			HEXA
		}
		public struct ComandoSerial
		{
			public Int32 ID;
			public string Nombre;
			public string Comando;
			public FinDeLinea FindeLinea;
			/*public Gtk.Button Enviar;
			public Gtk.Button Eliminar;*/
		}
	}
}
