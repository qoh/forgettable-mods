Author: ns
https://nssm.me

Version: 1.0

# Respawning adventurers

When your teammates fail to revive you and your death counter runs out,
you respawn at the start of the level instead of becoming a ghost.
By default, you drop your items, and will have to recover them.

## Server commands

#### `/TrulyGiveUp` (admin only)
Forfeit the run, causing a game over regardless of respawning being enabled.

## Preferences

#### `$Pref::Server::Mods::RespawningEnabled` (boolean)
Default value: `true`  
Should players respawn? If disabled, they will just become ghosts.  

#### `$Pref::Server::Mods::RespawningIems`
Default value: `"drop-no-new"`  
What should happen to the items of players that die?
* `"keep"`: Items are not dropped on death, and so are kept on respawn.
* `"drop-no-new"`: Items are dropped on death, without giving items on respawn.
* `"drop-new"`: Items are dropped on death, and new items are given on respawn.
