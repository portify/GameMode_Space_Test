function Array()
{
	return new ScriptObject()
	{
		__ref = 0;
		class = "ArrayObject";
		length = 0;
	};
}

function Array::from(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19)
{
	for (%length = 20; %length > 0; %length--)
	{
		if (%a[%length - 1] !$= "")
			break;
	}

	%array = new ScriptObject()
	{
		__ref = 0;
		class = "ArrayObject";
		length = %length;
	};

	for (%i = 0; %i < %length; %i++)
		%array.value[%i] = ref(%a[%i]);

	return %array;
}

function ArrayObject::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		unref(%this.value[%i]);
}

function ArrayObject::_set(%this, %i, %value)
{
	unref(%this.value[%i]);
	%this.value[%i] = ref(%value);

	if (isFunction(%this.setCallback))
		call(%this.setCallback, %this, %i, %value);
}

function ArrayObject::copy(%this)
{
	%array = new ScriptObject()
	{
		__ref = 0;
		class = "ArrayObject";
		length = %this.length;
	};

	for (%i = 0; %i < %this.length; %i++)
		%array._set(%i, %this.value[%i]);

	return %array;
}

function ArrayObject::clear(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		%this._set(%i, "");

	%this.length = 0;
}

function ArrayObject::append(%this, %value)
{
	%this._set(%this.length, %value);
	%this.length++;
}

function ArrayObject::insert(%this, %index, %value)
{
	%index = mClamp(%index, 0, %this.length);

	for (%i = %this.length; %i > %index; %i++)
	{
		%this._set(%i, %this.value[%i - 1]);
	}

	%this._set(%index, %value);
	%this.length++;
}

function ArrayObject::pop(%this, %index)
{
	%index = mClamp(%index, 0, %this.length);
	%value = %this.value[%index];

	%this.length--;

	for (%i = %index; %i < %this.length; %i++)
		%this._set(%i, %this.value[%i + 1]);

	%this._set(%this.length, "");
	return unref(%value);
}

function ArrayObject::find(%this, %value)
{
	for (%i = 0; %i < %this.length; %i++)
	{
		if (%this.value[%i] $= %value)
			return %i;
	}

	return -1;
}

function ArrayObject::concat(%this, %array)
{
	for (%i = 0; %i < %array.length; %i++)
	{
		%this._set(%this.length, %array.value[%i]);
		%this.length++;
	}
}

function ArrayObject::contains(%this, %value)
{
	return %this.find(%value) != -1;
}

function ArrayObject::remove(%this, %value)
{
	%index = %this.find(%value);

	if (%index == -1)
		return 0;

	%this.pop(%index);
	return 1;
}

function ArrayObject::count(%this, %value)
{
	%count = 0;

	for (%i = 0; %i < %this.length; %i++)
	{
		if (%this.value[%i] $= %value)
			%count++;
	}

	return %count;
}

function ArrayObject::swap(%this, %i, %j)
{
	%temp = %this.value[%i];
	%this._set(%i, %this.value[%j]);
	%this._set(%j, %temp);
}

function ArrayObject::reverse(%this)
{
	%max = mFloor((%this.length - 1) / 2);

	for (%i = 0; %i < %max; %i++)
		%this.swap(%i, %this.length - 1 - %i);
}

function ArrayObject::join(%this, %separator)
{
	for (%i = 0; %i < %this.length; %i++)
	{
		if (%i != 0)
			%text = %text @ %separator;

		%text = %text @ %this.value[%i];
	}

	return %text;
}

function ArrayObject::apply(%this, %func)
{
	for (%i = 0; %i < %this.length; %i++)
		call(%func, %this.value[%i]);
}

function ArrayObject::map(%this, %func)
{
	for (%i = 0; %i < %this.length; %i++)
		%this._set(%i, ref(call(%func, unref(%this.value[%i]))));
}

function ArrayObject::reduce(%this, %func, %value)
{
	for (%i = 0; %i < %this.length; %i++)
		%value = call(%func, %this.value[%i], %value);
	
	return %value;
}