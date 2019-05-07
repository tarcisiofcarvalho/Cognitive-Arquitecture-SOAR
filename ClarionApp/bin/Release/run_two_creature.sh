#!/bin/bash
echo "Loading World Server 3D"
java -jar WorldServer3D.jar &
sleep 5
echo "Opening SOAR Creature Controller: Red"
java -jar DemoJSOAR.jar 1 8000 grow 100 100 &
echo "Opening Clarion Creature Controller: Yellow"
mono ClarionApp.exe 1000 noReset 12000 noGrow

