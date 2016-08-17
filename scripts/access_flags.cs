deleteVariables("$AccessFlag*");
$AccessFlagCount = 0;

function addAccessFlag(%id, %name)
{
	$AccessFlag[$AccessFlagCount] = %id;
	$AccessFlagCount++;

	$AccessFlagName[%id] = %name;
	$AccessFlagID[%name] = %id;
}

function registerAccessFlags()
{
	for (%i = 0; %i < $AccessFlagCount; %i++)
		%list = %list SPC $AccessFlagName[$AccessFlag[%i]] SPC $AccessFlag[%i];

	registerOutputEvent("Space_Doors", "AccessFlag", "list" @ %list);
}

addAccessFlag(0, "Custodian");
addAccessFlag(1, "Server");
addAccessFlag(2, "Cargo");
addAccessFlag(3, "Command");
addAccessFlag(4, "Security");

registerAccessFlags();