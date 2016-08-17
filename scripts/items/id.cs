datablock ItemData(IDItem)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/id/id.dts";
	emap = 1;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	
	uiName = "ID Card";
	iconName = "Add-Ons/GameMode_Space_Test/shapes/id/icon_id";

	image = IDImage;
	canDrop = 1;

	replaceOnDrop = NoIDItem;
	replaceOnPickup = NoIDItem;
};

datablock ItemData(NoIDItem)
{
	shapeFile = "base/data/shapes/empty.dts";

	uiName = "No ID";
	iconName = "Add-Ons/GameMode_Space_Test/shapes/id/icon_no_id";

	doColorShift = 1;
	colorShiftColor = "0.5 0.3 0.3 1";
};

datablock ShapeBaseImageData(IDImage)
{
	className = "WeaponImage";

	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/id/id.dts";
	emap = 1;

	offset = "0 0 0";
	mountPoint = 0;
	correctMuzzleVector = 0;
	melee = 1;

	item = IDItem;
	armReady = 0;

	stateName[0]						= "Activate";
	stateTimeoutValue[0]				= 0.25;
	stateTransitionOnTimeout[0]			= "Ready";
	stateAllowImageChange[0]			= 0;

	stateName[1]						= "Ready";
	stateTransitionOnTriggerDown[1]		= "Use";
	stateAllowImageChange[1]			= 1;

	stateName[2]						= "Use";
	stateScript[2]						= "onUse";
	stateAllowImageChange[2]			= 0;
	stateTimeoutValue[2]				= 0.7;
	stateWaitForTimeout[2]				= 1;
	stateTransitionOnTriggerUp[2]		= "Ready";
};

function Player::addAccessFlag(%this, %flag)
{
	%props = %this.getToolProps(0);

	if (!isObject(%props))
		return;

	for (%i = 0; %i < %props.accessFlagCount; %i++)
	{
		if (%props.accessFlag[%i] == %flag)
			return;
	}

	%props.accessFlag[%props.accessFlagCount] = %flag;
	%props.accessFlagCount++;
}

function IDImage::onMount(%this, %obj, %slot)
{
	if (!isObject(%obj.client))
		return;

	%props = %obj.getToolProps();

	%text = "<color:FFFFBB>Nanotrasen - Standard ID\n";
	%text = %text @ "\c6" @ %props.name @ " (" @ (%props.gender ? "Male" : "Female") @ ", " @ %props.age @ " years)\n";
	%text = %text @ "\c6" @ %props.job @ "\n";
	
	commandToClient(%obj.client, 'CenterPrint', %text);
}

function IDImage::onUnMount(%this, %obj, %slot)
{
	commandToClient(%obj.client, 'ClearCenterPrint');
}

function IDImage::onUse(%this, %obj, %slot)
{
	%props = %obj.getToolProps();

	if (!isObject(%props))
	{
		if (isObject(%obj.client))
			%obj.client.centerPrint("\c6Your ID card is faulty.", 2);

		return;
	}

	%point = %obj.getEyePoint();
	%vector = vectorScale(%obj.getEyeVector(), 6);

	%ray = containerRayCast(%point, vectorAdd(%point, %vector), $TypeMasks::FxBrickObjectType);

	if (!%ray)
		return;

	if (%ray.isSpaceDoor)
	{
		if (!%ray.accessFlagCount)
		{
			if (isObject(%obj.client))
				%obj.client.centerPrint("\c6This door does not use an ID card scanner.", 2);

			return;
		}

		if (%ray.getDataBlock().isOpen)
			%ray.spaceDoor(%obj.client, %props);
		else
		{
			%ray.playSound(TargetAcquireSound);

			%obj.playThread(1, "armReadyRight");
			%obj.schedule(500, "playThread", 1, "root");
			%ray.schedule(650, "spaceDoor", %obj.client, %props);
		}
	}
}

datablock ItemData(TraitorPDAItem)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/id/id.dts";
	emap = 1;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	
	uiName = "Traitor PDA";
	iconName = "Add-Ons/GameMode_Space_Test/shapes/id/icon_id";

	image = TraitorPDAImage;
	canDrop = 1;

	doColorShift = 1;
	colorShiftColor = "1 0 0 1";
};

datablock ShapeBaseImageData(TraitorPDAImage)
{
	className = "WeaponImage";

	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/id/id.dts";
	emap = 1;

	offset = "0 0 0";
	mountPoint = 0;
	correctMuzzleVector = 0;
	melee = 1;

	item = TraitorPDAItem;
	armReady = 0;
};

function TraitorPDAImage::onMount(%this, %obj, %slot)
{
	if (!isObject(%obj.client))
		return;

	%props = %obj.getToolProps();
	commandToClient(%obj.client, 'CenterPrint', %props.text);
}

function TraitorPDAImage::onUnMount(%this, %obj, %slot)
{
	commandToClient(%obj.client, 'ClearCenterPrint');
}