$DS::Blood::DripTimeOnBlood = 5;
$DS::Blood::CheckpointCount = 30;
$DS::Blood::dryTime = 5 * 60000; //5 minutes

function BloodDripProjectile::onCollision(%this, %obj, %col, %pos, %fade, %normal) {
	if(!isObject(%col)) return;
	if (%col.getType() & $TypeMasks::FxBrickObjectType && !%obj.isPaint) {
		initContainerRadiusSearch(
			%pos, 0.5,
			$TypeMasks::ShapeBaseObjectType
		);

		while (isObject( %find = containerSearchNext())) {
			if (%find.isBlood) {
				%find.delete();
				break;
			}
		}
		//why doesnt this work
		// %decal = spawnDecal(bloodDecal @ getRandom(1, 2), vectorAdd(%pos, vectorScale(%normal, 0.02)), %normal);
		// talk(%decal.getTransform());
	}

	if (%col.getType() & $TypeMasks::PlayerObjectType && !%obj.isPaint) {
		%col.startDrippingBlood($DS::Blood::DripTimeOnBlood);
	}
	%obj.explode();
}

function BloodDripProjectile::onCollision(%this, %obj, %col, %fade, %pos, %normal)
{
	if (%col.getType() & ($TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType))
	{
		initContainerRadiusSearch(%pos, 0.1,
			$TypeMasks::ShapeBaseObjectType);

		while (isObject(%col = containerSearchNext()))
		{
			if (!%col.isBlood || %col.getDataBlock() != pegprintDecal.getId())
				continue;
			%found = %col;
		}
		if (%found)
		{
			%decal = %found;
			%decal.alpha = getMin(%decal.alpha + 0.01, 1);
			%decal.setScale(vectorAdd(%decal.getScale(), "0.05 0.05 0.05"));//vectorMin(vectorAdd(%decal.getScale(), "0.05 0.05 0.05"),"5 5 5"));
			%decal.freshness += 0.05;
		}
		else
		{
			%decal = spawnDecal(pegprintDecal, %pos, %normal);
			%decal.setScale("1 1 1");
			%decal.setTransform(vectorAdd(%decal.getPosition(), "0 0 0.01"));
			%decal.alpha = 0.6;
			%decal.isBlood = true;
			%decal.sourceClient = %obj.client;
			%decal.spillTime = $Sim::Time;
			%decal.freshness = 0.5; //freshness < 1 means can't get bloody footprints from it
		}
		%decal.color = "0.7 0 0" SPC getMin(%decal.alpha, 1);
		%decal.setNodeColor("ALL", %decal.color);
	}
	%obj.explode();
	return;
}

function BloodDripProjectile::onExplode(%this, %obj, %pos)
{
	ServerPlay3D(bloodDripSound @ getRandom(1, 4), %pos);
}

function createBloodDripProjectile(%position, %size, %paint) {
	%obj = new Projectile() {
		dataBlock = BloodDripProjectile;

		initialPosition = %position;
		initialVelocity = "0 0 -2";
		isPaint = %paint;
	};

	MissionCleanup.add(%obj);
	GameRoundCleanup.add(%obj);

	if (%size !$= "") {
		%obj.setScale(%size SPC %size SPC %size);
	}

	return %obj;
}

function Player::startDrippingBlood(%this, %duration) {
	%duration = mClampF(%duration, 0, 60);
	%remaining = %this.dripBloodEndTime - $Sim::Time;

	if (%duration == 0 || (%this.dripBloodEndTime !$= "" && %duration < %remaining)) {
		return;
	}

	%this.dripBloodEndTime = $Sim::Time + %duration;

	if (!isEventPending(%this.dripBloodSchedule)) {
		%this.dripBloodSchedule = %this.schedule(getRandom(300, 800), dripBloodSchedule);
	}
}

function Player::stopDrippingBlood(%this) {
	%this.dripBloodEndTime = "";
	cancel(%this.dripBloodSchedule);
}

function Player::dripBloodSchedule(%this) {
	cancel(%this.dripBloodSchedule);

	if ($Sim::Time >= %this.dripBloodEndTime) {
		return;
	}

	%this.doDripBlood(true);
	%this.dripBloodSchedule = %this.schedule(getRandom(300, 800), dripBloodSchedule);
}

function Player::doDripBlood(%this, %force, %start, %end) {
	if (!%force && $Sim::Time - %this.lastBloodDrip <= 0.2) {
		return false;
	}
	if (%start $= "") {
		%start = vectorAdd(%this.position, "0 0 0.1");
	}
	if (%end $= "") {
		%end = vectorSub(%this.position, "0 0 0.1");
	}

	%ray = containerRayCast(%start, %end, $TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType);

	%this.lastBloodDrip = $Sim::Time;
	%decal = spawnDecalFromRay(%ray, BloodDecal @ getRandom(1, 2), 0.2 + getRandom() * 0.85);
	if (isObject(%decal)) {
		%decal.isBlood = true;
		%decal.color = "0.7 0 0 1";
		%decal.setNodeColor("ALL", %decal.color);
		%decal.sourceClient = %this.client;
		%decal.spillTime = $Sim::Time;
		%decal.freshness = 0.5;
		%decal.bloodDryingSchedule = schedule($DS::Blood::dryTime, 0, bloodDryingLoop, %decal);
	}

	return true;

	%this.lastBloodDrip = $Sim::Time;

	%x = getRandom() * 6 - 3;
	%y = getRandom() * 6 - 3;
	%z = 0 - (20 + getRandom() * 40);

	return true;
}

function Player::doSplatterBlood(%this, %amount, %pos) {
	if (%pos $= "") {
		%pos = %this.getHackPosition();
	}

	if (%amount $= "") {
		%amount = getRandom(15, 30);
	}

	%masks = $TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType;
	%spread = 0.25 + getRandom() * 0.25;

	for (%i = 0; %i < %amount; %i++) {
		%cross = vectorScale(vectorSpread("0 0 -1", %spread), 6);
		%stop = vectorAdd(%pos, %cross);

		%ray = containerRayCast(%pos, %stop, %masks);
		%scale = 0.6 + getRandom() * 0.85;
		%decal = spawnDecalFromRay(%ray, BloodDecal @ getRandom(1, 2), %scale);
		if(getWord(%ray, 1))
		{
			%decal.isBlood = true;
			%decal.color = "0.7 0 0 1";
			%decal.setNodeColor("ALL", %decal.color);
			%decal.sourceClient = %this.client;
			%decal.spillTime = $Sim::Time;
			%decal.freshness = 3; //Basically amount of times someone can step in blood
			%decal.bloodDryingSchedule = schedule($DS::Blood::dryTime, 0, bloodDryingLoop, %decal);
			// serverPlay3d(bloodSpillSound, getWords(%ray, 1, 3));
			createBloodExplosion(getWords(%ray, 1, 3), vectorNormalize(%this.getVelocity()), %scale SPC %scale SPC %scale);
			if(vectorDot("0 0 -1", %decal.normal) >= 0.5 && !isEventPending(%decal.ceilingBloodSchedule)) {
				if(getRandom(0, 3) == 3)
				{
					%decal.ceilingBloodSchedule = schedule(getRandom(16, 500), 0, ceilingBloodLoop, %decal, getWords(%ray, 1, 3));
				}
			}
		}
	}
}

function GameConnection::period(%this)
{
	messageAll('', '\c3%1 \c5is on their period.', %this.character.name);
	%this.player.period();
}

function Player::period(%this)
{
	cancel(%this.period);

	if (%this.getState() $= "Dead")
		return;

	%this.doSplatterBlood(1);
	%this.period = %this.schedule(25, "period");
}

function Player::doBloodyFootprint(%this, %ray, %foot, %alpha) {
	if(%alpha $= "")
		%alpha = 1;
	if(%alpha <= 0)
		return;
	%datablock = footprintDecal;
	// if(isObject(%this.client))
	// {
	// 	if(%foot)
	// 		%datablock = %this.client.lleg ? pegprintDecal : footprintDecal;
	// 	else
	// 		%datablock = %this.client.rleg ? pegprintDecal : footprintDecal;
	// }
	%decal = spawnDecalFromRay(%ray, %datablock, 0.3);
	%decal.setScale("1 1 1");
	%set = vectorAdd(%decal.getTransform(), "0 0 0.05");
	%decal.setTransform(%set SPC getWords(%this.getTransform(), 3, 7));

	//%decal.setNodeColor("ALL", %obj.client.murderPantsColor);
	%decal.color = "0.7 0 0" SPC %alpha;
	%decal.setNodeColor("ALL", %decal.color);
	%decal.isBlood = true;
	if(isObject(%this.bloodClient))
		%decal.sourceClient = %this.bloodClient;
	else
		%decal.sourceClient = %this.client;
	%decal.spillTime = $Sim::Time;
	%decal.freshness = 0.5; //freshness < 1 means can't get bloody footprints from it
	%decal.bloodDryingSchedule = schedule($DS::Blood::dryTime, 0, bloodDryingLoop, %decal);
	%decal.isFootprint = true;
}

function Player::setBloodyFootprints(%this, %val, %bloodclient)
{
	%this.bloodyFootprints = %val;
	%this.bloodyFootprintsLast = %val;
	%this.bloodClient = %bloodclient;
	%this.bloody["lshoe"] = true;
	%this.bloody["rshoe"] = true;
	if (%this.client)
	{
		%this.client.applyBodyParts();
		%this.client.applyBodyColors();
	}
}

function ceilingBloodLoop(%this, %pos, %paint) {
	cancel(%this.ceilingBloodSchedule);
	if(!isObject(%this))
	{
		return;
	}

	if(!%this.driptime) {
		%this.driptime = 500;
	}

	%this.driptime = %this.driptime + 10;
	if(%this.driptime > 3000) {
		return;
	}

	if(%pos $= "") {
		return;
	}

	createBloodDripProjectile(%pos, "", %paint);
	%this.ceilingBloodSchedule = schedule(%this.driptime, 0, ceilingBloodLoop, %this, %pos, %paint);
}

function bloodDryingLoop(%this) {
	cancel(%this.bloodDryingSchedule);
	if(!isObject(%this)) return;
	if(%this.freshness <= 0)
	{
		%this.color = vectorMax("0 0 0", vectorSub(getWords(%this.color, 0, 2), "0.1 0.1 0.1")) SPC getWord(%this.color, 3);
		%this.setNodeColor("ALL", %this.color); //Make it appear darker
		return;
	}
	%this.freshness--;
	%this.bloodDryingSchedule = schedule($DS::Blood::dryTime, 0, bloodDryingLoop, %this);
}

function createBloodExplosion(%position, %velocity, %scale) {
	%datablock = bloodExplosionProjectile @ getRandom(1, 2);
	%obj = new Projectile() {
		dataBlock = %datablock;

		initialPosition = %position;
		initialVelocity = %velocity;
	};

	MissionCleanup.add(%obj);

	%obj.setScale(vectorMin(%scale, "1 1 1"));
	%obj.explode();
}

function createBloodSplatterExplosion(%position, %velocity, %scale) {
	%datablock = bloodExplosionProjectile3;
	%obj = new Projectile() {
		dataBlock = %datablock;

		initialPosition = %position;
		initialVelocity = %velocity;
	};

	MissionCleanup.add(%obj);

	%obj.setScale(vectorMin(%scale, "1 1 1"));
	%obj.explode();
}

//
//Datablocks below
//

datablock AudioProfile(BloodSpillSound) {
	fileName = "Add-Ons/GameMode_Space_Test/sounds/physics/blood_Spill.wav";
	description = AudioSilent3D;
	preload = true;
};

datablock AudioProfile(BloodDripSound1) {
	fileName = "Add-Ons/GameMode_Space_Test/sounds/physics/blood_drip1.wav";
	description = AudioSilent3D;
	preload = true;
};
datablock AudioProfile(BloodDripSound2) {
	fileName = "Add-Ons/GameMode_Space_Test/sounds/physics/blood_drip2.wav";
	description = AudioSilent3D;
	preload = true;
};
datablock AudioProfile(BloodDripSound3) {
	fileName = "Add-Ons/GameMode_Space_Test/sounds/physics/blood_drip3.wav";
	description = AudioSilent3D;
	preload = true;
};
datablock AudioProfile(BloodDripSound4) {
	fileName = "Add-Ons/GameMode_Space_Test/sounds/physics/blood_drip4.wav";
	description = AudioSilent3D;
	preload = true;
};

datablock StaticShapeData(footprintDecal)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/decals/footprint.dts";
};

datablock StaticShapeData(pegprintDecal)
{
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/decals/pegprint.dts";
};

datablock staticShapeData(BloodDecal1) {
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/decals/blood1.dts";

	doColorShift = true;
	colorShiftColor = "0.7 0 0 1";
};

datablock staticShapeData(BloodDecal2) {
	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/decals/blood2.dts";

	doColorShift = true;
	colorShiftColor = "0.7 0 0 1";
};

datablock ParticleData(bloodParticle)
{
	dragCoefficient		= 3.0;
	windCoefficient		= 0.2;
	gravityCoefficient	= 0.2;
	inheritedVelFactor	= 0;
	constantAcceleration	= 0.0;
	lifetimeMS		= 500;
	lifetimeVarianceMS	= 10;
	spinSpeed		= 40.0;
	spinRandomMin		= -50.0;
	spinRandomMax		= 50.0;
	useInvAlpha		= true;
	animateTexture		= false;
	//framesPerSec		= 1;

	textureName		= "Add-Ons/GameMode_Space_Test/shapes/blood2.png";
	//animTexName		= " ";

	// Interpolation variables
	colors[0]	= "0.7 0 0 1";
	colors[1]	= "0.7 0 0 0";
	sizes[0]	= 0.4;
	sizes[1]	= 2;
	//times[0]	= 0.5;
	//times[1]	= 0.5;
};

datablock ParticleEmitterData(bloodEmitter)
{
	ejectionPeriodMS = 3;
	periodVarianceMS = 0;

	ejectionVelocity = 0; //0.25;
	velocityVariance = 0; //0.10;

	ejectionOffset = 0;

	thetaMin         = 0.0;
	thetaMax         = 90.0;

	particles = bloodParticle;

	useEmitterColors = true;
	uiName = "";
};

datablock ExplosionData(bloodExplosion)
{
	//explosionShape = "";
	//soundProfile = bulletHitSound;
	lifeTimeMS = 300;

	particleEmitter = bloodEmitter;
	particleDensity = 5;
	particleRadius = 0.2;
	//emitter[0] = bloodEmitter;

	faceViewer     = true;
	explosionScale = "1 1 1";
};

datablock ProjectileData(bloodExplosionProjectile1)
{
	directDamage        = 0;
	impactImpulse	     = 0;
	verticalImpulse	  = 0;
	explosion           = bloodExplosion;
	particleEmitter     = bloodEmitter;

	muzzleVelocity      = 50;
	velInheritFactor    = 1;

	armingDelay         = 0;
	lifetime            = 2000;
	fadeDelay           = 1000;
	bounceElasticity    = 0.5;
	bounceFriction      = 0.20;
	isBallistic         = true;
	gravityMod = 0.1;

	hasLight    = false;
	lightRadius = 3.0;
	lightColor  = "0 0 0.5";
};



datablock ParticleData(bloodParticle2)
{
	dragCoefficient		= 3.0;
	windCoefficient		= 0.1;
	gravityCoefficient	= 0.3;
	inheritedVelFactor	= 1;
	constantAcceleration	= 0.0;
	lifetimeMS		= 300;
	lifetimeVarianceMS	= 10;
	spinSpeed		= 20.0;
	spinRandomMin		= -10.0;
	spinRandomMax		= 10.0;
	useInvAlpha		= true;
	animateTexture		= false;
	//framesPerSec		= 1;

	textureName		= "Add-Ons/GameMode_Space_Test/shapes/blood3.png";
	//animTexName		= " ";

	// Interpolation variables
	colors[0]	= "0.7 0 0 1";
	colors[1]	= "0.7 0 0 0";
	sizes[0]	= 1;
	sizes[1]	= 0;
	//times[0]	= 0.5;
	//times[1]	= 0.5;
};

datablock ParticleEmitterData(bloodEmitter2)
{
	ejectionPeriodMS = 5;
	periodVarianceMS = 0;

	ejectionVelocity = 0; //0.25;
	velocityVariance = 0; //0.10;

	ejectionOffset = 0;

	thetaMin         = 0.0;
	thetaMax         = 90.0;

	particles = bloodParticle2;

	useEmitterColors = true;
	uiName = "";
};

datablock ExplosionData(bloodExplosion2)
{
	//explosionShape = "";
	//soundProfile = bulletHitSound;
	lifeTimeMS = 300;

	particleEmitter = bloodEmitter2;
	particleDensity = 5;
	particleRadius = 0.2;
	//emitter[0] = bloodEmitter;

	faceViewer     = true;
	explosionScale = "1 1 1";
};

datablock ProjectileData(bloodExplosionProjectile2)
{
	directDamage        = 0;
	impactImpulse	     = 0;
	verticalImpulse	  = 0;
	explosion           = bloodExplosion2;
	particleEmitter     = bloodEmitter2;

	muzzleVelocity      = 50;
	velInheritFactor    = 1;

	armingDelay         = 0;
	lifetime            = 2000;
	fadeDelay           = 1000;
	bounceElasticity    = 0.5;
	bounceFriction      = 0.20;
	isBallistic         = true;
	gravityMod = 0.1;

	hasLight    = false;
	lightRadius = 3.0;
	lightColor  = "0 0 0.5";
};

datablock ParticleEmitterData(bloodEmitter3)
{
	ejectionPeriodMS = 10;
	periodVarianceMS = 0;

	ejectionVelocity = 8;
	velocityVariance = 2;
	orientParticles = 0;
	ejectionOffset = 0.2;

	thetaMin         = 0;
	thetaMax         = 25;

	particles = bloodParticle;

	useEmitterColors = true;
	uiName = "";
};

datablock ExplosionData(bloodExplosion3)
{
	//explosionShape = "";`
	//soundProfile = bulletHitSound;
	lifeTimeMS = 65;

	particleEmitter = "";
	particleDensity = 0.2;
	particleRadius = 30;
	emitter[0] = "bloodEmitter3";

	faceViewer     = true;
	explosionScale = "1 1 1";
};

datablock ProjectileData(bloodExplosionProjectile3)
{
	directDamage        = 0;
	impactImpulse	     = 0;
	verticalImpulse	  = 0;
	explosion           = bloodExplosion3;

	muzzleVelocity      = 50;
	velInheritFactor    = 1;

	armingDelay         = 0;
	lifetime            = 2000;
	fadeDelay           = 1000;
	bounceElasticity    = 0.5;
	bounceFriction      = 0.20;
	isBallistic         = true;
	gravityMod = 0.1;

	hasLight    = false;
	lightRadius = 3.0;
	lightColor  = "0 0 0.5";
};

datablock ParticleData(bloodDripParticle)
{
	dragCoefficient		= 1.0;
	windCoefficient		= 0.1;
	gravityCoefficient	= 0.5;
	inheritedVelFactor	= 1;
	constantAcceleration	= 0.0;
	lifetimeMS		= 200;
	lifetimeVarianceMS	= 0;
	spinSpeed		= 20.0;
	spinRandomMin		= -10.0;
	spinRandomMax		= 10.0;
	useInvAlpha		= true;
	animateTexture		= false;
	//framesPerSec		= 1;

	textureName		= "base/data/particles/dot.png";
	//animTexName		= " ";

	// Interpolation variables
	colors[0]	= "0.7 0 0 1";
	colors[1]	= "0.7 0 0 0.8";
	sizes[0]	= 0.1;
	sizes[1]	= 0;
	//times[0]	= 0.5;
	//times[1]	= 0.5;
};

datablock ParticleEmitterData(bloodDripEmitter)
{
	ejectionPeriodMS = 1;
	periodVarianceMS = 0;

	ejectionVelocity = 0; //0.25;
	velocityVariance = 0; //0.10;

	ejectionOffset = 0;

	thetaMin         = 0.0;
	thetaMax         = 90.0;

	particles = bloodDripParticle;

	useEmitterColors = true;
	uiName = "";
};

datablock ProjectileData(BloodDripProjectile)
{
	directDamage        = 0;
	impactImpulse	     = 0;
	verticalImpulse	  = 0;
	explosion           = bloodExplosion2;
	particleEmitter     = bloodDripEmitter;

	muzzleVelocity      = 60;
	velInheritFactor    = 1;

	armingDelay         = 3000;
	lifetime            = 3000;
	fadeDelay           = 2000;
	bounceElasticity    = 0.5;
	bounceFriction      = 0.20;
	isBallistic         = true;
	gravityMod = 1;

	hasLight    = false;
	lightRadius = 3.0;
	lightColor  = "0 0 0.5";
};
