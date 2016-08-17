datablock AudioProfile(SplashTextSound)
{
	fileName = "Add-Ons/GameMode_Space_Test/sounds/ui/splash_text.wav";
	description = AudioDefault3D;
	preload = 1;
};

datablock AudioProfile(TargetAcquireSound)
{
	fileName = "Add-Ons/GameMode_Space_Test/sounds/ui/target_acquire.wav";
	description = AudioDefault3D;
	preload = 1;
};

function FxDTSBrick::onDoorPlant(%this)
{
	%this.isSpaceDoor = 1;
	%this.accessFlagCount = 0;
}

function FxDTSBrick::spaceDoor(%this, %client, %props)
{
	%data = %this.getDataBlock();

	if (%data.isOpen)
	{
		if (!isObject(%props))
			%this.doorClose(%client);

		return;
	}

	if (%this.accessFlagCount)
	{
		if (!isObject(%props))
		{
			if (isObject(%client))
				%client.centerPrint("\c6This door requires an ID card.", 2);

			return;
		}

		for (%i = 0; %i < %props.accessFlagCount; %i++)
		{
			if (%props.accessFlag[%i] == -1)
			{
				%allAccess = 1;
				break;
			}
			else
				%hasAccess[%props.accessFlag[%i]] = 1;
		}

		%missingCount = 0;

		if (!%allAccess)
		{
			for (%i = 0; %i < %this.accessFlagCount; %i++)
			{
				if (!%hasAccess[%this.accessFlag[%i]])
				{
					%missing[%missingCount] = %this.accessFlag[%i];
					%missingCount++;
				}
			}
		}

		if (%missingCount)
		{
			if (isObject(%client))
			{
				%text = "\c3You lack the following access permission" @ (%missingCount == 1 ? "" : "s") @ " to open this door:\n";

				for (%i = 0; %i < %missingCount; %i++)
					%text = %text @ "\c6" @ $AccessFlagName[%missing[%i]] @ "\n";

				%client.centerPrint(%text, 2);
			}

			%this.playSound(SplashTextSound);
			return;
		}
	}

	%this.doorOpen(0, %client);
}

package SpaceDoors
{
	function FxDTSBrick::onPlant(%this)
	{
		Parent::onPlant(%this);
		%data = %this.getDataBlock();

		if (%data.isDoor)
		{
			%this.onDoorPlant();

			if (!%data.skipDoorEvents)
				// this is so hacky and bad
				%data.numEvents = 0;
		}
	}
	
	function FxDTSBrick::onLoadPlant(%this)
	{
		Parent::onLoadPlant(%this);

		if (%this.getDataBlock().isDoor)
			%this.onDoorPlant();
	}

	function serverCmdClearEvents(%client)
	{
		Parent::serverCmdClearEvents(%client);

		if (!isObject(%brick = %client.wrenchBrick))
			return;

		if (%brick.isSpaceDoor)
		{
			for (%i = 0; %i < %brick.accessFlagCount; %i++)
				%brick.accessFlag[%i] = "";

			%brick.accessFlagCount = 0;
		}
	}

	function serverCmdAddEvent(%client, %enabled, %inputID, %delay, %targetID, %ntID, %outputID, %p0, %p1, %p2, %p3)
	{
		Parent::serverCmdAddEvent(%client, %enabled, %inputID, %delay, %targetID, %ntID, %outputID, %p0, %p1, %p2, %p3);

		if (!isObject(%brick = %client.wrenchBrick))
			return;

		if ($InputEvent_Name["FxDTSBrick", %inputID] !$= "Space")
			return;
		
		%target = getWord(getField($InputEvent_TargetList["FxDTSBrick", %inputID], %targetID), 1);
		%output = $OutputEvent_Name[%target, %outputID];

		if (%target $= "Space_Doors" && %brick.isSpaceDoor)
		{
			if (%output $= "AccessFlag")
			{
				%brick.accessFlag[%brick.accessFlagCount] = %p0;
				%brick.accessFlagCount++;
			}
		}
	}
};

activatePackage("SpaceDoors");