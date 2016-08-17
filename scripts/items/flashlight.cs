$FlashlightRange = 50;
$FlashlightRate = 50;
$FlashlightSpeed = 0.54;

datablock AudioProfile(FlashlightToggleSound)
{
   fileName = "Add-Ons/GameMode_Space_Test/sounds/toggle_flashlight.wav";
   description = AudioClose3D;
   preload = 1;
};

datablock FxLightData(PlayerFlashlight : PlayerLight)
{
	uiName = "";
	flareOn = 0;

	radius = 16;
	brightness = 3;
};

datablock ItemData(FlashlightItem)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/flashlight.dts";
	emap = 1;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	
	uiName = "Flashlight";
	canDrop = 1;

	doColorShift = 1;
	colorShiftColor = "0.3 0.3 0.35 1";
};

datablock ShapeBaseImageData(FlashlightImage)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/flashlight.dts";
	//hasLight = 1;

	offset = "0 0 0";
	mountPoint = 1;

	doColorShift = 1;
	colorShiftColor = "0.3 0.3 0.35 1";

	// lightType = "ConstantLight";
	// lightColor = "1 1 1 1";
	// lightTime = "1000";
	// lightRadius = "10";
};

function Player::flashlightTick(%this)
{
	cancel(%this.flashlightTick);

	if (!isObject(%this.light))
		return;

	%start = %this.getMuzzlePoint(1);
	%vector = %this.getEyeVector();

	%range = $FlashlightRange;

	if ($EnvGuiServer::VisibleDistance !$= "")
	{
		%limit = $EnvGuiServer::VisibleDistance / 2;

		if (%range > %limit)
			%range = %limit;
	}

	%end = vectorAdd(%start, vectorScale(%vector, %range));
	%end = vectorAdd(%end, %this.getVelocity());

	%mask = 0;

	%mask |= $TypeMasks::StaticShapeObjectType;
	%mask |= $TypeMasks::FxBrickObjectType;
	%mask |= $TypeMasks::VehicleObjectType;
	%mask |= $TypeMasks::TerrainObjectType;
	%mask |= $TypeMasks::PlayerObjectType;

	%ray = containerRayCast(%start, %end, %mask, %this);

	if (%ray)
		%pos = vectorAdd(getWords(%ray, 1, 3), getWords(%ray, 4, 6));
	else
		%pos = %end;

	%path = vectorSub(%pos, %this.light.position);
	%length = vectorLen(%path);

	if (%length > $FlashlightSpeed)
	{
		%moved = vectorScale(%path, $FlashlightSpeed);
		%pos = vectorAdd(%this.light.position, %moved);
	}

	%this.light.setTransform(%pos);
	%this.light.reset();

	%this.flashlightTick = %this.schedule($FlashlightRate, "flashlightTick");
}

function pushServerPackageToBack(%package)
{
	%i = $numClientPackages $= "" ? 0 : $numClientPackages;
	%c = getNumActivePackages();

	for (%i = getNumActivePackages() - 1; %i >= $numClientPackages; %i--)
	{
		%current = getActivePackage(%i);

		if (%current !$= %package)
		{
			%stack = ltrim(%stack SPC %current);
			deactivatePackage(%current);
		}
	}

	if (%stack !$= "")
	{
		for (%i = getWordCount(%stack) - 1; %i >= 0; %i--)
			activatePackage(getWord(%stack, %i));
	}
}

package FlashlightPackage
{
	function serverCmdDropTool(%client, %slot)
	{
		if (isObject(%player = %client.player) && %player.tool[%slot] == FlashlightItem.getID() && isObject(%player.light))
		{
			%player.lastLightTime = -250;
			serverCmdLight(%client);
		}

		Parent::serverCmdDropTool(%client, %slot);
	}

	function serverCmdLight(%client)
	{
		if (!isObject(%client.miniGame.spaceRound))
			return Parent::serverCmdLight(%client);

		%player = %client.player;

		if (!isObject(%player) || %player.getState() $= "Dead")
			return Parent::serverCmdLight(%client);

		%maxTools = %player.getDataBlock().maxTools;

		for (%i = 0; %i < %maxTools; %i++)
		{
			if (%player.tool[%i] == FlashlightItem.getID())
				break;
		}

		if (%i == %maxTools)
			return;

		if (getSimTime() - %player.lastLightTime < 250)
			return;

		%player.lastLightTime = getSimTime();
		serverPlay3D(FlashlightToggleSound, %player.getHackPosition());

		if (isObject(%player.light))
		{
			%player.light.delete();

			if (%player.getMountedImage(1) == nameToID("FlashlightImage"))
				%player.unMountImage(1);
		}
		else
		{
			if (!isObject(%player.getMountedImage(1)))
				%player.mountImage("flashLightImage", 1);

			%player.light = new FxLight()
			{
				datablock = PlayerFlashlight;
				player = %player;

				iconSize = 1;
				enable = 1;
			};

			missionCleanup.add(%player.light);
			%player.light.setTransform(%player.getTransform());

			if (!isEventPending(%player.flashlightTick))
				%player.flashlightTick();
		}
	}

	function serverCmdGreenLight(%client)
	{
		// %player = %client.player;

		// if (!isObject(%player) || %player.getState() $= "Dead") {
		// 	parent::serverCmdGreenLight(%client);
		// 	return;
		// }
	}

	function Player::unmountImage(%this, %slot)
	{
		parent::unmountImage(%this, %slot);

		if (%slot == 1 && isObject(%this.light))
			%this.mountImage(FlashlightImage, 1);
	}
};

activatePackage("FlashlightPackage");
// pushServerPackageToBack("FlashlightPackage");