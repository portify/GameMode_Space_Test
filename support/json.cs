function JSON::parse(%text)
{
	%text = rTrim(%text);
	%length = strLen(%text);

	%result = JSON::scan(%text, 0, %length);

	if (%result $= "" || getWord(%result, 0) != %length)
		return "";

	return restWords(%result);
}

function JSON::dumps(%json, %type)
{
	if (%type $= "")
		%type = JSON::inferType(%json);

	switch$ (%type)
	{
		case "number":
			return %json;

		case "string":
			//%json = expandEscape(expandEscape(%json));
			//%json = collapseEscape(strReplace(%json, "\\\\'", "'"));
			//return "\"" @ %json @ "\"";
			return "\"" @ JSON::escape(%json) @ "\"";

		case "null":
			return "null";

		case "bool":
			return %json ? "true" : "false";

		case "array":
			for (%i = 0; %i < %json.length; %i++)
			{
				if (%i != 0)
					%joined = %joined @ ",";

				%joined = %joined @ JSON::dumps(%json.value[%i]);
			}

			return "[" @ %joined @ "]";

		case "map":
			%length = %json.__keys.length;

			for (%i = 0; %i < %length; %i++)
			{
				if (%i != 0)
					%joined = %joined @ ",";

				%key = %json.__keys.value[%i];

				%joined = %joined @ JSON::dumps(%key, "string") @ ":";
				%joined = %joined @ JSON::dumps(%json.__value[%key]);
			}

			return "{" @ %joined @ "}";
	}

	return "null";
}

function JSON::escape(%text)
{
	%length = strLen(%text);

	for (%i = 0; %i < %length; %i++)
	{
		%char = getSubStr(%text, %i, 1);

		switch$ (%char)
		{
			case "\"": %char = "\\\"";
			case "'": %char = "\\'";
			case "\x08": %char = "\\b";
			case "\x0C": %char = "\\f";
			case "\n": %char = "\\n";
			case "\r": %char = "\\r";
			case "\t": %char = "\\t";
		}

		%escaped = %escaped @ %char;
	}

	return %escaped;
}

function JSON::parseFile(%fileName)
{
	if (!isFile(%fileName))
	{
		error("ERROR: File '" @ %fileName @ "' does not exist");
		return "";
	}

	%file = new FileObject();

	if (!%file.openForRead(%fileName))
	{
		error("ERROR: Failed to open '" @ %fileName @ "' for reading");
		%file.delete();
		return "";
	}

	while (!%file.isEOF())
		%text = %text @ %file.readLine() @ "\n";

	%file.close();
	%file.delete();

	return JSON::parse(%text);
}

function JSON::dumpsFile(%json, %fileName)
{
	if (!isWriteableFileName(%fileName))
	{
		error("ERROR: File '" @ %fileName @ "' is not writeable");
		return 1;
	}

	%file = new FileObject();

	if (!%file.openForWrite(%fileName))
	{
		error("ERROR: Failed to open '" @ %fileName @ "' for writing");
		%file.delete();
		return 1;
	}

	%file.writeLine(JSON::dumps(%json));
	%file.close();
	%file.delete();

	return 0;
}

function JSON::inferType(%text)
{
	if (%text.class $= "ArrayObject")
		return "array";

	if (%text.class $= "MapObject")
		return "map";

	%length = strLen(%text);
	%result = JSON::scanNumber(%text, 0, %length);

	if (%result !$= "" && firstWord(%result) == %length)
		return "number";

	return "string";
}

function JSON::scan(%text, %i, %length)
{
	%i = JSON::skipSpacing(%text, %i, %length);
	%chr = getSubStr(%text, %i, 1);

	if (%chr $= "\"")
	{
		for (%j = %i++; %j < %length; %j++)
		{
			// TODO: Optimize by tracking last character in %escaped
			if (getSubStr(%text, %j, 1) $= "\"" && getSubStr(%blob, %j - 1, 1) !$= "\\")
				return %j + 1 SPC collapseEscape(getSubStr(%text, %i, %j - %i));
		}

		return "";
	}

	if (%chr $= "[")
		return JSON::scanArray(%text, %i + 1, %length);

	if (%chr $= "{")
		return JSON::scanMap(%text, %i + 1, %length);

	if (strstr("null", getSubStr(%text, %i, 4)) == 0)
		return %i + 4 SPC "";

	if (strstr("true", getSubStr(%text, %i, 4)) == 0)
		return %i + 4 SPC 1;

	if (strstr("false", getSubStr(%text, %i, 5)) == 0)
		return %i + 5 SPC 0;

	return JSON::scanNumber(%text, %i, %length);
}

function JSON::scanNumber(%text, %i, %length)
{
	%j = %i;
	%chr = getSubStr(%text, %j, 1);

	if (%chr $= "-")
	{
		%j++;
		%chr = getSubStr(%text, %j, 1);
	}

	if (%chr $= "0")
		%zero = 1;

	for (%j; %j < %length; %j++)
	{
		%chr = getSubStr(%text, %j, 1);

		if (%chr $= ".")
		{
			if (%radix || !%first)
				return "";

			%radix = 1;
			%first = 0;
		}
		else if (strpos("0123456789", %chr) != -1)
			%first = 1;
		else
			break;
	}

	if (!%first || %j - %i < 1)
		return "";

	if (%zero && %j - %i > 1)
		return "";

	return %j SPC getSubStr(%text, %i, %j - %i);
}

function JSON::scanArray(%text, %i, %length)
{
	%comma = 0;
	%empty = 1;

	%array = Array();
	%array.__isJSONObject = 1;

	while (1)
	{
		%i = JSON::skipSpacing(%text, %i, %length);

		if (%i > %length)
			break;

		%chr = getSubStr(%text, %i, 1);

		if (%chr $= "]")
		{
			if (%comma)
				break;
			else
				return %i + 1 SPC %array;
		}

		if (%chr $= ",")
		{
			if (%empty || %comma)
				break;

			%comma = 1;
			%i++;

			continue;
		}

		%result = JSON::scan(%text, %i, %length);

		if (%result $= "")
			break;

		%i = firstWord(%result);
		%array.append(restWords(%result));

		%empty = 0;
		%comma = 0;
	}

	%array.delete();
	return "";
}

function JSON::scanMap(%text, %i, %length)
{
	%comma = 0;
	%empty = 1;

	%map = Map();
	%map.__isJSONObject = 1;

	while (1)
	{
		%i = JSON::skipSpacing(%text, %i, %length);

		if (%i > %length)
			break;

		%chr = getSubStr(%text, %i, 1);

		if (%chr $= "}")
		{
			if (%comma)
				break;
			else
				return %i + 1 SPC %map;
		}

		if (%chr $= ",")
		{
			if (%empty || %comma)
				break;

			%comma = 1;
			%i++;

			continue;
		}

		if (%chr !$= "\"")
			break;

		%result = JSON::scan(%text, %i, %length);

		if (%result $= "")
			break;

		%i = JSON::skipSpacing(%text, firstWord(%result), %length);
		%key = restWords(%result);

		if (getSubStr(%text, %i, 1) !$= ":")
			break;

		%result = JSON::scan(%text, %i + 1, %length);

		if (%result $= "")
			break;

		%i = firstWord(%result);
		%map.set(%key, restWords(%result));

		%comma = 0;
		%empty = 0;
	}

	%map.delete();
	return "";
}

function JSON::skipSpacing(%text, %i, %length)
{
	for (%i; %i < %length; %i++)
	{
		if (strpos(" \t\r\n", getSubStr(%text, %i, 1)) == -1)
			break;
	}

	if (getSubStr(%text, %i, 2) !$= "//")
		return %i;

	for (%i++; %i < %length; %i++)
	{
		if (getSubStr(%text, %i, 1) $= "\n")
			break;
	}

	return JSON::skipSpacing(%text, %i, %length);
}