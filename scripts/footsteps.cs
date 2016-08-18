$FOOTSTEPS_INTERVAL = 150;
//$FOOTSTEPS_MIN_LANDING = -1.5;
$FOOTSTEPS_MIN_LANDING = 2.5;
$FOOTSTEPS_MIN_WALKING = 2.5;
$FOOTSTEPS_MIN_IMPACT = 15;
$FOOTSTEPS_BLOODYFOOTPRINTS = 1;
$FOOTSTEPS_MAXBLOODSTEPS = 30;

datablock AudioDescription(AudioPlayerFootstepLowProfile : AudioDefault3D)
{
	maxDistance = 30;
	referenceDistance = 10;
};

datablock AudioDescription(AudioPlayerFootstepHighProfile : AudioDefault3D)
{
	maxDistance = 50;
	referenceDistance = 10;
};

function discoverFootstepSounds()
{
	%pattern = "Add-Ons/GameMode_Space_Test/sounds/player/footsteps/*.wav";

	for (%file = findFirstFile(%pattern); %file !$= ""; %file = findNextFile(%pattern))
	{
		%data = strReplace(fileBase(%file), "_", "\t");

		if (getFieldCount(%data) < 2)
			continue;

		%type = getField(%data, 0);
		%material = getField(%data, 1);

		if (!(%type $= "jumpland" || %type $= "run" || %type $= "walk"))
		{
			echo("\c2ERROR: Invalid footstep sound type '" @ %type @ "' in " @ %file);
			continue;
		}

		%name = "FootstepSound_" @ fileBase(%file);
		%description = %type $= "walk" ? AudioPlayerFootstepLowProfile : AudioPlayerFootstepHighProfile;

		eval("datablock AudioProfile(" @ %name @ "){fileName=%file;description=%description;preload=1;};");

		if (!isObject(%name))
		{
			echo("\c2ERROR: Failed to create footstep datablock for " @ %file);
			continue;
		}

		if ($Footstep::Exists[%type, %material, %name])
			continue;

		$Footstep::Exists[%type, %material, %name] = 1;

		if ($Footstep::Count[%type, %material] $= "")
			$Footstep::Count[%type, %material] = 0;

		$Footstep::Profile[%type, %material, $Footstep::Count[%type, %material]] = %name;
		$Footstep::Count[%type, %material]++;
	}
}

if (!$DiscoveredFootstepSounds)
{
	$DiscoveredFootstepSounds = 1;
	discoverFootstepSounds();
}

function playFootstep(%type, %material, %position)
{
	if ($Footstep::Count[%type, %material] < 1)
		return 0;

	%index = getRandom($Footstep::Count[%type, %material] - 1);
	%data = $Footstep::Profile[%type, %material, %index];

	if (isObject(%data))
	{
		if (%position $= "")
			serverPlay2D(%data);
		else
			serverPlay3D(%data, %position);
	}
}

function Player::updateFootsteps(%this, %lastVert)
{
	cancel(%this.updateFootsteps);

	if (%this.getState() $= "Dead")
		return;

	%velocity = %this.getVelocity();

	%vert = getWord(%velocity, 2);
	%horiz = vectorLen(setWord(%velocity, 2, 0));

	if (-%lastVert >= $FOOTSTEPS_MIN_LANDING && %vert >= 0)
		%this.getDataBlock().onLand(%this, -%lastVert);

	if (%horiz >= $FOOTSTEPS_MIN_WALKING && !%this.isCrouched() && (!%this.getDataBlock().canJet || !%this.triggerState[4]))
	{
		if (!isEventPending(%this.playFootsteps))
			%this.playFootsteps(1);
	}
	else if (isEventPending(%this.playFootsteps))
		cancel(%this.playFootsteps);

	%this.updateFootsteps = %this.schedule(32, "updateFootsteps", %vert);
}

function Player::playFootsteps(%this, %foot)
{
	cancel(%this.playFootsteps);

	if (%this.getState() $= "Dead")
		return;

	%running = %this.getDataBlock().isRunning;

	%this.getDataBlock().onFootstep(%this, %foot, %running);
	%this.playFootsteps = %this.schedule(%running ? 230 : 300, "playFootsteps", !%foot); // 270
}

function Player::getFootPosition(%this, %foot)
{
	%base = %this.getPosition();
	%side = vectorCross(%this.getUpVector(), %this.getForwardVector());

	if (!%foot)
		%side = vectorScale(%side, -1);

	return vectorAdd(%base, vectorScale(%side, 0.4));
}

function Player::getFootObject(%this, %foot)
{
	%pos = %this.getFootPosition(%foot);

	return containerRayCast(
		vectorAdd(%pos, "0 0 0.1"),
		vectorSub(%pos, "0 0 1.1"),
		//$TypeMasks::All, %this
		$TypeMasks::FxBrickAlwaysObjectType
	);
}

function Armor::onLand(%this, %obj, %force)
{
	for (%i = 0; %i < 2; %i++)
	{
		%ray = %obj.getFootObject(%i);

		if (!%ray)
			continue;

		if (%obj.bloodyFootprints > 0)
		{
			%obj.doBloodyFootprint(%ray, %i, %obj.bloodyFootprints / %obj.bloodyFootprintsLast);
		}

		if (%force >= $FOOTSTEPS_MIN_IMPACT)
			%material = "heavy";
		else
			%material = %ray.getDataBlock().material;

		if (%material $= "")
			%material = "solidmetal";

		playFootstep("jumpland", %material, getWords(%ray, 1, 3));
	}
}

function Armor::onFootstep(%this, %obj, %foot, %running)
{
	%ray = %obj.getFootObject(%foot);

	if (!%ray)
		return;

	if($FOOTSTEPS_BLOODYFOOTPRINTS)
	{
		initContainerRadiusSearch(getWords(%ray, 1, 3), 0.4, $TypeMasks::ShapeBaseObjectType);
		while (%col = containerSearchNext())
		{
			if (%col.isBlood && %col.freshness >= 1)// && %col.sourceClient != %obj.client)
			{
				%obj.setBloodyFootprints(getMin(%obj.bloodyFootprints + %col.freshness * 3, $FOOTSTEPS_MAXBLOODSTEPS), %col.sourceClient); //Default freshness for splatter blood is 3, so 15 footsteps for fresh step.
				%col.freshness--; //Decrease blood freshness
				createBloodExplosion(getWords(%ray, 1, 3), %obj.getVelocity(), %col.getScale());
				serverPlay3d(bloodSpillSound, getWords(%ray, 1, 3));
				break;
			}
		}
		//if (!%obj.isCloaked && 0)
		if (%obj.bloodyFootprints > 0)
		{
			%obj.doBloodyFootprint(%ray, %foot, %obj.bloodyFootprints / %obj.bloodyFootprintsLast);
			%obj.bloodyFootprints--;
		}
	}

	%material = %ray.getDataBlock().material;

	if (%material $= "")
		%material = "solidmetal";

	playFootstep(%running ? "run" : "walk", %material, getWords(%ray, 1, 3));
}

package FootstepsPackage
{
	function Armor::onNewDataBlock(%this, %obj)
	{
		Parent::onNewDataBlock(%this, %obj);
		
		if (!isEventPending(%obj.updateFootsteps))
			%obj.updateFootsteps = %obj.schedule(0, "updateFootsteps");
	}

	function Armor::onTrigger(%this, %obj, %slot, %state)
	{
		Parent::onTrigger(%this, %obj, %slot, %state);
		%obj.triggerState[%slot] = %state ? 1 : 0;
	}

	function Armor::onImpact(%this, %obj, %col, %pos, %speed)
	{
		if (%speed >= $FOOTSTEPS_MIN_LANDING)
			%this.onLand(%obj, %speed);

		Parent::onImpact(%this, %obj, %col, %pos, %speed);
	}
};

activatePackage("FootstepsPackage");
JumpSound.fileName = "";