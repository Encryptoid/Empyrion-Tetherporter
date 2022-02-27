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

# Initial Setup
Please read the Initial Setup section here: https://github.com/Encryptoid/zucchini-empyrion

# Portals
In order to activate a portal, you must place a portal.tether file in the Database folder.
See an example below of format:

Playfield,PosX,PosY,PosZ,RotX,RotY,RotZ,EventName
Haven,-5156.1,29.8,302.6,0,0,0,"Test Event Name"

If this portal file is correct the mod commands will be accesible. Removing this file or it's contents will disable the commands. This does not require a server restart as the portal is checked for when the command is run, so you can start and end an event without a restart. You can also rename the file to store it, perhaps to "disable_portal.tether".

The Empyrion Mod API allows you to assign values for the player position and rotation when teleporting, but I have not been able to get assigning a rotation to work. I always arrive upside down, spin a bit, and then settle. Which is actually pretty immersive.




