# MMCloud - File Storage and Sharing

Windows application that provides Cloud file storage and file sharing solutions using TCP socket programming in C#

**Designed & Implemented by:**
 * [Mert Kosan](https://github.com/mertkosan)
 * [M.Mucahid Benlioglu](https://github.com/mbenlioglu)

## Getting Started

This repository contains both server-side and client-side projects and combines these two with one .sln file.

**Folder Structure:**

	/
	|
	|___ MMClient/
	|
	|___ MMServer/
	|
	|___ MMCloud.sln

In order to obtain a successful developement environment, project should be loaded from MMCloud.sln. This way
common functionalities wil be connected in a proper manner.

## Features

* __Client:__
    * Provide input checks to confirm whether the entered input is in the appropriate format or not, prompts proper
      message if not.
    * Client login supports timeout for login operation, prompts appropriate message for failed connections (i.e. no
      server running at given address)
    * If the username entered is already active in the system, User is shown an appropriate messagebox and connection
      is terminated.
    * After login user can browse files to upload, upload files to server, download, rename, delete or manage sharing
      status of existing files in the cloud, logout or terminate the application. 
    * User can select multiple files to upload. These files are queued and sequentially uploaded.
    * Client detects the server's availability during upload request, and automatically logs out the user.
    * If user wants to logout or terminate the application during an ongoing upload, he/she will be prompted to decide
      whether to cancel the upload  and leave or stay.
* __Server:__
    * During initialization validity of port number and selected cloud path is checked, if any inconsistencies are found
      an appropriate prompt is shown to user.
    * The server can check if the user already active in the system.
    * The server can create necessery files & folders for the user.
    * The server can handle multiple incoming connections.
    * The server can be stopped and started anytime, if a server stop or crash happens, client will cancel any upload
      process.
    * After the termination of the program, when server is restarted previously selected cloud path will be remembered
      and filepath will be automatically filled with the previous selection.

* All activities done by both client and server is printed to their respective rich text boxes.
