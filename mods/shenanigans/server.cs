if (!$ModLoaded_Server[coresupport] &&
	isFile("Add-Ons/mods/coresupport/server.cs"))
{
	exec("Add-Ons/mods/coresupport/server.cs");
}

if ($ModVersion[coresupport] !$= 1 || !$ModLoaded_Server[coresupport])
{
	error("Mod 'shenanigans' missing v1 of mod 'coresupport'");
	return;
}

registerServerCommand("FreeCam", "Toggle a free flying camera mode.");
registerServerCommand("Spawn",
	"Place a new instance on your player of (almost) anything in the game.",
	"Object\tdataBlock", "adminonly");
registerServerCommand("Dungeon",
	"Enter a new dungeon by theme name.",
	"Theme\tstring\nInstant\tbool\toptional", "adminonly");

function serverCmdFreeCam(%client)
{
	%control = %client.getControlObject();

	if (%control == %client.camera)
	{
		%clientPlayer = getClientPlayer(%client);

		if (isObject(%client.player))
		{
			%client.setControlObject(%clientPlayer);
		}
		else
		{
			messageClient(%client, '',
				"<color:ff0000>You have no body to switch back to.");
		}
	}
	else
	{
		%transform = VectorAdd(%control.getWorldBoxCenter(), "0 0 3");
		%client.setControlObject(%client.camera);
		%client.camera.setFlyMode();
		%client.camera.setTransform(%transform);
	}
}

function serverCmdSpawn(%client, %dataName)
{
	if (!%client.isAdmin)
	{
		messageClient(%client, '',
			"<color:ff0000>You must be a server admin to use this command.");
		return;
	}

	%clientPlayer = getClientPlayer(%client);

	if (!isObject(%clientPlayer))
	{
		messageClient(%client, '',
			"<color:ff0000>You do not have a body to spawn things on.");
		return;
	}

	%data = nameToID(%dataName);

	%targetTransform = %clientPlayer.getTransform();

	if (!isObject(%data))
	{
		%data = 0;
		%className = "";
	}
	else
	{
		%dataName = %data.getName();
		%className = %data.getClassName();
	}

	if (%className $= "PlayerData")
	{
		%clientPlayer.setDamageTimeout();
		%spawned = %data.buildBot();
		%spawned.setTransform(%targetTransform);
	}
	else if (%className $= "ItemData")
	{
		%spawned = placeItemAtPos(%data, %targetTransform, "");
	}
	else if (
		%className $= "InteractableData" || %className $= "TrapData" ||
		%className $= "DoorData")
	{
		%spawned = %data.createItem();
		%spawned.setTransform(%targetTransform);
	}
	else
	{
		messageClient(%client, '',
			"<color:ff0000>No supported type of object to spawn found" @
			"by that name.");
		return;
	}

	$s4 = $s3;
	$s3 = $s2;
	$s2 = $s1;
	$s1 = $s0;
	$s0 = %spawned;
	$s = $s0;
}

function serverCmdDungeon(%client, %theme, %instant)
{
	if (!%client.isAdmin)
	{
		messageClient(%client, '',
			"<color:ff0000>You must be a server admin to use this command.");
		return;
	}

	if (!attemptDuckTypeTheme(%theme))
	{
		%theme = %theme @ "Theme";

		if (!attemptDuckTypeTheme(%theme))
		{
			messageClient(%client, '',
				"<color:ff0000>Theme does not appear to be valid.");
			return;
		}
	}

	if (!%instant)
	{
		%fromTheme = getDungeonTheme();
		clearDungeoneThemOrder();

		$theme::current = 0;
		$theme::floor = 0;
		$theme::overallProgress = 0;

		$nThemes = 2;
		$themeOrder0 = %fromTheme;
		$themeOrder1 = %theme;

		startChangeDungeon();
	}
	else
	{
		replaceDungeonTheme(%theme);
		clearDungeon();
		createDungeon();
		onDungeonLoaded();
	}
}

function attemptDuckTypeTheme(%theme)
{
	return
		%theme.skin !$= "" ||
		%theme.floors !$= "" ||
		%theme.prefabFolder !$= "";
}

function walkAnywhere(%player)
{
	cancel(%player.walkAnywhereLoop);

	%xy = %player.getGridPos();
	%x = getWord(%xy, 0);
	%y = getWord(%xy, 1);

	for (%dx = -1; %dx <= 1; %dx++)
	{
		for (%dy = -1; %dy <= 1; %dy++)
		{
			setServerTileType(%x + %dx, %y + %dy, $TILEFLOOR);
		}
	}

	if (%player.client.getName() $= LocalClientConnection)
	{
		clientCmdSetAlertLine(%xy);
	}

	%player.walkAnywhereLoop = schedule(100, %player, walkAnywhere, %player);
}

