AviaToolset.exe
the utility in Avia project

1. send transaction to CMC
-transaction -json=<transaction data folder> -folder=<log folder>


2. send transaction to verizon


3. cmc log in
set apsthome=c:\ProgramData\FutureDial\AVIA
hydra\HydraLogin.exe -u=qa -p=qa
read hydra\hydralogin.xml


4. prepare avia env
-prepareEnv
run once, to do list:
1. set FDHOME 
2. create FDHOME\AVIA folder


return:
-1: unknown error
0: success, no error
1: fail to load transaction json data file




Notes:
0. CMC installer
cmcqa serial number: 7215db74-bbb0-413b-c02e-dd4c6dfd4a33



1.
From: Garza, Jason [mailto:jason.garza@verizonwireless.com] 
Sent: 2019年7月31日 7:48
To: Dennis Pettit
Cc: Walter Bernard Jarek; Jason Li; Rafael Lozano
Subject: Re: FW: [E] API Spec for the AVIA Machine

Dennis,

The <POWER> response should be a Yes or No. I know the AVIA will not need the devices to be powered on, but 1 of our qualifying questions is Does the device power on? and this should be the response.  I believe this is for a future case if we do not use operators to check for these qualifying questions before devices are graded.  As for now, I believe you can assume a Yes for all devices going through AVIA.

The <RESULT> response should be Pass or Fail and would be if the device was successfully graded by the system.  

