AviaToolset.exe
the utility in Avia project

1. send transaction to CMC
-transaction -json=<transaction data folder> -folder=<log folder>


2. send transaction to verizon


3. cmc log in
set apsthome=c:\ProgramData\FutureDial\AVIA
hydra\HydraLogin.exe -u=qa -p=qa
read hydra\hydralogin.xml


return:
-1: unknown error
0: success, no error
1: fail to load transaction json data file
