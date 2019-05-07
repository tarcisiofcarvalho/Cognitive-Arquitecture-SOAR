cd %~dp0%
start java -jar WorldServer3D.jar
timeout 3
start java -jar DemoJSOAR.jar 1 8000 grow 100 100
timeout 3
ClarionApp 1000 noReset 12000 noGrow




