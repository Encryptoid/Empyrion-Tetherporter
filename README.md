# Empyrion-Tetherporter

This is a Server based mod for Empyrion designed for events on the Anvil server, but could be used on other servers.

The "entry point" for mod code is Tetherporter.cs

The admin must setup a portal.tether file in the server files. This does not require a server restart to enable/disable.

If there is a portal setup, then two commands are available to all players:

# !tetherport 
This command will save a users playfield and location to: Database/{steamId}.tether
Then the player will be telported to the location in the portal.tether file.

# !untether
This will return the player to their saved location. Currently there tether is not deleted, so a player can untether until the portal is disabled when the event ends.


# Notes about initial setup
I found that running on different Empyrion servers, my machines and different hosting services would run the Empyrion Server executable from different locations.
eg. 
Run from: "Empyrion - Dedicated Server" or from "Empyrion - Dedicated Server/DedicatedServer"

This unfortunately means it is difficult to locate a config file and the database folder the mod uses. To solve this, a file named "tetherporter.dbpath" must be added to location that Empyrion runs from. This file will contain a single line, that is the relative path from the run folder, to the database folder.

The Database path it is looking for is: "Empyrion - Dedicated Server\Content\Mods\Tetherporter\Database"
So example tetherport.dbpath content would be:

-If ran from: "Empyrion - Dedicated Server"

The relative path would be "Content/Mods/Tetherporter/Database"

-If ran from "Empyrion - Dedicated Server/DedicatedServer"

The relative path would be "../Content/Mods/Tetherporter/Database"

# Initial setup steps
In order to find the run directory, copy the folder Tetherporter and it's content from the MOD folder to your Empyrion mods folder i.e: "Empyrion - Dedicated Server\Content\Mods"
Boot up your server and look for the log line similar to: 

"-LOG- {EmpyrionModdingFramework} [Tetherporter]TeleportTether is running from directory: L:\SteamLibrary\steamapps\common\Empyrion - Dedicated Server\DedicatedServer"

Then copy the MOD/tetherporter.dbpath with the correct relative path to the run folder.

Reboot the server and you should see the log line:

"Tetherporter will use the database path: L:\SteamLibrary\steamapps\common\Empyrion - Dedicated Server\Content\Mods\Tetherporter\Database"

# Portals
In order to activate a portal, you must place a portal.tether file in the Database folder.
See an example below of format:


Playfield,PosX,PosY,PosZ,RotX,RotY,RotZ,EventName 

Haven,-5156.1,29.8,302.6,0,0,0,"Test Event Name"


If this portal file is correct the mod commands will be accesible. Removing this file or it's contents will disable the commands. This does not require a server restart as the portal is checked for when the command is run, so you can start and end an event without a restart. You can also rename the file to store it, perhaps to "disable_portal.tether".

The Empyrion Mod API allows you to assign values for the player position and rotation when teleporting, but I have not been able to get assigning a rotation to work. I always arrive upside down, spin a bit, and then settle. Which is actually pretty immersive.




