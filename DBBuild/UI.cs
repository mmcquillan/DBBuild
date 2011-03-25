using System;

namespace DBBuild
{
	class UI
	{

		#region Members
		private const int tabSpace = 12;
		#endregion

		#region PUBLIC Feedback
		public static void Feedback(string type, string output)
		{
			Feedback(type, output, true);
		}

		public static void Feedback(string type, string output, bool verbose)
		{

			if ( verbose )
			{

				// grab the original color
				ConsoleColor OriginalColor = Console.ForegroundColor;

				// adjust color based on type
				switch (type)
				{
					case "ERROR":
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case "SUCCESS":
						Console.ForegroundColor = ConsoleColor.Green;
						break;
					case "WARNING":
						Console.ForegroundColor = ConsoleColor.Yellow;
						break;
					default:
						Console.ForegroundColor = ConsoleColor.DarkGray;
						break;
				}

				// write out the type
				Console.Write(type.PadRight(tabSpace));

				// reset color to original
				Console.ForegroundColor = OriginalColor;

				// see if the msg is long enough to wrap
				if(output.Length <= Console.WindowWidth - tabSpace - 1)
				{
					Console.WriteLine(output);
				}
				else
				{
					Console.WriteLine(output.Substring(0, Console.WindowWidth - tabSpace - 1));
					int marker = Console.WindowWidth - tabSpace - 1;
					while(marker < output.Length)
					{
						if(output.Length <= marker + Console.WindowWidth - tabSpace - 1)
						{
							Console.WriteLine("".PadRight(tabSpace) + output.Substring(marker));
							marker = output.Length;
						}
						else
						{
							Console.WriteLine("".PadLeft(tabSpace) + output.Substring(marker, Console.WindowWidth - tabSpace - 1));
							marker = marker + Console.WindowWidth - tabSpace - 1;
						}
					}
				}

			}
		}
		#endregion

		#region PUBLIC TwoColumns
		public static void TwoColumns(string col1, string col2)
		{
			Console.Write(col1.PadRight(20));
			Console.WriteLine(col2);
		}
		#endregion

	}
}
