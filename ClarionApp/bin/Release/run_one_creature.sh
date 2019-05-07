#!/bin/bash
echo "Loading World Server 3D"
java -jar WorldServer3D.jar &
sleep 5
echo "Opening Clarion Creature Controller"
mono ClarionApp.exe 1500 reset 5000 grow
