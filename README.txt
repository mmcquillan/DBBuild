-----------------------------------------------------------------------------
The Database Build Tool                                           dbbuild.exe
-----------------------------------------------------------------------------

Maintained @ https://github.com/mmcquillan/DBBuild

License: MIT License


-- Description --------------------------------------------------------------
There are lots of great applications to script out and find differences in
SQL Server schemas. What I think is missing is a flexible tool to reassemble
your scripts and deploy them easily. This tool provides a means of combining
the scripts into a single SQL file or have them run against a database in an
order you specify. There is also an option for keeping track of which scripts
have been applied to a database to help deploy schema changes in a much
easier way.



-- Build Models -------------------------------------------------------------

One Big SQL Script Model:

	The dbb application is very good at concatenating many SQL scripts
	into one big SQL script. You can use a combination of specifying
	each individual script or a directory of sql scripts (they should
	process in alphabetical order). This is useful if you want to compile
	a script and distribute it without need for this application. The
	main commands for this model are APPEND, CREATE, WRITE and CLOSE.


Run Now Model:

	The dbb application can use the same philosophy as above to combine
	individual scripts or a directory of scripts, but will run them
	against a live DB instead of compiling them into another script.
	The advantage of this is to instantly know if the scripts work. The
	main disadvantage is that this is not distributable. You would have
	to run this set of commands through the dbb application against each
	target DB. The main commands for this model are CONNECT, RUN,
	RUNONCE and DISCONNECT.



-- Executable Usage ---------------------------------------------------------
The dbb.exe application can be launched and worked with in interactive mode
but it is much more useful to launch it with a file that contains commands
for it to execute. Say you want an easy way to create your [MusicCollection]
database schema. You might create a dbb file that will act as a recipe of how
to run the scripts. In this case, the command would look like this (you
can name the file any way you want, so long as the contents are in a text
format):

dbb.exe myfile.txt


The contents of the file might look like this:

~~~begin your file~~~

# environment settings
SET $DBSERVER$ = localhost
SET $DBCATALOG$ = MusicCollection
SET $ROOT_DIR$ = c:\db\music

# connect to the db
CONNECT

# run all the necessary scripts
RUN $ROOT_DIR$\Tables\*.sql
RUN $ROOT_DIR$\Triggers\*.sql
RUN $ROOT_DIR$\Application Data\*.sql
RUN $ROOT_DIR$\Foreign Keys\*.sql
RUN $ROOT_DIR$\Functions\*.sql
RUN $ROOT_DIR$\Stored Procedures\*.sql
RUN $ROOT_DIR$\Views\*.sql
RUN $ROOT_DIR$\Jobs\*.sql

# disconnect and report back
DISCONNECT
SUCCESS Installation Complete

~~~end your file~~~



-- Macro Variables ----------------------------------------------------------


$APPENDGO$

	This is a true/false value which determines if the GO batch separator
	is automatically added after each script that is written using the
	WRITE command to open files. This is useful if you scripts which may
	or may not have the batch separator that is needed for some sql
	statements. This is on by default.


$DBBCHANGES$

	This is the schema and table name that is used for tracking which
	scripts have been run. When you use the CONNECT command, it will
	check that this table exists, otherwise create it. When a RUNONCE
	command is used, this table is checked for the passed in script name.
	The default value for this is "dbo.DBBChanges".


$DBBVERSION$

	This is the schema and table name that is used for tracking when the
	schema was initially installed and last updated. When you use the
	CONNECT command, it will check that this table exists, otherwise
	create it. When DISCONNECT is issued, it will update this table with
	a last update date. The default value for this is "dbo.DBBVersion".


$DBCATALOG$

	This is the database catalog to initially point the connection to
	when the CONNECT command is issued. After each script is ran using
	the RUN or RUNONCE command, this value gets updated with the current
	DB catalog as it could have changed in the script. The default value
	for this is "tempdb".


$DBTRUSTED$

	True or False flag to control if the login/pass is used or trusted
	connection to the database for authentication.
	

$DBLOGIN$

	This is to hold the login for connecting to a DB, but this is
	currently not in use.


$DBPASSWORD$

	This is to hold the password for connecting to a DB, but this is
	currently not in use.


$DBSERVER$

	This is the database server to point the connection to when the
	CONNECT command is issued. The default value for this is "localhost".


$PREPENDFILE$

	This is a true/false value which will issue a print statement of the
	script being processed with the WRITE command and prepend it to any
	open file from the APPEND or CREATE command. This is useful for when
	compiling a script and if the script fails, you have direction on
	where to look. This is on by default.


$RUNONCESKIP$

	This is a true/false value which will allow a RUNONCE command to
	run through it's normal logic of ensuring that a script gets run a
	single time, but will not actually run the script. It essentially
	simulates running the script. This is very useful for running through
	some update statements which are already reflected in the schema, but
	you need to have all the previous update scripts shown as having been
	run so they will not run the next time the DB is updated. This is off
	by default.


$VERBOSE$

	This is a true/false value which will determine if the dbb
	application shows feedback on what commands are being run. It will
	not suppress error messages. This is on by default.


$VERSIONING$

	This is a true/false value which will control if any of the
	versioning features are enabled. If this is set to false, it will
	disable the use or creation of the DBBVersion and DBBChanges tables.
	This is off by default.



-- Commands -----------------------------------------------------------------

?
	Shows the list of commands.


APPEND <file to append to>

	Use this command when you want to open up an existing file and append
	scripts to it using the WRITE command. This is similar to the CREATE
	command, except that CREATE will replace the file if it already
	exists instead of appending to the end. When done appending scripts
	to the file, make sure to use the CLOSE command to free up the lock
	and write out any buffered text to the file (especially if you use
	the file elsewhere in your dbb recipe file).


CLOSE <file to close>

	Use this command to close any open files for output. The can be a
	file opened using the APPEND or CREATE command. This action will
	happen on EXIT, but should be noted to explicitly close a file when
	that file is called elsewhere in the dbb recipe for input.


CMD <command to execute>

	Use this command to run an executable outside of the dbb application.


CONNECT

	Use this command to connect to a database as defined by the active
	values for the macro variables $DBSERVER$, $DBCATALOG$. Currently the
	connection is made with a trusted connection of the executing user,
	but this will change in a future release and will optionally use the
	$DBLOGIN$ and $DBPASSWORD$ macro variables to determine if it should
	connect with a sql login. Warning - The scripts that run could easily
	change the active DB catalog (use master), so be warned!


CREATE <file to create>

	Use this command when you want to create or replace a file for
	writing out scripts to with the WRITE command. This is similar to
	the APPEND command, except that APPEND will keep any existing file
	and append to the end. When done, make sure to use the CLOSE command
	to free up the lock and write out any buffered text to the file
	(especially if you use the file elsewhere in your dbb recipe file).


DISCONNECT

	Use this command to disconnect from the database server that was
	connected to with the CONNECT command..


EXIT

	Quits the dbb application.


INCLUDE <dbb file to process>

	Use this command to call out to other dbb recipe files. Be aware that
	any settings you have will be applied to that file as well. The
	execution will occur as if the included file was part of the calling
	dbb recipe file.


RUN <file(s) to run>

	Use this command to run a script (or set of scripts) against the
	active connected database as determined by the CONNECT command. You
	can pass in a single file or a group of files by using a wildcard
	such as "somewhere\schema.sql" or "somewhere\*.sql".


RUNONCE <file(s) to run>

	Use this command to run a script (or set of scripts) exactly once
	against the active connected database as determined by the CONNECT
	command. This command makes use of the versioning tables and will
	look to see if the script has been run against the DB or not. If it
	has, it will just skip the script and move on to the next. There are
	options around this command which should be noted in the Macro
	Variables section of this document. You can pass in a single file or
	a group of files by using a wildcard such as "somewhere\schema.sql"
	or "somewhere\*.sql".


SET <find> = <replace>

	Sets a macro variable that is available for use in other command
	arguments (such as RUN $ROOT_DIR$\myscript.sql) and will also be
	used as substitutions on all scripts that are input using the RUN,
	RUNONCE and WRITE command. It is possible to use this as a search and
	replace as there are no restrictions on what you name your macro
	variables (such as SET localhost = 10.10.10.1). There are a set of
	application default macro variables which are listed in the section
	above and will control the behavior of some commands.


SHOW

	Shows all macro variables and their values.


SUCCESS <optional message>

	Outputs a confirmation of success with an optional message as
	feedback that the dbb recipe made it to a certain point.


TEST <file to test>

	This command might go away some day. This was added to test the
	output of a batch of commands (outputting errors from OSQL into a
	file). It basically will scan a file for lines that look like SQL
	error messages and exit dbb with an error code of 1 if it finds them.


WRITE <file(s) to written>

	Use this command to write a script (or set of scripts) against any
	files that have been opened with APPEND or CREATE. It is useful to
	note that you can have several files written to at once by using the
	APPEND or CREATE commands multiple times for different files. You
	can pass in a single file or a group of files by using a wildcard
	such as "somewhere\schema.sql" or "somewhere\*.sql".



-- Requirements -------------------------------------------------------------
This executable will run, provided you have MS SQL Server 2012 SMO.

Microsoft.SqlServer.ConnectionInfo
<sql directory>\120\SDK\Assemblies\Microsoft.SqlServer.ConnectionInfo.dll

Microsoft.SqlServer.Management.Sdk.Sfc
<sql directory>\120\SDK\Assemblies\Microsoft.SqlServer.Management.Sdk.Sfc.dll

Microsoft.SqlServer.Smo
<sql directory>\120\SDK\Assemblies\Microsoft.SqlServer.Smo.dll

