============================== MMCLOUD - INTRODUCTION ==============================

	Following README file explains implementation of the cloud project, current
stage of the project and provides a user manual for both building and using the 
project.

The MMCloud © project is designed and implemented by:
 
	Mert KOSAN - 17472
	M. Mucahid BENLIOGLU - 17871

===================================================================================
			BUILDING AND EXECUTION OF THE PROJECT
===================================================================================

	Folder structure:
	
	MMCloud/
	|
	|____MMClient/
	|
	|____MMServer/
	|
	|____MMCloud.sln
	
	
	The project folder ( MMCloud/ ) contains two subfolder; MMClient/ for the client
program and MMServer/ for the server program. MMCloud.sln file combines these two 
projects into one visual studion solution and makes it easier to compile and debug.

	The client program and the server program combines some of the common functinalities
into a common c# code. This common class is shared between the projects and the .sln file
combines them together. Therefore, starting the project by using MMCloud.sln is both easier
and needed.


===================================================================================
			CURRENT STAGE OF THE PROJECT
===================================================================================

[24/11/2016]: Project step 1 is completed
	The client:
		- The client and server can establish communication.
		- The client checks the format of the input entered by the user, prompts
		appropriate messageboxes if wrong format is encountered.
		- The client tries to connect the server with given details. After a timeout
		if connection cannot be established, (i.e. no server is running with entered
		details) an appropriate messagebox is shown to user.
		- If the username entered is already active in the system, User is shown an 
		appropriate messagebox and connection is terminated.
		- After login user can browse files to upload, upload files to server, logout or
		terminate the application. 
		- User can select multiple files to upload. These files are queued and sequentially
		uploaded.
		- During the upload if the browsed file is not found or, entered filepath is wrong 
		user is given the choice to skip that file, cancel the upload processs or retry.
		- Client detects the server's availability during upload request, and logs out the 
		user.
		- If user wants to logout or terminate the application during an ongoing upload, he 
		will be prompted to decide whether to cancel the upload  and leave or stay.
	The server:
		- During initialization validity of port number and selected cloud path is checked,
		if any inconsistencies are found an appropriate prompt is shown to user.
		- The server can check if the user already active in the system.
		- The server can create necessery files & folders for the user.
		- The server can handle multiple incoming connections.
		- The server can be stopped and started anytime, if a server stop or crash happens,
		client will cancel any upload process.
		- After the termination of the program, when server is restarted previously selected 
		cloud path will be remembered and filepath will be automatically filled with the previous
		selection.
	
	All activities done by both client and server is printed to their respective rich text boxes.
	
[15/12/2016]: Project step 2 is completed
	Necessery changes are done to handle download, rename, delete, file list request functionalities
	
[28/12/2016]: Project step 2 is completed
	Necessery changes are done to handle file share and revoke functionalities
===================================================================================
===================================================================================