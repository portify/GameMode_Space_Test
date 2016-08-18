$LAST_DMG = "";
$DMG_BRUTE = $LAST_DMG++;
$DMG_BURN = $LAST_DMG++;
$DMG_SHARP = $LAST_DMG++;
$DMG_TOXIN = $LAST_DMG++;

function Player::initDamage(%this)
{
	%this.usesDamage = 1;

	%this.organs = new ScriptGroup()
	{
		new ScriptObject() { class = "PlayerOrgan"; name = "chest";                       maxDamage = 200; };
		new ScriptObject() { class = "PlayerOrgan"; name =  "head";                       maxDamage = 200; };
		new ScriptObject() { class = "PlayerOrgan"; name =  "larm"; uiName =  "left arm"; maxDamage =  75; };
		new ScriptObject() { class = "PlayerOrgan"; name =  "rarm"; uiName = "right arm"; maxDamage =  75; };
		new ScriptObject() { class = "PlayerOrgan"; name =  "lleg"; uiName =  "left leg"; maxDamage =  75; };
		new ScriptObject() { class = "PlayerOrgan"; name =  "rleg"; uiName = "right leg"; maxDamage =  75; };
	};

	%this.internalOrgans = new ScriptGroup()
	{
		new ScriptObject() { name =    "brain"; };
		new ScriptObject() { name =    "heart"; };
		new ScriptObject() { name = "appendix"; };
	};

	%count = %this.organs.getCount();

	for (%i = 0; %i < %count; %i++)
		%this.organs.getObject(%i).owner = %this;
}

function PlayerOrgan::damage(%this, %type, %damage)
{
	%total = %this.getTotalDamage();

	if (%total + %damage > %this.maxDamage)
		%damage = getMin(%this.maxDamage - %total, %damage);
	
	if (%damage > 0)
		%this.damage[%type] += %damage;
}

function PlayerOrgan::heal(%this, %type, %damage)
{
	if (%damage > 0)
		%this.damage[%type] = getMax(0, %this.damage[%type] - %damage);
}

function PlayerOrgan::canTakeDamage(%this)
{
	return %this.getTotalDamage() < %this.maxDamage;
}

function PlayerOrgan::isDamaged(%this)
{
	return %this.getTotalDamage() > 0;
}

function PlayerOrgan::getTotalDamage(%this)
{
	%total = 0;

	for (%i = 1; %i <= $LAST_DMG; %i++)
		%total += %this.damage[%i];

	return %total;
}

function Player::getTotalDamage(%this)
{
	%total = 0;
	%count = %this.organs.getCount();

	for (%i = 0; %i < %count; %i++)
		%total += %this.organs.getObject(%i).getTotalDamage();

	return %total;
}

function Player::getTypeDamage(%this, %type)
{
	%total = 0;
	%count = %this.organs.getCount();

	for (%i = 0; %i < %count; %i++)
		%total += %this.organs.getObject(%i).damage[%type];

	return %total;
}

package SpaceDamage
{
	function Armor::onRemove(%this, %obj)
	{
		if (%obj.usesDamage)
		{
			%obj.organs.delete();
			%obj.internalOrgans.delete();
		}
		
		Parent::onRemove(%this, %obj);
	}

	function Armor::damage(%this, %obj, %source, %position, %damage, %type)
	{
		if (!%obj.usesDamage)
			return Parent::damage(%this, %obj, %source, %position, %damage, %type);

		%damageType = $Damage::Type[%type];

		if ($Damage::Direct[%type])
		{
			if (%position !$= "")
			{
				%region = %obj.getHitRegion(%position);

				if (%region $= "hip")
					%region = "chest";
			}

			if (%obj.isCrouched())
				%damage *= 1.5;
		}

		if (%damageType $= "")
			%damageType = $DMG_BRUTE;

		%count = %obj.organs.getCount();
		%split = 0;

		for (%i = 0; %i < %count; %i++)
		{
			%organ = %obj.organs.getObject(%i);

			if ((%region $= "" || %organ.name $= %region) && %organ.canTakeDamage())
				%split++;
		}

		// should consider applying all to a random damageable organ instead
		%organDamage = %damage / %split;

		for (%i = 0; %i < %count; %i++)
		{
			%organ = %obj.organs.getObject(%i);

			if ((%region $= "" || %organ.name $= %region) && %organ.canTakeDamage())
				%organ.damage(%damageType, %organDamage);
		}

		%health = 100 - %obj.getTotalDamage();
		%obj.lastDamageTime = getSimTime();

		if (!%obj.isCorpse)
		{
			if (getSimTime() - %obj.lastDamageTime > 300)
				%obj.painLevel = 0;
			
			%obj.painLevel += %damage;

			%flash = %obj.getDamageFlash() + (%damage / %obj.maxHealth) * 2;

			if (%flash > 0.75)
				%flash = 0.75;

			%obj.setDamageFlash(%flash);

			if (%damage > 7)
				%obj.playPain();

			if (%obj.painLevel >= 40)
				%obj.emote(PainHighImage, 1);
			else if (%obj.painLevel >= 25)
				%obj.emote(PainMidImage, 1);
			else
				%obj.emote(PainLowImage, 1);

			createBloodSplatterExplosion(%position, vectorNormalize(vectorSub(%position, %source.getEyePoint())), "1 1 1");
			%obj.doSplatterBlood(%randMax, %position, %vector, %type == $DamageType::Sharp ? 45 : 180);
		}
		else
		{
			if (!%obj.isDead && %health <= -100)
				%obj.isDead = 1;

			return;
		}

		if (%health > 0)
			return;

		%obj.setDataBlock(PlayerSpaceCorpseArmor);
		%obj.isCorpse = 1;
		//%obj.isDead = %obj.health < (%obj.maxHealth * -2);
		//%obj.isDead = 1;
		%obj.isDead = %health <= -100;

		%obj.playDeathCry();
		%obj.playDeathAnimation();
		%obj.setArmThread("land");
		%obj.setActionThread("root");
		%obj.setDamageFlash(0.75);
		%obj.setShapeNameDistance(0);

		if (isObject(SpaceRound))
		{
			if (isObject(%source.sourceObject))
				%culprit = %source.sourceObject.character;
			else if (%source.getClassName() $= "Player")
				%culprit = %source.character;
			else if (%source.getType() & $TypeMasks::VehicleObjectType)
				%culprit = %source.getControllingClient().player.character;

			if (isObject(%culprit))
				SpaceRound.onKill(%culprit, %obj.character);

			SpaceRound.playerSet.remove(%obj);
			SpaceRound.corpseSet.add(%obj);

			SpaceRound.schedule(0, "checkEnd");
		}

		// if (isObject(%vehicle = %obj.getObjectMount()))
		// {
		// 	%vehicle.onDriverLeave(%obj);
		// 	%obj.unmount();
		// }

		%obj.setImageTrigger(0, 0);
		%count = %obj.getMountedObjectCount();

		for (%i = 0; %i < %count; %i++)
		{
			%rider = %obj.getMountedObject(%i);
			%rider.getDataBlock().doDismount(%rider, 1);
		}

		if (isObject(%client = %obj.client))
		{
			%client.player = "";
			%obj.originalClient = %client;

			%client.setControlObject(%client.camera);
			%client.camera.setControlObject(%client.camera);
			%client.camera.setMode("Corpse", %obj);
		}
	}
};

activatePackage("SpaceDamage");