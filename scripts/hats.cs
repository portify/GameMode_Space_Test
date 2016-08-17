function HatItem::onPickup()
{
}

function Player::dropHat(%this)
{
	%item = $HatItem[%this.currHat];

	if (isObject(%item))
	{
		%drop = new Item()
		{
			datablock = %item;
			dropper = %this.client;
		};

		MissionCleanup.add(%drop);

		%drop.setCollisionTimeout(%this);
		%drop.setScopeAlways();
		%drop.setTransform(%this.getEyePoint());
		%drop.setVelocity(getRandom(-4, 4) SPC getRandom(-4, 4) SPC 6);
		%drop.schedulePop();

		%this.mountHat("");
	}
}

function Player::mountHat(%this, %hat)
{
	for (%i = 0; $hat[%i] !$= ""; %i++)
		%this.hideNode($hat[%i]);

	for (%i = 0; $accent[%i] !$= ""; %i++)
		%this.hideNode($accent[%i]);

	if (%hat !$= "")
		%image = $HatImage[%hat];

	if (isObject(%image))
	{
		%this.mountImage(%image, 2);
		%this.currHat = %hat;
	}
	else
	{
		%this.unMountImage(2);
		%this.currHat = "";
	}
}

function registerHat(%name, %dir, %offset, %eyeOffset)
{
	%rotation = eulerToMatrix("0 0 0");
	%scale = "0.1 0.1 0.1";

	if (!isObject($HatImage[%name]))
	{
		eval(
			"datablock ShapeBaseImageData(TemporaryHatImage){" @
			"shapeFile=%dir;mountPoint=$HeadSlot;offset=%offset;eyeOffset=%eyeOffset;" @
			"rotation=%rotation;scale=%scale;hatName=%name;};");

		%image = nameToID("TemporaryHatImage");

		if (!isObject(%image))
		{
			error("ERROR: Unable to register image for hat '" @ %name @ "'");
			return 1;
		}

		%image.setName("");
		$HatImage[%name] = %image;
	}

	if (!isObject($HatItem[%name]))
	{
		eval(
			"datablock ItemData(TemporaryHatItem){" @
			"className=\"HatItem\";shapeFile=%dir;mass=1;density=0.2;" @
			"elasticity=0.2;friction=0.6;emap=1;};");

		%item = nameToID("TemporaryHatItem");

		if (!isObject(%item))
		{
			error("ERROR: Unable to register item for hat '" @ %name @ "'");
			return 1;
		}

		%item.setName("");
		$HatItem[%name] = %item;
	}

	if (!$HatExists[%name])
	{
		$HatExists[%name] = 1;
		$HatName[$HatCount] = %name;
		$HatCount++;
	}

	return 0;
}

function scanHats()
{
	%pattern = "Add-Ons/GameMode_Space_Test/hats/*/*.dts";

	for (%file = findFirstFile(%pattern); %file !$= ""; %file = findNextFile(%pattern))
	{
		%path = filePath(%file);
		%base = fileBase(%file);

		%offset = "0 0 0";
		%eyeOffset = "0 0 -1000";

		if (isFile(%path @ ".txt"))
		{
			echo("need to implement config file reading");
			continue;
		}

		registerHat(%base, %file, %offset, %eyeOffset);
	}
}

function HatMod_GetProperties(%configDir) {
	%offset = "0 0 0";
	%minVer = "0";
	%eyeOffset = "0 0 -1000";

	if(%configDir !$= "Default") {
		%file = new fileObject();
		%file.openForRead(%configDir);

		while(!%file.isEOF()) {
			%line = trim(%file.readLine());

			if(%line $= "")
				continue;

			%firstWord = getWord(%line, 0);
			%wordCount = getWordCount(%line);

			switch$(%firstWord) {
			case "offset":
				%offset = getWords(%line, %wordCount-3, %wordCount-1);
				%offset = strReplace(%offset, "\"", "");
				%offset = strReplace(%offset, ";", "");
				%offset = vectorAdd(%offset, "0 0 0"); //Forces it to be a 3d vector

			case "minVer":
				%minVer = getWord(%line, getWordCount(%line)-1);

			case "eyeOffset":
				%eyeOffset = getWords(%line, %wordCount-3, %wordCount-1);
				%eyeOffset = strReplace(%eyeOffset, "\"", "");
				%eyeOffset = strReplace(%eyeOffset, ";", "");
				%eyeOffset = vectorAdd(%eyeOffset, "0 0 0"); //Forces it to be a 3d vector
			}
		}

		%file.close();
		%file.delete();
	}

	return %offset TAB %minVer TAB %eyeOffset;
}

if ($HatCount $= "")
{
	$HatCount = 0;
	scanHats();
}