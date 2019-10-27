$ModVersion[respawning] = 1;

if (!$ModLoaded_Server[coresupport] &&
	isFile("Add-Ons/mods/coresupport/server.cs"))
{
	exec("Add-Ons/mods/coresupport/server.cs");
}

if ($ModVersion[coresupport] $= 1 && $ModLoaded_Server[coresupport])
{
	registerServerCommand("TrulyGiveUp", "Give up on the run and end it.",
		"", "adminonly");
}

if ($Pref::Server::Mods::RespawningEnabled $= "")
{
	$Pref::Server::Mods::RespawningEnabled = true;
}

if ($Pref::Server::Mods::RespawningItems $= "")
{
	// keep: Items don't drop, kept on respawn.
	// drop-no-new: Items drop, no new items given on respawn.
	// drop-new: Items drop, new items given on respawn.
	$Pref::Server::Mods::RespawningItems = "drop-no-new";
}

function respawning_respawnClient(%client, %giveNewItems)
{
	if (isObject(%client.ghostPlayer))
	{
		%client.ghostPlayer.delete();
		%client.ghostPlayer = 0;
	}

	if (isObject(%client.player))
	{
		%client.player.delete();
		%client.player = 0;
	}

	%client.setStartingHealth();

	if (%giveNewItems)
	{
		%client.createInventory();
	}

	%client.spawnPlayer();
	%client.setSpectate(0);
	%client.updateStats();
}

function serverCmdTrulyGiveUp(%client)
{
	if (!%client.isAdmin)
	{
		messageClient(%client, '',
			"<color:ff0000>You must be a server admin to use " @
			"<color:ffff66>/TrulyGiveUp<color:ff0000>.");
		return;
	}

	setGameOverDeathMessage("Forfeited",
		%client.nameBase @ " gave up on the run.");
	doGameOverScreen(true);
}

package RespawningServer
{
	function Player::die(%player)
	{
		if ($Pref::Server::Mods::RespawningItems !$= "keep")
		{
			if (isObject(%player.client.inventory))
			{
				%player.client.inventory.dropAllItems();
			}
		}

		return Parent::die(%player);
	}

	function GameConnection::doRealDeath(%client)
	{
		if (!$Pref::Server::Mods::RespawningEnabled)
		{
			return Parent::doRealDeath(%client);
		}

		respawning_respawnClient(%client,
			$Pref::Server::Mods::RespawningItems $= "drop-new");
	}

	function checkForGameOverState()
	{
		return !$Pref::Server::Mods::RespawningEnabled &&
			Parent::checkForGameOverState();
	}

	function gameOverCheck()
	{
		return !$Pref::Server::Mods::RespawningEnabled &&
			Parent::gameOverCheck();
	}

	function doGameOverScreen(%force)
	{
		if (!$Pref::Server::Mods::RespawningEnabled || %force)
		{
			return Parent::doGameOverScreen();
		}
	}

	function onDungeonLoaded()
	{
		Parent::onDungeonLoaded();

		%i = ClientGroup.getCount();

		while (%i-- >= 0)
		{
			%client = ClientGroup.getObject(%i);

			if (isObject(%client.ghostPlayer) && !isObject(%client.player))
			{
				respawning_respawnClient(%client, true);
			}
		}
	}
};

activatePackage(RespawningServer);

$ModLoaded_Server[respawning] = 1;
