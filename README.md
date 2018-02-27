# Return of Reckoning Launcher
This project is an attempt to provide a better, open source, alternative to the official Return of Reckoning Launcher.

![Screenshot](http://i.imgur.com/YTGiy1M.png)

It features the following over the current official launcher as of Feb 26 2018:

* Added a minimize button to the main window
* On successful game start, the launcher will automatically minimize
* On game exit, the launcher will automatically restore to the screen
* Users can start the game multiple times within the same launcher session
    * Ex. if a user launches the game, exits the game, they can click Connect to launch the game again without issues
* Greatly improved the error handling
* Added better dependency file handling and checks
    * By default it will allow the user to have dependency files with a higher file version number than the embedded ones without attempting to replace them with the embedded files.
* Added launch arguments
    * --Debug starts the launcher and begins exporting debug information to 3 files in the current directory, an application configuration file named RoR_Configs.txt, a DxDiag report named RoR_DxDiag.txt, and an MSInfo32 report named RoR_MSInfo.txt.
    * --NoErrors will suppress most error popup windows and some error messages, but continue to give short error text messages on the main window for critical messages.
    * --CustomDeps disables dependency file checking. This will assume the user has all the required dependency files and that they will work with the launcher.
    * --CheckDepHash enables the old method of dependency file hash comparisons instead of file version comparisons to determine if an embedded dependency file needs to replace an external dependency file. This can be helpful if a user wants to make sure their external dependency files are identical to the ones that come embedded.
    * --SkipSSLValidation will skip all SSL Validation checks during web requests. This allows the launcher to continue working like normal while the SSL Certificate for the login server is expired.
* Bug fixes and optimizations for numerous issues including the Connect button not always working
* Cleaned up the window interfaces from the developer perspective in visual studio (fixes some issues at the same time)
* Uses a new Mythic Patch Handler (MYPHandler) that no longer requires Performance Counters
    * This is also open source and can be found [here](https://github.com/ThiconZ/Mythic-Patch-Handler).
