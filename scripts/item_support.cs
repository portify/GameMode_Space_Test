function Player::setTool(%this, %slot, %tool, %props, %flag)
{
	if (%slot < 0 || %slot >= %this.getDataBlock().maxTools)
		return;

	%tool = isObject(%tool) ? %tool.getID() : 0;

	if (isObject(%this.toolProps[%slot]))
		%this.toolProps[%slot].delete();

	%this.tool[%slot] = %tool;
	%this.toolProps[%slot] = %props;

	if (isObject(%client = %this.client))
		messageClient(%client, 'MsgItemPickup', '', %slot, %tool, %flag);
}

function Player::getToolProps(%this, %slot)
{
	if (%slot $= "")
		%slot = %this.currTool;

	if (%slot < 0 || %slot >= %this.getDataBlock().maxTools)
		return 0;

	if (isObject(%props = %this.toolProps[%slot]))
		return %props;

	return %this.toolProps[%slot] = new ScriptObject();
}

if (!isFunction("ItemData", "onRemove"))
	eval("function ItemData::onRemove(){}");

package SpaceItems
{
	function Player::clearTools(%this, %client)
	{
		%maxTools = %this.getDataBlock().maxTools;

		for (%i = 0; %i < %maxTools; %i++)
		{
			if (isObject(%props = %this.toolProps[%i]))
				%props.delete();
		}

		Parent::clearTools(%this, %client);
	}

	function Armor::onNewDataBlock(%this, %obj)
	{
		%max = %obj.getDataBlock().maxTools;
		Parent::onNewDataBlock(%this, %obj); // don't actually know if it changes before this function triggers at all

		for (%i = %this.maxTools; %i < %max; %i++)
		{
			if (isObject(%props = %obj.toolProps[%i]))
				%props.delete();
		}
	}

	function Armor::onRemove(%this, %obj)
	{
		for (%i = 0; %i < %this.maxTools; %i++)
		{
			if (isObject(%props = %obj.toolProps[%i]))
				%props.delete();
		}

		Parent::onRemove(%this, %obj);
	}

	function ItemData::onRemove(%this, %obj)
	{
		if (isObject(%obj.props))
			%obj.props.delete();

		Parent::onRemove(%this, %obj);
	}

	function ItemData::onPickup(%this, %obj, %player, %parent)
	{
		if (!isObject(%round = %obj.spaceRound))
			return Parent::onPickup(%this, %obj, %player);
	}

	function serverCmdDropTool(%client, %slot)
	{
		if (isObject(%round = %client.miniGame.spaceRound))
		{
			if (!(isObject(%player = %client.player) && isObject(%tool = %player.tool[%slot])))
				return;

			if (!%tool.canDrop)
				return;

			%scale = getWord(%player.getScale(), 2);

			%muzzlePoint = vectorAdd(%player.getPosition(), "0 0" SPC 1.5 * %scale);
			%muzzleVector = %player.getEyeVector();

			%item = new Item()
			{
				datablock = %tool;
				spaceRound = %round;
				props = %player.toolProps[%slot];
				miniGame = %client.miniGame;
				bl_id = %client.getBLID();
			};

			MissionCleanup.add(%item);

			%item.setTransform(vectorAdd(%muzzlePoint, %muzzleVector) SPC rotFromTransform(%player.getTransform()));
			%item.setVelocity(vectorScale(%muzzleVector, 10 * %scale));
			%item.setCollisionTimeout(%player);

			%item.setShapeName(%tool.uiName);
			%item.setShapeNameDistance(20);

			if (%tool.doColorShift)
				%item.setShapeNameColor(%tool.colorShiftColor);

			%round.itemSet.add(%item);

			%player.toolProps[%slot] = "";
			%player.setTool(%slot, %replace = %tool.replaceOnDrop, "", 1);

			if (isObject(%image = %tool.image))
			{
				%mountPoint = %image.mountPoint;

				if (%mountPoint $= "")
					%mountPoint = 0;

				if (%player.getMountedImage(%mountPoint) == %image.getID())
				{
					if (isObject(%replace.image))
						%player.mountImage(%replace.image, %mountPoint);
					else
						%player.unMountImage(%mountPoint);
				}
			}

			return;
		}

		Parent::serverCmdDropTool(%client, %slot);
	}
};

activatePackage("SpaceItems");