ModelLookup.exe -start-service

start a web service


ModelLookup.exe -kill-service

stop the web service

request:
curl -i -X GET http://localhost:21912/lookup?imei=12345678902432432
response:
{"function":"Lookup","imei":"35197706123456789","tac":"35197706","Version":"19.12.24.1","error":0,"message":"imie=35197706123456789 lookup complete.","maker":"Apple","model":"iPhoneXs"}

error:
0: success, lookup complete and return the maker and model
1: error, missing imei
2: error, The IMEI ({imei}) format incorrect.
3: error, imie not found in table.
4: error, local DB not ready. please lookup late.
5: error, the IMEI, TAC is in the blacklist.