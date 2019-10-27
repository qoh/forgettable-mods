$ModVersion[coresupport] = 1;

// returns false:
// returns true:
// 0 = Completion type index (-1: command | n: argument n)
// 1 = Completion entry substring
// 2 = Completion replace start
// 3 = Completion replace end
// 4 = Command name
// 5 = Argument count
// n = Argument n-6
function coresupport_parseCommandText(%text, %editIndex)
{
	if (getSubStr(%text, 0, 1) !$= "/")
	{
		return false;
	}

	%len = strlen(%text);
	%index = strpos(%text, " ", 1);

	if (%index == -1)
	{
		%index = %len;
	}

	$ret4 = getSubStr(%text, 1, %index - 1);

	if (%editIndex >= 1 && %editIndex <= %index)
	{
		// Set completion info for the command name
		$ret0 = -1;
		$ret1 = getSubStr(%text, 1, %editIndex - 1);
		$ret2 = 1;
		$ret3 = %index;
	}

	%index++; // Skip the space if there was one
	%argCount = 0;

	while (%index < %len)
	{
		%startIndex = %index;

		// We are at the start of an argument
		%quoted = getSubStr(%text, %index, 1) $= "\"";

		if (%quoted)
		{
			%endIndex = %index + 1;

			while (true)
			{
				%endIndex = strpos(%text, "\"", %endIndex);

				if (%endIndex == -1 ||
					getSubStr(%text, %endIndex - 1, 1) !$= "\\")
				{
					break;
				}
				else
				{
					// Escaped quotation mark, don't end yet
					%endIndex++;
				}
			}
		}
		else
		{
			%endIndex = strpos(%text, " ", %index);
		}

		if (%endIndex == -1)
		{
			%endIndex = %len;
		}

		if (%quoted)
		{
			%argValue = getSubStr(%text, %index, %endIndex - %index);
			%argValue = strReplace(%argValue, "\\\"", "\"");
			%endIndex++; // Skip over the ending quotation mark
		}
		else
		{
			%argValue = getSubStr(%text, %index, %endIndex - %index);
		}

		if (%editIndex >= %index && %editIndex <= %endIndex)
		{
			// Set completion info for the argument
			$ret0 = %argCount;
			$ret1 = ""; // substring thus far
			$ret2 = %index;
			$ret3 = %endIndex;
		}

		$ret[6 + %argCount] = %argValue;
		%argCount++;

		%index = %endIndex;

		// Continue and consume the space if there is one
		if (getSubStr(%text, %index, 1) $= " ")
		{
			%index++;
		}
	}

	$ret5 = %argCount;
	return true;
}
