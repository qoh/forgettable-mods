exec("./common.cs");

if ($CoreSupport::ServerCommandCount $= "")
{
	$CoreSupport::ServerCommandCount = 0;
}

function registerServerCommand(%name, %desc, %args, %flags)
{
	if (!$CoreSupport::IsServerCommand[%name])
	{
		$CoreSupport::IsServerCommand[%name] = true;
		$CoreSupport::ServerCommandName[
			$CoreSupport::ServerCommandCount] = %name;
		$CoreSupport::ServerCommandCount++;
		$CoreSupport::ServerCommandsSorted = false;
	}

	%adminOnly = false;

	%flagCount = getRecordCount(%flags);

	for (%i = 0; %i < %flagCount; %i++)
	{
		%flag = getRecord(%flags, %i);

		if (%flag $= "adminonly")
		{
			%adminOnly = true;
		}
		else
		{
			warn("registerServerCommand(\"" @ expandEscape(%name) @
				"\", ...): Unrecognized flag '" @ %flag @ "'");
			backTrace();
		}
	}

	$CoreSupport::ServerCommandDesc[%name] = %desc;
	$CoreSupport::ServerCommandArgs[%name] = %args;
	$CoreSupport::ServerCommandAdminOnly[%name] = %adminOnly;
}

registerServerCommand("Help",
	"Display a list of commands available on the server.");
registerServerCommand("Find",
	"Teleport yourself to someone else.",
	"Target\tplayername");
registerServerCommand("Fetch",
	"Teleport someone else to yourself.",
	"Target\tplayername");

$coreSupport_helpStartArg[false] = "<";
$coreSupport_helpStartArg[true] = "[";
$coreSupport_helpEndArg[false] = ">";
$coreSupport_helpEndArg[true] = "]";
$coreSupport_helpArgColor[false] = "eecc77";
$coreSupport_helpArgColor[true] = "ccaa33";

function serverCmdHelp(%client)
{
	if (getRealTime() - %client.lastHelpRequestTime < 100)
	{
		return;
	}

	%client.lastHelpRequestTime = getRealTime();
	coresupport_serverEnsureCommandsSorted();

	messageClient(%client, '', " ");
	messageClient(%client, '',
		"<color:555555>========== <color:999999>/Help start: <color:99dd99>" @
		"Page Down <color:999999>scrolls <color:555555>==========");

	for (%i = 0; %i < $CoreSupport::ServerCommandCount; %i++)
	{
		%name = $CoreSupport::ServerCommandName[%i];

		if ($CoreSupport::ServerCommandAdminOnly[%name] && !%client.isAdmin)
		{
			continue;
		}

		%desc = $CoreSupport::ServerCommandDesc[%name];
		%args = $CoreSupport::ServerCommandArgs[%name];

		%argsText = "";

		if (%compact < 2)
		{
			%argsCount = getRecordCount(%args);

			for (%j = 0; %j < %argsCount; %j++)
			{
				%arg = getRecord(%args, %j);
				%argName = getField(%arg, 0);
				%fieldCount = getFieldCount(%arg);
				%isOptional = false;

				for (%k = getFieldCount(%arg) - 1; %k >= 2; %k--)
				{
					if (getField(%arg, %k) $= "optional")
					{
						%isOptional = true;
						break;
					}
				}

				%argsText = %argsText SPC
					"<color:" @ $coreSupport_helpArgColor[%isOptional] @ ">" @
					$coreSupport_helpStartArg[%isOptional] @ %argName @
					$coreSupport_helpEndArg[%isOptional];
			}
		}

		%color = $CoreSupport::ServerCommandAdminOnly[%name]
			? "ff4400"
			: "ffff00";

		if (%compact)
		{
			messageClient(%client, '', " <color:" @ %color @ ">/" @ %name @
				"<color:ffffff>- " @ %desc);
		}
		else
		{
			messageClient(%client, '', " <color:" @ %color @ ">/" @ %name @ %argsText);

			if (%desc !$= "")
			{
				messageClient(%client, '', "    <color:ffffff>" @ %desc);
			}
		}
	}

	messageClient(%client, '',
		"<color:555555>========== <color:999999>/Help end: <color:99dd99>" @
		"Page Up <color:999999>scrolls <color:555555>==========");
}

function serverCmdFind(%client, %targetName)
{
	%clientPlayer = getClientPlayer(%client);

	if (!isObject(%clientPlayer))
	{
		messageClient(%client, '',
			"<color:ff0000>You do not have a body to teleport.");
		return;
	}

	%targetClient = findClient(%targetName);

	if (!isObject(%targetClient))
	{
		messageClient(%client, '',
			"<color:ff0000>No player found by that name.");
		return;
	}

	if (%client.getID() == %targetClient)
	{
		messageClient(%client, '', "<color:ff0000>Nothing happens...");
		return;
	}

	%targetPlayer = getClientPlayer(%targetClient);

	if (!isObject(%targetPlayer))
	{
		messageClient(%client, '',
			"<color:ff0000>That player has no body to teleport to.");
		return;
	}

	%clientPlayer.setTransform(
		VectorAdd(%targetPlayer.getTransform(), "0 0 1"));
	%clientPlayer.setVelocity("0 0 0");
}

function serverCmdFetch(%client, %targetName)
{
	%clientPlayer = getClientPlayer(%client);

	if (!isObject(%clientPlayer))
	{
		messageClient(%client, '',
			"<color:ff0000>You do not have a body to teleport players to.");
		return;
	}

	%targetClient = findClient(%targetName);

	if (!isObject(%targetClient))
	{
		messageClient(%client, '',
			"<color:ff0000>No player found by that name.");
		return;
	}

	if (%client.getID() == %targetClient)
	{
		messageClient(%client, '', "<color:ff0000>Nothing happens...");
		return;
	}

	%targetPlayer = getClientPlayer(%targetClient);

	if (!isObject(%targetPlayer))
	{
		messageClient(%client, '',
			"<color:ff0000>That player has no body to teleport.");
		return;
	}

	%targetPlayer.setTransform(
		VectorAdd(%targetPlayer.getTransform(), "0 0 1"));
	%targetPlayer.setVelocity("0 0 0");
}

function serverCmdCoreSupport_GetCommandList(%client)
{
	if (getRealTime() - %client.lastCommandListRequestTime < 1000)
	{
		return;
	}

	%client.lastCommandListRequestTime = getRealTime();
	coresupport_serverSendCommandList(%client);
}

function coresupport_serverEnsureCommandsSorted()
{
	if (!isObject(SortTextList))
	{
		new GuiTextListCtrl(SortTextList);
	}

	SortTextList.clear();

	for (%i = 0; %i < $CoreSupport::ServerCommandCount; %i++)
	{
		SortTextList.addRow(%i, $CoreSupport::ServerCommandName[%i]);
	}

	SortTextList.sort(0, true);

	for (%i = 0; %i < $CoreSupport::ServerCommandCount; %i++)
	{
		$CoreSupport::ServerCommandName[%i] = SortTextList.getRowText(%i);
	}

	SortTextList.clear();
}

function coresupport_serverSendCommandList(%client)
{
	coresupport_serverEnsureCommandsSorted();
	commandToClient(%client, 'CoreSupport_ClearServerCommands');

	for (%i = 0; %i < $CoreSupport::ServerCommandCount; %i++)
	{
		%name = $CoreSupport::ServerCommandName[%i];

		if ($CoreSupport::ServerCommandAdminOnly[%name] && !%client.isAdmin)
		{
			continue;
		}

		%desc = $CoreSupport::ServerCommandDesc[%name];
		%args = $CoreSupport::ServerCommandArgs[%name];

		commandToClient(%client, 'CoreSupport_AddServerCommand',
			%name,
			$CoreSupport::ServerCommandDesc[%name],
			$CoreSupport::ServerCommandArgs[%name],
			$CoreSupport::ServerCommandFlags[%name]);
	}
}

function findClient(%name)
{
	%count = ClientGroup.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		%client = ClientGroup.getObject(%i);

		if (%client.nameBase $= %name)
		{
			return %client;
		}
	}

	%name = strlwr(%name);

	for (%i = 0; %i < %count; %i++)
	{
		%client = ClientGroup.getObject(%i);

		if (strpos(strlwr(%client.nameBase), %name) != -1)
		{
			return %client;
		}
	}

	return 0;
}

function getClientPlayer(%client)
{
	if (isObject(%client.player))
	{
		return %client.player;
	}

	if (isObject(%client.ghostPlayer))
	{
		return %client.ghostPlayer;
	}

	return 0;
}

package CoreSupportServer
{
	function serverCmdMessageSent(%client, %message)
	{
		if (!coresupport_parseCommandText(%message, 0))
		{
			return Parent::serverCmdMessageSent(%client, %message);
		}

		%commandName = $ret4;
		%numArgs = $ret5;

		for (%i = 0; %i < %numArgs; %i++)
		{
			%a[%i] = $ret[6 + %i];
		}

		call("serverCmd" @ %commandName, %client,
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17);
	}
};

activatePackage(CoreSupportServer);

$ModLoaded_Server[coresupport] = 1;
