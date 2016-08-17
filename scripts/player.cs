datablock PlayerData(PlayerSpaceArmor : PlayerStandardArmor)
{
	uiName = "Space Player";
	mass = 150;

	canJet = 0;
	maxTools = 10;

	runForce = 4320;
	jumpForce = 1540;

	firstPersonOnly = 1;
	thirdPersonOnly = 0;
	jumpDelay = 30;
};

datablock PlayerData(PlayerSpaceCorpseArmor : PlayerStandardArmor)
{
	uiName = "";
	boundingBox = PlayerSpaceArmor.crouchBoundingBox;
};

datablock PlayerData(PlayerSpaceRunningArmor : PlayerSpaceArmor)
{
	uiName = "";
	isRunning = 1;

	showEnergyBar = 0;
	maxForwardSpeed = 10.5;

	runForce = 6000;
	jumpForce = 864;

	minRunEnergy = 2.5;
	minJumpEnergy = 20;

	runEnergyDrain = 2;
	jumpEnergyDrain = 20;

	firstPersonOnly = 1;
	thirdPersonOnly = 0;
	jumpDelay = 10;
};

function PlayerSpaceArmor::onTrigger(%this, %obj, %slot, %state)
{
	Parent::onTrigger(%this, %obj, %slot, %state);

	if (%slot == 4 && %state)
	{
		%obj.setDataBlock(PlayerSpaceRunningArmor);
		%obj.monitorEnergyLevel();
	}
}

function PlayerSpaceRunningArmor::onTrigger(%this, %obj, %slot, %state)
{
	Parent::onTrigger(%this, %obj, %slot, %state);

	if (%slot == 4 && !%state)
	{
		%obj.setDataBlock(PlayerSpaceArmor);
		%obj.monitorEnergyLevel();
	}
}

function Player::monitorEnergyLevel(%this, %last)
{
	cancel(%this.monitorEnergyLevel);

	if (%this.getState() $= "Dead" || !isObject(%this.client))
		return;

	%show = %this.getEnergyLevel() < %this.getDataBlock().maxEnergy;

	if (%show != %last)
		commandToClient(%this.client, 'ShowEnergyBar', %show);

	%this.monitorEnergyLevel = %this.schedule(100, "monitorEnergyLevel", %show);
}

function Player::examinePlayer(%this, %client)
{
	%count = %this.clothing.getCount();
	%masked = 0;

	for (%i = 0; %i < %count; %i++)
	{
		%item = %this.clothing.getObject(%i);

		if (%item.type $= "Hat" && %item.hatName $= "SkiMask")
			%masked = 1;
	}

	%char = %this.character;

	if (%masked)
		%name = "an unknown masked person";
	else
		%name = %char.realName;

	if (%char.gender)
	{
		%they_upper = "He";
		%them_upper = "Him";
		%their_upper = "His";
	}
	else
	{
		%they_upper = "She";
		%them_upper = "Her";
		%their_upper = "Her";
	}

	%they_lower = strlwr(%they_upper);
	%them_lower = strlwr(%them_upper);
	%their_lower = strlwr(%their_upper);

	%msg = "<color:BAFF16>This is " @ %name @ ". ";

	if (%this.tool0 == IDItem.getID())
		%msg = %msg @ %their_upper @ " ID card shows the position of " @ %this.getToolProps(0).job @ ".";
	else
		%msg = %msg @ %they_upper @ " does not have an ID card.";

	messageClient(%client, '', %msg);

	for (%i = 0; %i < %count; %i++)
	{
		if (%i != 0 && %i == %count - 1)
			%wearing = %wearing @ " and ";
		else if (%i != 0)
			%wearing = %wearing @ ", ";

		%item = %this.clothing.getObject(%i);
		%wearing = %wearing @ %item.desc;

		if (%item.bloody)
			%wearing = %wearing @ " <color:FFAAAA>stained with blood\c6";
	}

	if (%wearing !$= "")
		messageClient(%client, '', "\c6" @ %they_upper @ " is wearing " @ %wearing @ ".");

	%item = %this.getMountedImage(0).item;

	if (%item.uiName !$= "")
		messageClient(%client, '', "\c6" @ %they_upper @ " is holding a(n) \c3" @ %item.uiName @ "\c6.");

	if (%this.ambiguousGender)
		messageClient(%client, '', "<color:FFFFBB>" @ %they_upper @ " has a strange " @ (%char.gender ? "feminine" : "masculine") @ " quality to " @ %them_lower @ ".");

	if (%this.isCorpse)
	{
		if (%this.isDead)
			messageClient(%client, '', "<color:FF5544>" @ %they_upper @ " is limp and unresponsive; there are no signs of life.");
		else
			messageClient(%client, '', "<color:FFBB44>" @ %they_upper @ " isn't responding to anything around " @ %their_lower @ " and seems to be asleep.");
	}
	else if (!isObject(%this.client))
		messageClient(%client, '', "<color:BBBBBB>" @ %they_upper @ " has a vacant, braindead stare...");

	if (%this.suicided)
		messageClient(%client, '', "<color:FFBB44>" @ %they_upper @ " appears to have committed suicide... there is no hope of recovery.");

	if (%dmg = %this.getTypeDamage($DMG_BRUTE))
	{
		if (%dmg < 30)
			messageClient(%client, '', "<color:FFBB44>" @ %they_upper @ " has minor bruisings.");
		else
			messageClient(%client, '', "<color:FF5544>" @ %they_upper @ " has severe bruisings!");
	}

	if (%dmg = %this.getTypeDamage($DMG_BURN))
	{
		if (%dmg < 30)
			messageClient(%client, '', "<color:FFBB44>" @ %they_upper @ " has minor burns.");
		else
			messageClient(%client, '', "<color:FF5544>" @ %they_upper @ " has severe burns!");
	}

	if (%dmg = %this.getTypeDamage($DMG_SHARP))
	{
		if (%dmg < 30)
			messageClient(%client, '', "<color:FFBB44>" @ %they_upper @ " has minor cuts.");
		else
			messageClient(%client, '', "<color:FF5544>" @ %they_upper @ " has severe cuts!");
	}

	if (isEventPending(%this.bleedTick))
		messageClient(%client, '', "<color:FF5544>" @ %they_upper @ " is bleeding!");
}

function serverCmdSuccumb(%client)
{
	%camera = %client.camera;

	if (!isObject(%camera = %client.camera) || !isObject(%corpse = %camera.getOrbitObject()) || !%corpse.isCorpse)
		return;

	if (%corpse.isDead || %corpse.originalClient != %client)
		return;

	%corpse.isDead = 1;
	%client.centerPrint("\c6You have succumbed to death. You can now spectate.", 2);
}

function SimSet::getIndexOf(%this, %object)
{
	if (!isObject(%object) || !%this.isMember(%object))
		return -1;

	%count = %this.getCount();

	for (%i = 0; %i < %count; %i++)
	{
		if (%this.getObject(%i) == %object)
			return %i;
	}

	return -1;
}

if (!isFunction("Armor", "onAdd"))
	eval("function Armor::onAdd(){}");

package STPlayerPackage
{
	function GameConnection::applyBodyParts(%this)
	{
		if (isObject(%this.player.character))
			%this.player.applyCharacter();
		else
			Parent::applyBodyParts(%this);
	}

	function GameConnection::applyBodyColors(%this)
	{
		if (isObject(%this.player.character))
			%this.player.applyCharacter();
		else
			Parent::applyBodyColors(%this);
	}

	// function Armor::damage(%this, %obj, %source, %position, %damage, %type)
	// {
	// 	if (%obj.isCorpse)
	// 	{
	// 		if (!%obj.isDead && %damage > 0)
	// 		{
	// 			if ((%obj.health -= %damage) < (%obj.maxHealth * -2))
	// 				%obj.isDead = 1;
	// 		}

	// 		return;
	// 	}

	// 	%round = %obj.spaceRound;

	// 	if (!isObject(%round))
	// 		return Parent::damage(%this, %obj, %source, %position, %damage, %type);

	// 	if (%obj.isCrouched() && $Damage::Direct[%type])
	// 		%damage = %damage * 2.1 * 0.75;

	// 	%damage /= getWord(%obj.getScale(), 2);

	// 	if (%damage <= 0)
	// 		return;

	// 	%obj.lastDamageTime = getSimTime();
	// 	%obj.lastDamagePos = %position;

	// 	if (getSimTime() - %obj.lastDamageTime > 300)
	// 		%obj.painLevel = 0;
		
	// 	%obj.painLevel += %damage;
	// 	%obj.health -= %damage;

	// 	%flash = %obj.getDamageFlash() + (%damage / %obj.maxHealth) * 2;

	// 	if (%flash > 0.75)
	// 		%flash = 0.75;

	// 	%obj.setDamageFlash(%flash);
	// 	%painThreshold = 7;

	// 	if (%this.painThreshold !$= "")
	// 		%painThreshold = %this.painThreshold;

	// 	if (%damage > %painThreshold)
	// 		%obj.playPain();

	// 	if (!%this.useCustomPainEffects)
	// 	{
	// 		if (%obj.painLevel >= 40)
	// 			%obj.emote(PainHighImage, 1);
	// 		else if (%obj.painLevel >= 25)
	// 			%obj.emote(PainMidImage, 1);
	// 		else
	// 			%obj.emote(PainLowImage, 1);
	// 	}

	// 	if (%obj.health > 0)
	// 		return;

	// 	if (isObject(%source.sourceObject))
	// 		%culprit = %source.sourceObject.character;
	// 	else if (%source.getClassName() $= "Player")
	// 		%culprit = %source.character;
	// 	else if (%source.getType() & $TypeMasks::VehicleObjectType)
	// 		%culprit = %source.getControllingClient().player.character;

	// 	if (isObject(%culprit))
	// 		%round.onKill(%culprit, %obj.character);

	// 	%obj.playDeathCry();
	// 	%obj.setDamageFlash(0.75);

	// 	if (isEventPending(%obj.lavaSchedule))
	// 	{
	// 		cancel(%obj.lavaSchedule);
	// 		%obj.lavaSchedule = 0;
	// 	}

	// 	%vehicle = %obj.getObjectMount();
	// 	%mask = $TypeMasks::VehicleObjectType | $TypeMasks::PlayerObjectType;

	// 	if (isObject(%vehicle) && (%vehicle.getType() & %mask))
	// 	{
	// 		%vehicle.onDriverLeave(%obj);
	// 		%obj.unMount();
	// 	}

	// 	%obj.setImageTrigger(0, 0);
	// 	%count = %obj.getMountedObjectCount();

	// 	for (%i = 0; %i < %count; %i++)
	// 	{
	// 		%rider = %obj.getMountedObject(%i);
	// 		%rider.getDataBlock().doDismount(%rider, 1);
	// 	}

	// 	%client = %obj.client;

	// 	%obj.originalClient = %client;
	// 	%obj.client = "";

	// 	%obj.isCorpse = 1;
	// 	%obj.isDead = %obj.health < (%obj.maxHealth * -2);

	// 	%obj.playThread(1, "death1");

	// 	%client.player = "";

	// 	%round.playerSet.remove(%obj);
	// 	%round.corpseSet.add(%obj);

	// 	if (isObject(%client))
	// 	{
	// 		%client.setControlObject(%client.camera);
	// 		%client.camera.setControlObject(%client.camera);
	// 		%client.camera.setMode("Corpse", %obj);
	// 	}

	// 	%round.schedule(0, "checkEnd");
	// }

	function Observer::onTrigger(%this, %obj, %slot, %state)
	{
		%client = %obj.getControllingClient();

		if (!isObject(%round = %client.miniGame.spaceRound))
			return Parent::onTrigger(%this, %obj, %slot, %state);

		%orbit = %obj.getOrbitObject();

		if (%orbit.isCorpse && !%orbit.isDead && %orbit.originalClient == %client)
		{
			%client.centerPrint("\c6You are not dead yet, so you cannot spectate.\n\c6Say \c3/succumb \c6to succumb to death.", 2);
			return;
		}

		if (!%state)
			return;

		if (%obj.mode $= "Observer")
		{
			if (%slot == 2 && %round.playerSet.getCount())
				%obj.setMode("Corpse", %round.playerSet.getObject(0));
		}
		else
		{
			if (%slot == 2)
				%obj.setMode("Observer");
			else if (%slot == 0 || %slot == 4)
			{
				%count = %round.playerSet.getCount();

				if (!%count)
					%obj.setMode("Observer");
				else
				{
					%index = %round.playerSet.getIndexOf(%obj.getOrbitObject());

					if (%slot == 0)
						%index = (%index + 1) % %count;
					else
					{
						%index--;

						if (%index < 0)
							%index += %count;
					}

					%obj.setMode("Corpse", %round.playerSet.getObject(%index));
				}
			}
		}

		if (%obj.mode $= "Observer")
			%text = "<just:center>\c6Free mode\n";
		else if (%obj.mode $= "Corpse")
		{
			if (isObject(%orbit = %obj.getOrbitObject()))
				%text = "<just:center>\c6Following \c3" @ %orbit.character.realName @ "\n";
			else
				%text = "<just:center>\c3Follow mode";
		}
		else
			%text = "";

		commandToClient(%client, 'BottomPrint', %text, 0, 1);
		//Parent::onTrigger(%this, %obj, %slot, %state);

		if (%slot == 4 && %state && 0)
		{
			if (isObject($DefaultMiniGame.spaceRound.player[%client.getBLID()]))
				$DefaultMiniGame.spaceRound.player[%client.getBLID()].delete();

			%client.spaceJoinRound(%client.miniGame, %client.miniGame.spaceRound);
		}
	}

	function Player::activateStuff(%this)
	{
		if (!isObject(%this.spaceRound))
			return Parent::activateStuff(%this);

		if (!isObject(%this.client))
			return;

		%point = %this.getEyePoint();
		%vector = vectorScale(%this.getEyeVector(), 6);

		%ray = containerRayCast(
			%point, vectorAdd(%point, %vector),
			$TypeMasks::ItemObjectType |
			$TypeMasks::PlayerObjectType |
			$TypeMasks::FxBrickObjectType,
			%this);

		if (!%ray)
			return;

		%type = %ray.getType();

		if (%type & $TypeMasks::FxBrickObjectType)
		{
			if (%ray.isSpaceDoor)
				%ray.spaceDoor(%this.client);
			else
			{
				$InputTarget_Self = %ray.getID();
				$InputTarget_Player = %this;
				$InputTarget_Client = %this.client;

				%ray.processInputEvent("onActivate", %this.client);
			}
		}
		else if (%type & $TypeMasks::ItemObjectType)
		{
			if (isObject(%ray.spaceRound) && %ray.canPickup)
			{
				%i = %max = %this.getDataBlock().maxTools;
				%data = %ray.getDataBlock();

				if (isObject(%data.replaceOnPickup))
				{
					for (%i = 0; %i < %max; %i++)
					{
						if (%this.tool[%i] == %data.replaceOnPickup.getID())
							break;
					}
				}
				
				if (%i == %max)
				{
					for (%i = 0; %i < %max; %i++)
					{
						if (!isObject(%this.tool[%i]))
							break;
					}
				}

				if (%i != %max)
				{
					%props = %ray.props;
					%ray.props = "";

					if (%ray.isStatic())
						%ray.respawn();
					else
						%ray.delete();

					%this.setTool(%i, %data, %props);
				}
			}
		}
		else if ((%type & $TypeMasks::PlayerObjectType) && isObject(%ray.character) && isObject(%ray.clothing))
		{
			%ray.examinePlayer(%this.client);
		}
	}

	function serverCmdSuicide(%client)
	{
		%client.player.suicided = 1;
		Parent::serverCmdSuicide(%client);
	}

	function Armor::onEnterLiquid(%this,%player,%a,%b)
	{
		Parent::onEnterLiquid(%this,%player,%a,%b);
	}

	function Armor::whileInWaterBrick(%this,%player,%a,%b)
	{
		// Parent::whileInWaterBrick(%this,%player,%a,%b);
	}
};

activatePackage("STPlayerPackage");