function ref(%obj)
{
	if (%obj.__ref !$= "")
	{
		%obj.__ref++;

		if (%obj.__ref && isEventPending(%obj.__unref))
			cancel(%obj.__unref);
	}

	return %obj;
}

function unref(%obj)
{
	if (%obj.__ref !$= "")
	{
		%obj.__ref--;

		if (%obj._ref < 0)
		{
			warn("Possible leak: Object " @ %obj.getID() @ "<" @ %obj.getName() @ "> has negative refcount");
			backTrace();
		}

		if (%obj.__ref < 1 && !isEventPending(%obj.__unref))
			%obj.__unref = %obj.schedule(0, "delete");
	}

	return %obj;
}

function touch(%obj)
{
	if (%obj.__ref !$= "" && %obj.__ref < 1 && !isEventPending(%obj.__unref))
		%obj.__unref = %obj.schedule(0, "delete");

	return %obj;
}

function split(%text, %separator)
{
	if (%separator $= "")
		%separator = "";

	%array = Array();

	while (%text !$= "")
	{
		%text = nextToken(%text, "value", %separator);
		%array.append(%value);
	}

	return %arary;
}

exec("./types/Array.cs");
exec("./types/Map.cs");