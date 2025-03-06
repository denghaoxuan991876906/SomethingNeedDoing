--DD helper for solo or party of other cousins

--important variables
fatfuck = 1
number_of_party = 2 --how many

--counters
fattack = 0
fanav = 0
samenav = 0
wallitbro = 0

rpX = GetPlayerRawXPos()
rpY = GetPlayerRawYPos()
rpZ = GetPlayerRawZPos()


number_of_party = number_of_party - 1 --this index actually starts at 0
while fatfuck == 1 do
	yield("/wait 1")

	yield("/echo Counters -> attack->"..fattack.." nav->"..fanav.." stopnav->"..samenav.." wall->"..wallitbro)
	fattack = fattack + 1
	fanav = fanav + 1
	wallitbro = wallitbro + 1

	if wallitbro > 50 then
		--run in a stright line for 3 seconds once every 50 seconds. this will fix stuck hallway bs with no target on HUD
		yield("/hold W <wait.3.0>")
		yield("/release W")
		wallitbro = 0
	end

	if fattack > 5 then 
		--attack stuff
		yield("/bm on")
		--get thee to next floor
		yield("/target Point")
		yield("/interact")
		yield("/send NUMPAD0")
		fattack = 0
		--also check for dead party members and path to them asap
		for i=0,number_of_party do
			nemm = GetPartyMemberName(i)
			yield("Party member["..i.."] Name->"..GetPartyMemberName(i).." HP->"..GetPartyMemberHP(i))
			if GetPartyMemberHP(i) < 100 then
				yield("/echo we need to save x "..GetPartyMemberRawXPos(i).." y "..GetPartyMemberRawYPos(i).." z "..GetPartyMemberRawZPos(i).."!")
				yield("/vnav stop")
				yield("/bmrai off")
				yield("/wait 1")		
				yield("/vnav moveto "..GetPartyMemberRawXPos(i).." "..GetPartyMemberRawYPos(i).." "..GetPartyMemberRawZPos(i))
				yield("/wait 5")		
			end
		end
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