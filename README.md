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
