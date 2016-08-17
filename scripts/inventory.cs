datablock ItemData(EmptyRightHand)
{
	iconName = "";
	uiName = "Right (Empty)";
};

datablock ItemData(EmptyLeftHand)
{
	iconName = "";
	uiName = "Left (Empty)";
};

datablock ItemData(BackpackMenu)
{
	iconName = "Add-Ons/GameMode_Space_Test/icons/backpack";
	uiName = "Backpack";
};

datablock ItemData(ClothingMenu)
{
	iconName = "";
	uiName = "Clothing";
};

datablock ItemData(DropToolMenu)
{
	iconName = "";
	uiName = "Drop";
};

datablock ItemData(ToBackpackMenu)
{
	iconName = "";
	uiName = "To Backpack";
};

datablock ItemData(ToClothingMenu)
{
	iconName = "";
	uiName = "To Clothing";
};

$INVMODE_HAND = 0;
$INVMODE_PROPS_HAND = 1;
$INVMODE_INV = 2;
$INVMODE_PROPS_INV = 3;
$INVMODE_CLOTH = 4;
$INVMODE_PROPS_CLOTH = 5;

function Player::invInit(%this)
{
	if (!isObject(%this.client.miniGame.spaceRound))
		return;

	%this.invSetMode($INVMODE_HAND, 0);
}

function Player::invSetMode(%this, %mode, %a, %b)
{
	if (!isObject(%client = %this.client))
		return;

	switch (%mode)
	{
		case $INVMODE_HAND:
			commandToClient(%client, 'PlayGui_CreateToolHud', 4);
			schedule(0, 0, commandToClient, %client, 'SetScrollMode', 2);
			messageClient(%client, 'MsgItemPickup', '', 0, isObject(%this.invHand[0]) ? %this.invHand[0].getID() : EmptyRightHand.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 1, isObject(%this.invHand[1]) ? %this.invHand[1].getID() : EmptyLeftHand.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 2, BackpackMenu.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 3, ClothingMenu.getID(), 1);
			commandToClient(%client, 'SetActiveTool', %a);
		case $INVMODE_PROPS_HAND:
			%this.invOrigin = %a;
			commandToClient(%client, 'PlayGui_CreateToolHud', 2);
			schedule(0, 0, commandToClient, %client, 'SetScrollMode', 2);
			messageClient(%client, 'MsgItemPickup', '', 0, ToBackpackMenu.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 1, ToClothingMenu.getID(), 1);
			commandToClient(%client, 'SetActiveTool', 0);
		case $INVMODE_INV:
			commandToClient(%client, 'PlayGui_CreateToolHud', 5);
			schedule(0, 0, commandToClient, %client, 'SetScrollMode', 2);
			for (%i = 0; %i < 5; %i++)
				messageClient(%client, 'MsgItemPickup', '', %i, %this.invBackpack[%i], 1);
			commandToClient(%client, 'SetActiveTool', %a);
		case $INVMODE_PROPS_INV:
			%this.invOrigin = %a;
			commandToClient(%client, 'PlayGui_CreateToolHud', 3);
			schedule(0, 0, commandToClient, %client, 'SetScrollMode', 2);
			messageClient(%client, 'MsgItemPickup', '', 0, EmptyRightHand.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 1, EmptyLeftHand.getID(), 1);
			messageClient(%client, 'MsgItemPickup', '', 2, ToClothingMenu.getID(), 1);
			commandToClient(%client, 'SetActiveTool', 0);
	}

	%this.invMode = %mode;
}

package SpaceInventory
{
	function serverCmdDropTool(%client)
	{
		if (!isObject(%round = %client.miniGame.spaceRound) || !isObject(%player = %client.player))
			return Parent::serverCmdLight(%client);

		%client.centerPrint("\c6dropping items from inventory is not implemented", 2);
	}

	function serverCmdUnUseTool(%client)
	{
		if (!isObject(%round = %client.miniGame.spaceRound) || !isObject(%player = %client.player))
			return Parent::serverCmdLight(%client);

		switch (%player.invMode)
		{
			case $INVMODE_HAND:
			case $INVMODE_PROPS_HAND: %player.invSetMode($INVMODE_HAND, %player.invOrigin);
			case $INVMODE_INV: %player.invSetMode($INVMODE_HAND, 2);
			case $INVMODE_PROPS_INV: %player.invSetMode($INVMODE_INV, %player.invOrigin);
		}
	}

	function serverCmdUseTool(%client, %slot)
	{
		if (!isObject(%round = %client.miniGame.spaceRound) || !isObject(%player = %client.player))
			return Parent::serverCmdLight(%client);

		switch (%player.invMode)
		{
			case $INVMODE_HAND: %max = 4;
			case $INVMODE_PROPS_HAND: %max = 2;
			case $INVMODE_INV: %max = 5;
			case $INVMODE_PROPS_INV: %max = 3;
		}

		if (%slot >= 0 && %slot < %max && %slot $= mFloor(%slot))
		{
			%player.invSlot = %slot;

			if (%player.invMode == $INVMODE_HAND && %slot < 3)
				%player.activeHand = %slot;
		}
	}

	function serverCmdLight(%client)
	{
		if (!isObject(%round = %client.miniGame.spaceRound) || !isObject(%player = %client.player))
			return Parent::serverCmdLight(%client);

		switch$ (%player.invMode)
		{
			case $INVMODE_HAND:
				if (%player.invSlot == 3)
					%client.centerPrint("\c6this thing isn't done you dumbass", 2);
				else if (%player.invSlot == 2)
					%player.invSetMode($INVMODE_INV, 0);
				else
					%player.invSetMode($INVMODE_PROPS_HAND, %player.invSlot);
			case $INVMODE_PROPS_HAND:
				if (%player.invSlot == 0)
				{
					for (%i = 0; %i < 5; %i++)
					{
						if (!isObject(%player.invBackpack[%i]))
							break;
					}

					if (%i != 5)
					{
						%player.invBackpack[%i] = %player.invHand[%player.invOrigin];
						%player.invBackpackProps[%i] = %player.invHandProps[%player.invOrigin];
						%player.invSetMode($INVMODE_INV, %i);
					}
					else
						%client.centerPrint("\c6Your backpack is full.", 2);
				}
				else
					%client.centerPrint("\c6this thing isn't done you dumbass", 2);
			case $INVMODE_INV:
				if (isObject(%player.invBackpack[%player.invSlot]))
					%player.invSetMode($INVMODE_PROPS_INV, %player.invSlot);
			case $INVMODE_PROPS_INV:
				if (%player.invSlot == 2)
					%client.centerPrint("\c6this thing isn't done you dumbass", 2);
				else
				{
					%hand = %player.invHand[%player.invSlot];
					%handProps = %player.invHandProps[%player.invSlot];

					%player.invHand[%player.invSlot] = %player.invBackpack[%player.invOrigin];
					%player.invHandProps[%player.invSlot] = %player.invBackpackProps[%player.invOrigin];
					%player.invBackpack[%player.invOrigin] = %hand;
					%player.invBackpackProps[%player.invOrigin] = %handProps;

					%player.invSetMode($INVMODE_HAND, %player.invSlot);
				}
		}
	}

	function Armor::onRemove(%this, %obj)
	{
		for (%i = 0; %i < 2; %i++)
		{
			if (isObject(%props = %obj.invHandProps[%i]))
				%props.delete();
		}
		
		for (%i = 0; %i < 5; %i++)
		{
			if (isObject(%props = %obj.invBackpackProps[%i]))
				%props.delete();
		}

		Parent::onRemove(%this, %obj);
	}

	function ItemData::onPickup(%this, %obj, %player, %amount)
	{
		if (!isObject(%round = %obj.spaceRound))
			return Parent::onPickup(%this, %obj, %player, %amount);

		if (!%obj.canPickup || %player.isCorpse || !isObject(%player.spaceRound) || %round != %player.spaceRound)
			return;

		%region = $INVMODE_HAND;

		for (%i = 0; %i < 2; %i++)
		{
			if (!isObject(%player.invHand[%i]))
				break;
		}

		if (%i == 2)
		{
			%region = $INVMODE_INV;

			for (%i = 0; %i < 5; %i++)
			{
				if (!isObject(%player.invHand[%i]))
					break;
			}

			if (%i == 5)
				return;
		}

		%props = %obj.props;
		%obj.props = "";

		if (%obj.isStatic())
			%obj.respawn();
		else
			%obj.delete();

		if (%region == $INVMODE_HAND)
		{
			%player.invHand[%i] = %this;
			%player.invHandProps[%i] = %props;
		}
		else if (%region == $INVMODE_INV)
		{
			%player.invBackpack[%i] = %this;
			%player.invBackpackProps[%i] = %props;
		}

		if (%player.invMode == %region)
			%player.invSetMode(%region, %player.invSlot);
	}
};

activatePackage("SpaceInventory");