using System;
using System.Text.RegularExpressions;
using System.IO.Ports;
using Gtk;

	public class Utiles ///Utiles Varios
	{
		public static bool ValidarPatente(string patente)
		{
			string Patente = patente.ToUpper();
			Regex check = new Regex("[A-Z]{2}[A-Z0-9]{2}[0-9]{2}");
			if (check.IsMatch(Patente)) {
				return true;
			} else {
				return false;
			}
		}
		public static string digitoVerificador(int rut)
		{
			int Digito;
			int Contador;
			int Multiplo;
			int Acumulador;
			string RutDigito;
			if (rut < 1000000) {
				return "W";
			}
			Contador = 2;
			Acumulador = 0;

			while (rut != 0) {
				Multiplo = (rut % 10) * Contador;
				Acumulador = Acumulador + Multiplo;
				rut = rut / 10;
				Contador = Contador + 1;
				if (Contador == 8) {
					Contador = 2;
				}

			}

			Digito = 11 - (Acumulador % 11);
			RutDigito = Digito.ToString().Trim();
			if (Digito == 10) {
				RutDigito = "K";
			}
			if (Digito == 11) {
				RutDigito = "0";
			}
			return (RutDigito);
		}
		public static bool ValidarStringRut(string str)
		{
			string[] RutSeparado = str.Split('-');
			if (RutSeparado.Length < 2) {
				return false;
			}
			RutSeparado[0] = RutSeparado[0].Replace(".", "");
			int RutSinDV;
			int.TryParse(RutSeparado[0], out RutSinDV);
			if (RutSinDV == 0) {
				return false;
			}
			string DVCalculado = digitoVerificador(RutSinDV);
			if (DVCalculado.CompareTo(RutSeparado[1].ToUpper()) != 0) {
				return false;
			}
			return true;
		}
		
		public static int GetIndexOfComboBoxByString(Gtk.ComboBox cb, string StringBuscado)
		{
			int IndiceComboBox=-1;
			if (StringBuscado == null) {
				Console.WriteLine("[{0}]-ERR- StringBuscado : {1}, Resultado : {2}", GetCallerMemberName(),StringBuscado,IndiceComboBox);
				return IndiceComboBox;
			}
			((Gtk.ListStore)cb.Model).Foreach((model, path, iter) =>  {
				string ComboText = (string)((Gtk.ListStore)model).GetValue(iter, 0);
				if (ComboText.ToUpper().Contains(StringBuscado.ToUpper())) {
					IndiceComboBox = path.Indices[0];
					return true;
				}
				return false;
			});
			Console.WriteLine("[{0}] StringBuscado : {1}, Resultado : {2}", GetCallerMemberName(),StringBuscado,IndiceComboBox);
			return IndiceComboBox;
		}

		public static void ListaPuertos(Gtk.ComboBox Cbp){
			Console.WriteLine("Listando Puertos");
			foreach (string s in Enum.GetNames(typeof(Handshake)))
			{
				Console.WriteLine("   {0}", s);
			}
			//PortList.Changed-= ComboSelecionaPuertoHandleChange;
			if (((ListStore)Cbp.Model) == null) {
				Cbp.Model = new ListStore(typeof(string));
			} else {
				((ListStore)Cbp.Model).Clear();
			}
			//PortList.Changed+= ComboSelecionaPuertoHandleChange;
			string [] Puertos = SerialPort.GetPortNames();
			foreach(string st in Puertos){
				((ListStore)Cbp.Model).AppendValues(st);
				Cbp.Active = 0;
			}
		}

		public static void AppendTextToBuffer(Gtk.TextView tw,string text)
		{
			Gtk.TextIter iter = tw.Buffer.EndIter;
			tw.Buffer.Insert(ref iter, text);
			tw.ScrollToIter(iter, 0, true, 0, 0);
		}

		public static string GetCallerMemberName([System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
			[System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
			[System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
		{
			return memberName;
		}
	}
