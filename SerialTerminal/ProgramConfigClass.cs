using System;
using System.IO;
using System.Xml.Serialization;

public class ProgramConfigClass
{
	public string AppDataPath;
	public DirectoryInfo WorkingDir;
	public string ProgramName;
	public ProgramConfigClass()
	{
		#region SeteoDirectorioTrabajo
		ProgramName = Path.GetFileNameWithoutExtension(System.AppDomain.CurrentDomain.FriendlyName);
		AppDataPath=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		if (System.IO.Directory.Exists(AppDataPath)) {
			WorkingDir = System.IO.Directory.CreateDirectory(AppDataPath + System.IO.Path.DirectorySeparatorChar + ProgramName);
		} else {
			WorkingDir = new System.IO.DirectoryInfo(".");
		}
		System.IO.Directory.SetCurrentDirectory(WorkingDir.FullName);
		#endregion
	}
	public bool SaveProgramConfig<T> (T obj)
	{
		XmlSerializer ser = new XmlSerializer(typeof(T));
		StreamWriter sw = new StreamWriter(obj.ToString());
		ser.Serialize(sw, obj);
		sw.Close();
		return true;
	}

	public bool LoadProgramConfig<T>(ref T obj)
	{
		XmlSerializer ser = new XmlSerializer(typeof(T));
		if (!File.Exists(obj.ToString())) {
			return false;
		}
		StreamReader sr = new StreamReader(obj.ToString());
		try{
			obj = (T)ser.Deserialize(sr);
		}catch(Exception){
			Console.WriteLine("Error al cargar el arhivo, Al parecer este no es un archivo de configuracion");
		}finally{
			sr.Close();
		}
		return true;
	}
}

