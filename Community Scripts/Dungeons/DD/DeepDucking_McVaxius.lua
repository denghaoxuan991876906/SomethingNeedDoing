--DD helper for solo or party of other cousins

--important variable
fatfuck = 1

--counters
fattack = 0
fanav = 0
samenav = 0
wallitbro = 0

rpX = GetPlayerRawXPos()
rpY = GetPlayerRawYPos()
rpZ = GetPlayerRawZPos()

while fatfuck == 1 do
	yield("/wait 1")

	yield("/echo Counters -> attack->"..fattack.." nav->"..fanav.." stopnav->"..samenav.." wall->"..wallitbro)
	fattack = fattack + 1
	fanav = fanav + 1
	wallitbro = wallitbro + 1

	if wallitbro > 50 then
		yield("/hold W <wait.3.0>")
		yield("/release W")
		wallitbro = 0
	end

	if fattack > 5 then 
		--attack stuf
		yield("/bm on")
		--get thee to next floor
		yield("/target Point")
		yield("/interact")
		yield("/send NUMPAD0")
		fattack = 0
	end

	if fanav > 30 and samenav < 30 then
		fanav = 0
		nemm = GetTargetName()
		yield("/vnav moveto "..GetObjectRawXPos(nemm).." "..GetObjectRawYPos(nemm).." "..GetObjectRawZPos(nemm))
	end

	if GetPlayerRawXPos() == rpX then samenav = samenav + 1 end
	if GetPlayerRawYPos() == rpY then samenav = samenav + 1 end
	if GetPlayerRawZPos() == rpZ then samenav = samenav + 1 end
	
	if samenav > 30 then
		yield("/vnav stop")
		samenav = 0
	end

	--we get stuck sometimes should probably stop vnav after a while if we havent moved.
	
	
end