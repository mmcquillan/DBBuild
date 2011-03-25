using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace DBBuild
{
  class BuildEngine
	{

		#region Constructor
		public BuildEngine()
		{
			mac = new Macros();
			mac.Set("$DBSERVER$", "localhost");
			mac.Set("$DBCATALOG$", "tempdb");
			mac.Set("$DBLOGIN$", "");
			mac.Set("$DBPASSWORD$", "");
			mac.Set("$VERBOSE$", "true");
			mac.Set("$DBBVERSION$", "dbo.DBBVersion");
			mac.Set("$DBBCHANGES$", "dbo.DBBChanges");
			mac.Set("$PREPENDFILE$", "true");
			mac.Set("$APPENDGO$", "true");
			mac.Set("$RUNONCESKIP$", "false");
			mac.Set("$VERSIONING$", "false");
		}
		#endregion

		#region Members
		private ArrayList files = new ArrayList();
    private ArrayList filenames = new ArrayList();
  	private Macros mac;
  	private Runner runner;
		#endregion

		#region PUBLIC Parse
		public void Parse(string cmd)
    {
      if (cmd.Trim().StartsWith("#") || cmd.Trim() == "")
      {
        // ignore this is a comment or empty
      }
      else
      {
        string name = ParseName(cmd);
        string value = ParseValue(cmd);
        if (name != "SET")
        {
          value = mac.Substitute(value);
        }
        switch (name)
        {
					case "?":
						Help();
						break;
					case "APPEND":
						Append(value);
						break;
					case "CLOSE":
						Close(value);
						break;
					case "CMD":
						Cmd(value);
						break;
					case "CONNECT":
						Connect();
						break;
					case "CREATE":
						Create(value);
						break;
					case "DISCONNECT":
						Disconnect();
						break;
					case "EXIT":
						Exit();
						break;
					case "INCLUDE":
						Include(value);
						break;
					case "INCLUDETHIS":
						IncludeThis(value);
						break;
					case "RUN":
						Run(value);
						break;
					case "RUNONCE":
						RunOnce(value);
						break;
					case "SET":
						Set(value);
						break;
					case "SHOW":
						Show();
						break;
					case "SUCCESS":
						Success(value);
						break;
					case "TEST":
						Test(value);
						break;
					case "WRITE":
						Write(value);
						break;
          default:
            UI.Feedback("ERROR", "'" + cmd + "' is not supported");
            break;
        }
      }
		}
		#endregion

		// Parsing Commands ///////////////////////////////////////////////////////
		
		#region PRIVATE ? Help
		private void Help()
		{
			Console.WriteLine("?");
			Console.WriteLine("Shows these help options.\n");

			Console.WriteLine("APPEND <file to append to>");
			Console.WriteLine("Appends a script for output.\n");

			Console.WriteLine("CLOSE <file to close>");
			Console.WriteLine("Closes a script for output.\n");

			Console.WriteLine("CMD <command to execute>");
			Console.WriteLine("Runs an external command.\n");

			Console.WriteLine("CONNECT");
			Console.WriteLine("Connects to a DB.\n");

			Console.WriteLine("CREATE <file to create>");
			Console.WriteLine("Creates a script for output.\n");

			Console.WriteLine("DISCONNECT");
			Console.WriteLine("Disconnects to a DB.\n");

			Console.WriteLine("EXIT");
			Console.WriteLine("Quits the application.\n");

			Console.WriteLine("INCLUDE <dbb file to process>");
			Console.WriteLine("Includes another dbb file for execution.\n");

			Console.WriteLine("RUN <file(s) to run>");
			Console.WriteLine("Opens script(s) to run against a DB.\n");

			Console.WriteLine("RUNONCE <file(s) to run>");
			Console.WriteLine("Opens script(s) to run once against a DB.\n");

			Console.WriteLine("SET <find> = <replace>");
			Console.WriteLine("Sets a macro variable that will be processed.\n");

			Console.WriteLine("SHOW");
			Console.WriteLine("Shows all the macros and thier values.\n");

			Console.WriteLine("SUCCESS <optional message>");
			Console.WriteLine("Outputs a confirmation of success.\n");

			Console.WriteLine("TEST <file to test>");
			Console.WriteLine("Scans a file for SQL errors and exits with failure if found.\n");

			Console.WriteLine("WRITE <file(s) to written>");
			Console.WriteLine("Opens script(s) to write into created/appended files.\n");

		}
		#endregion

		#region PRIVATE Append
		private void Append(string file)
		{
			// feedback
			UI.Feedback("APPEND", file, mac.GetTF("$VERBOSE$"));

			// open up a new stream
			StreamWriter sw = new StreamWriter(file, true);
			files.Add(sw);
			filenames.Add(file);
		}
		#endregion

		#region PRIVATE Close
		private void Close(string file)
		{
			for ( int i = 0; i < filenames.Count; i++ )
			{
				if ( filenames[i].ToString() == file )
				{
					// feedback
					UI.Feedback("CLOSE", file, mac.GetTF("$VERBOSE$"));

					StreamWriter sw = (StreamWriter) files[i];
					sw.Close();
					files.RemoveAt(i);
					filenames.RemoveAt(i);
				}
			}
		}
		#endregion

		#region PRIVATE Cmd
		private void Cmd(string cmd)
		{
			// feedback
			UI.Feedback("CMD", cmd, mac.GetTF("$VERBOSE$"));

			string file = cmd.Substring(0, cmd.IndexOf(' '));
			string arg = cmd.Substring(cmd.IndexOf(' ') + 1);
			ProcessStartInfo proc = new ProcessStartInfo();
			proc.CreateNoWindow = true;
			proc.Arguments = arg;
			proc.FileName = file;
			proc.RedirectStandardOutput = true;
			proc.RedirectStandardError = true;
			proc.RedirectStandardOutput = true;
			proc.UseShellExecute = false;
			Process p = Process.Start(proc);
			p.WaitForExit();
		}
		#endregion

		#region PRIVATE Connect
		private void Connect()
		{
			if(runner == null)
			{
				// feedback
				UI.Feedback("CONNECT", mac.Get("$DBSERVER$") + "." + mac.Get("$DBCATALOG$"), mac.GetTF("$VERBOSE$"));

				// establish a connection
				runner = new Runner(mac);

				// ensure the current DB is setup
				if(mac.GetTF("$VERSIONING$"))
				{
					runner.SetupVersioning();
				}
				
			}
		}
		#endregion

		#region PRIVATE Create
		private void Create(string file)
		{
			// feedback
			UI.Feedback("CREATE", file, mac.GetTF("$VERBOSE$"));

			StreamWriter sw = new StreamWriter(file, false);
			files.Add(sw);
			filenames.Add(file);
		}
		#endregion

		#region PRIVATE Disconnect
		private void Disconnect()
		{
			if ( runner != null )
			{
				// feedback
				UI.Feedback("DISCONNECT", mac.Get("$DBSERVER$") + "." + mac.Get("$DBCATALOG$"), mac.GetTF("$VERBOSE$"));

				// mark last version
				if ( mac.GetTF("$VERSIONING$") )
				{
					runner.VersionSucceeded();
				}

				// clean up
				runner = null;
			}
		}
		#endregion

		#region PRIVATE Exit
		private void Exit()
		{
			// clean up loose ends
			CleanUp();

			// die
      Environment.Exit(0);
		}
		#endregion

		#region PRIVATE Include
		private void Include(string file)
		{
			// feedback
			UI.Feedback("INCLUDE", file, mac.GetTF("$VERBOSE$"));

			StreamReader sr = new StreamReader(file);
			string ln = sr.ReadLine();
			while ( ln != null )
			{
				Parse(ln);
				ln = sr.ReadLine();
			}
		}
		#endregion

		#region PRIVATE IncludeThis
		private void IncludeThis(string file)
		{
			StreamReader sr = new StreamReader(file);
			string ln = sr.ReadLine();
			while ( ln != null )
			{
				Parse(ln);
				ln = sr.ReadLine();
			}
		}
		#endregion

		#region PRIVATE Run
		private void Run(string cmd)
		{

			// var to hold the currently processing file
			string currentFile = "";

			try
			{

				// get the list of files
				string[] myfiles = GetFiles(cmd);

				// loop through the files
				foreach ( string file in myfiles )
				{
					// feedback
					UI.Feedback("RUN", file, mac.GetTF("$VERBOSE$"));

					// set current file
					currentFile = file;

					// read in the file
					StreamReader sr = new StreamReader(file);

					// run var fix
					string content = mac.Substitute(sr.ReadToEnd());

					// send to DB
					runner.ExecuteSQL(content);

					// update the catalog
					runner.SetCatalog();
				}

			}
			catch ( Exception ex )
			{
				UI.Feedback("ERROR", ex.GetBaseException().Message);
				UI.Feedback("ERROR", "Processing File: '" + currentFile + "'");
				throw;
			}

		}
		#endregion

		#region PRIVATE RunOnce
		private void RunOnce(string cmd)
		{
			// var to hold the currently processing file
			string currentFile = "";

			try
			{

				// get the list of files
				string[] myfiles = GetFiles(cmd);

				// loop through the files
				foreach ( string file in myfiles )
				{

					// feedback
					UI.Feedback("RUNONCE", file, mac.GetTF("$VERBOSE$"));

					// set current file
					currentFile = file;

					// check if this file has been run
					if ( runner.ChangeCheck(file) )
					{
						try
						{
							// mark start
							runner.ChangeStart(file);

							// read in the file
							StreamReader sr = new StreamReader(file);

							// run var fix
							string content = mac.Substitute(sr.ReadToEnd());

							// send to DB
							if ( !mac.GetTF("$RUNONCESKIP$") )
							{
								runner.ExecuteSQL(content);
							}
							else
							{
								UI.Feedback("WARNING", "File was logged but did not run.");
							}

							// mark success
							runner.ChangeSucceeded(file);

							// update the catalog
							runner.SetCatalog();
						}
						catch ( Exception )
						{
							// mark fail
							runner.ChangeFailed(file);
							throw;
						}
					}
				}

			}
			catch ( Exception ex )
			{
				UI.Feedback("ERROR", ex.GetBaseException().Message);
				UI.Feedback("ERROR", "Processing File: '" + currentFile + "'");
				throw;
			}
		}
		#endregion

		#region PRIVATE Set
		private void Set(string cmd)
		{
			string[] var = cmd.Split('=');
			if(var.Length == 2)
			{
				mac.Set(var[0].Trim(), var[1].Trim());
			}
			else
			{
				mac.Set(var[0].Trim(), "");
			}
		}
		#endregion

		#region PRIVATE Show
		private void Show()
		{
			mac.Show();
		}
		#endregion

		#region PRIVATE Success
		private void Success(string msg)
		{
			// show success feedback
			UI.Feedback("SUCCESS", msg, mac.GetTF("$VERBOSE$"));
		}
		#endregion

		#region PRIVATE Test
		private void Test(string file)
		{
			// feedback
			UI.Feedback("TEST", file, mac.GetTF("$VERBOSE$"));

			int ExitCode = 0;
			TextWriter se = Console.Error;
			StreamReader sr = new StreamReader(file);
			string ln = sr.ReadLine();
			while ( ln != null )
			{
				if ( ln.StartsWith("Msg ") )
				{
					ExitCode = 1;
					se.WriteLine(ln);
				}
				ln = sr.ReadLine();
			}
			se.Close();
			Environment.Exit(ExitCode);
		}
		#endregion

		#region PRIVATE Write
		private void Write(string cmd)
		{
			// var to hold the currently processing file
			string currentFile = "";

			try
			{

				// get the list of files
				string[] myfiles = GetFiles(cmd);

				// loop through the files
				foreach ( string file in myfiles )
				{
					// feedback
					UI.Feedback("WRITE", file, mac.GetTF("$VERBOSE$"));

					// set current file
					currentFile = file;

					// read in the file
					StreamReader sr = new StreamReader(file);

					// run var fix
					string content = mac.Substitute(sr.ReadToEnd());

					// send to all open outputs
					foreach ( StreamWriter sw in files )
					{

						// optionaly out the file name
						if ( mac.GetTF("$PREPENDFILE$") )
						{
							sw.WriteLine("PRINT '" + file.Replace("'", "''") + "'");
							sw.WriteLine("GO");
							sw.WriteLine("");	
						}
						
						// output the input
						sw.WriteLine(content);
						sw.WriteLine("");

						// check for ending with go
						if(mac.GetTF("$APPENDGO$"))
						{
							sw.WriteLine("GO");
							sw.WriteLine("");
						}
					}
				}

			}
			catch ( Exception ex )
			{
				UI.Feedback("ERROR", ex.Message);
				UI.Feedback("ERROR", "Processing File: '" + currentFile + "'");
				throw;
			}
		}
		#endregion

		// Other Commands /////////////////////////////////////////////////////////

		#region PUBLIC CleanUp
		public void CleanUp()
		{
			// close out any open files
			foreach ( StreamWriter sw in files )
			{
				sw.Close();
			}

			// disconnect from the DB
			Disconnect();
		}
		#endregion

		#region PRIVATE GetFiles
		private string[] GetFiles(string file)
		{

			// needed vars
			string[] myfiles;

			// check if this is all files in a dir
			if ( file.IndexOf("*") > 0 )
			{
				// get the position of the final dir slash
				int i = file.LastIndexOf("\\") + 1;

				// get an array of files
				myfiles = Directory.GetFiles(file.Remove(i, file.Length - i), file.Substring(i, file.Length - i));
			}
			else
			{
				// an array of one
				myfiles = new string[] { file };
			}

			// return the results
			return myfiles;

		}
		#endregion

		#region PRIVATE ParseName
		private string ParseName(string cmd)
		{
			string ret;
			if ( cmd.Trim().IndexOf(' ') > 0 )
			{
				ret = cmd.Trim().Substring(0, cmd.Trim().IndexOf(' ')).ToUpper();
			}
			else
			{
				ret = cmd.Trim().ToUpper();
			}
			return ret;
		}
		#endregion

		#region PRIVATE ParseValue
		private string ParseValue(string cmd)
		{
			string ret;
			if ( cmd.Trim().IndexOf(' ') > 0 )
			{
				ret = cmd.Trim().Substring(cmd.Trim().IndexOf(' ') + 1);
			}
			else
			{
				ret = "";
			}
			return ret;
		}
		#endregion

	}
}
