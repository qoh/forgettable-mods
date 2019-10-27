exec("./common.cs");

if (!NewMessageHud_Edit.addedCoreSupportEditCommand)
{
	NewMessageHud_Edit.command = "coresupport_clientEditMessage();" @
		NewMessageHud_Edit.command;
	NewMessageHud_Edit.addedCoreSupportEditCommand = true;
}

function coresupport_clientEditMessage()
{
	%text = NewMessageHud_Edit.getValue();
	%cursorPos = NewMessageHud_Edit.getCursorPos();

	if (!coresupport_parseCommandText(%text, %cursorPos))
	{
		// Hide completions if visible
		return;
	}

	%commandName = $ret4;

	// TODO: Autocomplete
}

function clientCmdCoreSupport_ClearServerCommands()
{
	ServerConnection.serverCommandCount = 0;
}

function clientCmdCoreSupport_AddServerCommand(%name, %desc, %args, %flags)
{
	if (!ServerConnection.serverCommandAdded[%name])
	{
		if (ServerConnection.serverCommandCount $= "")
		{
			ServerConnection.serverCommandCount = 0;
		}

		ServerConnection.serverCommandAdded[%name] = true;
		ServerConnection.serverCommandName[
			ServerConnection.serverCommandCount] = %name;
		ServerConnection.serverCommandCount++;
	}

	ServerConnection.serverCommandDesc[%name] = %desc;
	ServerConnection.serverCommandArgs[%name] = %args;
	ServerConnection.serverCommandFlags[%name] = %flags;
}

// .setCursorPos(pos)

package CoreSupportClient
{
	function NewMessageHud::open(%this)
	{
		Parent::open(%this);
	}

	function NewMessageHud::close(%this)
	{
		Parent::close(%this);
	}

	function NewMessageHud_Edit::eval(%this)
	{
		%text = NewMessageHud_Edit.getValue();

		if (!coresupport_parseCommandText(%text, 0))
		{
			return Parent::eval(%this);
		}

		%commandName = $ret4;
		%numArgs = $ret5;

		for (%i = 0; %i < %numArgs; %i++)
		{
			%a[%i] = $ret[6 + %i];
		}

		%taggedCommand = eval("return'" @ expandEscape(%commandName) @ "';");

		commandToServer(%taggedCommand,
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18);

		NewMessageHud.close();
	}
};

activatePackage(CoreSupportClient);

$ModLoaded_Client[coresupport] = 1;
