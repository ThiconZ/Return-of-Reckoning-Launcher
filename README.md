# Return of Reckoning Launcher
This project is an attempt to provide a better, open source, alternative to the official Return of Reckoning Launcher.

![Screenshot](http://i.imgur.com/YTGiy1M.png)

It features the following over the current official one as of Feb 27 2017:

* Added a minimize button to the main window
* On successful game start, the launcher will automatically minimize
* On game exit, the launcher will automatically restore to the screen
* Users can start the game multiple times within the same launcher session.
    * Ex. if a user launches the game, exits the game, they can click Connect to launch the game again without issues
* Greatly improved the error handling
* Added better dependency file handling and checks
* Added launch arguments
    * --Debug starts the launcher and begins exporting debug information to 3 files in the current directory, an application configuration file named RoR_Configs.txt, a DxDiag report named RoR_DxDiag.txt, and an MSInfo32 report named RoR_MSInfo.txt.
    * --NoErrors will suppress most error popup windows and some error messages, but continue to give short error text messages on the main window for critical messages
* Bug fixes for numerous issues including the Connect button not always working
* Cleaned up the window interfaces from the developer perspective in visual studio (fixes some issues at the same time)
