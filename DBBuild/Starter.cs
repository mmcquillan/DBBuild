// TODO: Add an ASK opton
// TODO: Add a lots of Error Handling
// TODO: Create a latch to keep two instances from running at once
// TODO: Save Errors in the DBB tables
// TODO: Add Sql/Trusted Login Support smarts
// TODO: Ensure cmd outputs values
// TODO: Ensure errors exit with a bad error code
// TODO: Throw a warning when the Catalog changes
// TODO: Add DEBUG logging
// TODO: Add restartability for RUNONCE failures

using System;

namespace DBBuild
{
    class Starter
    {
        #region The Main Program
        static void Main(string[] args)
        {

            try
            {
                // instantiate new builder engine
                BuildEngine b = new BuildEngine();

                // if we have cmd line input, then must be a file
                if (args.Length > 0)
                {

                    // check for file extension

                    // construct a string command
                    string cmd = "INCLUDETHIS " + args[0];

                    // send command to be parsed
                    b.Parse(cmd);

                }
                else
                {

                    // feedback for interactive
                    Console.WriteLine("dbb interactive (? for help)");

                    // continuous loop for input
                    while (true)
                    {

                        // input lead in
                        Console.Write("> ");

                        // capture input
                        string input = Console.ReadLine();

                        // send to be parsed
                        b.Parse(input);

                    }
                }

                // clean up any open resources
                b.CleanUp();

            }
            catch (Exception ex)
            {

                // unexpected error
                UI.Feedback("ERROR", ex.Message);
                Environment.Exit(1);

            }
        }
        #endregion
    }
}
