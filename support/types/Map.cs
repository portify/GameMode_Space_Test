function Map()
{
	return new ScriptObject()
	{
		__ref = 0;
		class = "MapObject";
		__keys = Array();
	};
}

function Map::isValidAttr(%key)
{
	if (
		%key $= "class" || %key $= "superClass" || %key $= "__keys" ||
		%key $= "__ref" || %key $= "__unref" || %key $= ""
	)
		return 0;

	%length = strLen(%key);

	%a = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	%b = %a @ "0123456789";

	for (%i = 0; %i < %length; %i++)
	{
		if (strpos(%i ? %b : %a, getSubStr(%key, %i, 1)) == -1)
			return 0;
	}

	return 1;
}

function MapObject::onRemove(%this)
{
	%length = %this.__keys.length;

	for (%i = 0; %i < %length; %i++)
		unref(%this.__value[%this.__keys.value[%i]]);

	%this.__keys.delete();
}

function MapObject::copy(%this)
{
	%map = new ScriptObject()
	{
		__ref = 0;
		class = "MapObject";
		__keys = %this.__keys.copy();
	};

	%length = %this.__keys.length;

	for (%i = 0; %i < %length; %i++)
	{
		%key = %this.__keys.value[%i];
		%map.set(%key, %this.__value[%key]);
	}

	return %map;
}

function MapObject::clear(%this)
{
	%length = %this.__keys.length;

	for (%i = 0; %i < %length; %i++)
		%this.remove(%this.__keys.value[%i], 1);

	%this.__keys.clear();
}

function MapObject::set(%this, %key, %value)
{
	if (%this.__keys.contains(%key))
		unref(%this.__value[%key]);
	else
		%this.__keys.append(%key);

	%this.__value[%key] = ref(%value);

	if (Map::isValidAttr(%key))
		eval("%this." @ %key @ "=%value;");

	return %value;
}

function MapObject::get(%this, %key, %default)
{
	if (%this.__keys.contains(%key))
		return %this.__value[%key];

	return %default;
}

function MapObject::remove(%this, %key, %keepKeyTable)
{
	if (%keepKeyTable || %this.__keys.remove(%key))
	{
		unref(%this.__value[%key]);
		%this.__value[%key] = "";

		if (Map::isValidAttr(%key))
			eval("%this." @ %key @ "=\"\";");

		return 1;
	}

	return 0;
}

function MapObject::setDefault(%this, %key, %default)
{
	if (!%this.__keys.contains(%key))
		%this.set(%key, %default);

	return %this.__value[%key];
}

function MapObject::pop(%this, %key, %default)
{
	if (%this.__keys.contains(%key))
	{
		%value = %this.__value[%key];
		%this.remove(%key);
		return %value;
	}

	return %default;
}

function MapObject::patch(%this, %map)
{
	%length = %map.__keys.length;

	for (%i = 0; %i < %length; %i++)
	{
		%key = %map.__keys.value[%i];
		%this.set(%key, %map.__value[%key]);
	}
}

function MapObject::exists(%this, %key)
{
	return %this.__keys.contains(%key);
}

function MapObject::keys(%this)
{
	return %this.__keys.copy();
}

function MapObject::values(%this)
{
	%array = Array();
	%length = %this.__keys.length;

	for (%i = 0; %i < %length; %i++)
		%array.append(%this.__value[%this.__keys.value[%i]]);

	return %array;
}

function MapObject::pairs(%this)
{
	%array = Array();
	%length = %this.__keys.length;

	for (%i = 0; %i < %length; %i++)
	{
		%key = %this.__keys.value[%i];
		%array.append(Array::from(%key, %this.__value[%key]));
	}

	return %array;
}

function MapObject::add(%this, %key, %value)
{
	if (%this.__keys.contains(%key))
	{
		%old = %this.__value[%key] | 0;
		%this.set(%key, ((%old | 0) + (%value | 0)) | 0);
	}
	else
		%this.set(%key, %value);
}

function MapObject::sub(%this, %key, %value)
{
	if (%this.__keys.contains(%key))
	{
		%old = %this.__value[%key] | 0;
		%this.set(%key, ((%old | 0) - (%value | 0)) | 0);
	}
	else
		%this.set(%key, %value);
}