# Empyrion-Tetherporter

This is a Server based mod for Empyrion designed for events on the Anvil server, but could be used on other servers.

The "entry point" for mod code is Tetherporter.cs

It is recommended when setting up a portal to do so with a bit of space, not near any walls, and ideally levitating off the ground slightly. 

Non-god mode players are also often injured during their particle realignment(fall damage) so medical stations near portals are also recommended.

The following two command are made available to all players:

# !tetherport 
**Alternative: !ttp**

This command will launch a UI and list portal locations specified in the portal.tether file.
Only admins will see records where AdminYN = Y. Only admins will see the additonal commands available.

Once a user chooses a portal, the mod will save a users playfield and location to: Database/{steamId}.tether
Then the player will be telported to the selected portal.
If a user has a tetherport file already, one will not be created. This is to allow users to visit multiple portals and then return.

# !untether
This will return the player to their saved location, and delete their tether record.

# Admin-Only Commands:
**Currently portal names should not include spaces. Quotes are not required.**

# !portal-create "PortalName"
This will create a portal record in portal.tether.

# !portal-admin "PortalName"
This will flip the AdminYN value of the portal, allowing non-admins to see and access it.

# !portal-delete-please "PortalName"
This will delete the portal specified. You have to say please to make sure you want to do it.
