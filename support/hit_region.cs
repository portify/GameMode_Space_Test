// exact player height = 2.65013

// datablock StaticShapeData(CubeData)
// {
// 	shapeFile = "Add-Ons/GameMode_Space_Test/shapes/cube/cube_glow.dts";
// };

// function serverCmdT(%client, %t)
// {
// 	%t = vectorAdd(%client.player.getPosition(), "0 0" SPC %t * 2.65013);

// 	if (isObject(%client.player.o))
// 		%client.player.o.delete();

// 	%client.player.o = new StaticShape()
// 	{
// 		datablock = CubeData;
// 		scale = "1.25 1.25 0";
// 		position = %t;
// 	};

// 	%client.player.o.setNodeColor("ALL", "1 0 1 0.4");
// }

// function test(%a, %b)
// {
// 	clientCmdCenterPrint(%a.getRegion(%b.position, 1), 3);
// 	schedule(100, 0, test, %a, %b);
// }

function Player::getHitRegion(%this, %position, %clamp)
{
	if (%this.isCrouched() || %this.isCorpse) // i give up
		return "";

	%local = %this.getPosition();

	%deltaZ = getWord(%position, 2) - getWord(%local, 2);
	%deltaZ = %deltaZ / getWord(%this.getScale(), 2) / 2.65013;

	if (%clamp)
		%deltaZ = getMin(1, getMax(0, %deltaZ));
	else if (%deltaZ < 0 || %deltaZ > 1)
		return "";

	if (%deltaZ > 0.7)
		return "head";

	%angle = mATan(
		getWord(%position, 1) - getWord(%local, 1),
		getWord(%position, 0) - getWord(%local, 0));

	%forwardVector = %this.getForwardVector();
	%angle -= mATan(getWord(%forwardVector, 1), getWord(%forwardVector, 0));

	if (%angle > $pi)
		%angle -= $pi * 2;

	if (%angle < -$pi)
		%angle += $pi * 2;

	if (%deltaZ < 0.25)
	{
		if (%angle < 0)
			return "rleg";
		return "lleg";
	}
	
	if (%angle > -$pi / 1.5 && %angle < $pi / 1.5)
	{
		if (%angle < -0.8)
			return "rarm";
		if (%angle > 0.8)
			return "larm";
	}

	return %deltaZ < 0.39 ? "hip" : "chest";
}