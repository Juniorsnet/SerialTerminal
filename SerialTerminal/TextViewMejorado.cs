using System;
using Gtk;

namespace SerialTerminal
{
	public class TextViewMejorado
	{
		public TextTag Resaltado;
		public TextTag Negrita;
		public TextTag Cursiva;
		public TextTag SubRayado;
		public TextTag ColorVerde;
		public TextTag ColorVerdeClaro;
		public Gtk.TextView OriginalTextView;
		public Gtk.TextMark BeginMarkLastSearch;
		public Gtk.TextMark EndMarkLastSearch;
		public TextViewMejorado():this(new Gtk.TextView())
		{

		}
		public TextViewMejorado(Gtk.TextView tw)
		{
			OriginalTextView = tw;
			Build();
		}

		void Build()
		{
			ColorVerdeClaro = new TextTag("VerdeClaro");
			ColorVerdeClaro.ForegroundGdk = new Gdk.Color(0, 196, 0);
			OriginalTextView.Buffer.TagTable.Add(ColorVerdeClaro);
			ColorVerde = new TextTag("Verde");
			ColorVerde.Foreground = "green";
			OriginalTextView.Buffer.TagTable.Add(ColorVerde);
			Negrita = new TextTag("Negrita");
			Negrita.Weight=Pango.Weight.Bold;
			OriginalTextView.Buffer.TagTable.Add(Negrita);
			Resaltado = new TextTag("Resaltado");
			Resaltado.Background = "white";
			Resaltado.Foreground = "black";
			OriginalTextView.Buffer.TagTable.Add(Resaltado);
			Cursiva = new TextTag("Cursiva");
			Cursiva.Style = Pango.Style.Italic;
			OriginalTextView.Buffer.TagTable.Add(Cursiva);
			SubRayado = new TextTag("SubRayado");
			SubRayado.Underline = Pango.Underline.Single;
			OriginalTextView.Buffer.TagTable.Add(SubRayado);
			OriginalTextView.ModifyFont(Pango.FontDescription.FromString("monospace 10"));
			OriginalTextView.ModifyBase(StateType.Normal,new Gdk.Color(0,0,0));
			OriginalTextView.ModifyText(StateType.Normal,new Gdk.Color(255,255,255));
			//OriginalTextView.SizeAllocated+= OriginalTextView_SizeAllocated;
			OriginalTextView.WrapMode = WrapMode.WordChar;
			setup(Gui.ScrolledWindowSerialTextView.Vadjustment);
			BeginMarkLastSearch = new TextMark(null,true);
			EndMarkLastSearch= new TextMark(null,true);
		}
		/*
		void OriginalTextView_SizeAllocated (object o, SizeAllocatedArgs args)
		{
			((Gtk.TextView)o).ScrollToIter(((Gtk.TextView)o).Buffer.EndIter, 0, true, 0, 0);
		}*/

		public void DeleteTail()
		{
			TextIter start;
			TextIter end;
			if (OriginalTextView.Buffer.CharCount > Int32.MaxValue-(1024*1024)) {
				start = OriginalTextView.Buffer.StartIter;
				end = OriginalTextView.Buffer.GetIterAtOffset(Int32.MaxValue-(1024*1024*2));
				OriginalTextView.Buffer.Delete(ref start, ref end);
			}
		}

		public void AppendTextTag (string s, params TextTag[] tag)
		{
			Gtk.TextIter ptr;
			ptr = OriginalTextView.Buffer.EndIter;
			if (tag.Length>0) {
				OriginalTextView.Buffer.InsertWithTags(ref ptr, s,tag);
			} else {
				OriginalTextView.Buffer.Insert(ref ptr, s);
			}
		}

		int RegisterScroll = 0;
		void scroll (object sender,  EventArgs ev){
			//Console.WriteLine("[scroll] Value={0}, Upper={1}, PageSize={2}",((Gtk.Adjustment)sender).Value,((Gtk.Adjustment)sender).Upper,((Gtk.Adjustment)sender).PageSize);
			((Gtk.Adjustment)sender).Value=((Gtk.Adjustment)sender).Upper-((Gtk.Adjustment)sender).PageSize;
		}

		void doAutoScroll (object sender,  EventArgs ev) {
			Gtk.Adjustment va = ((Gtk.Adjustment)sender);
			//Console.WriteLine("[doAutoScroll] Value={0}, Upper={1}, PageSize={2}",va.Value,va.Upper,va.PageSize);
			if (va.Value == va.Upper - va.PageSize) { 
				if (RegisterScroll == 0) {
					va.Changed += scroll;
					RegisterScroll++;
				}
			} else if (RegisterScroll > 0) {
				va.Changed -= scroll;
				RegisterScroll--;
			}
		}

		void setup(Gtk.Adjustment incomingAsciiVerticalAdjuster) {
			incomingAsciiVerticalAdjuster.Changed+=scroll;
			RegisterScroll++;
			incomingAsciiVerticalAdjuster.ValueChanged+=doAutoScroll;
		}
	}
}

