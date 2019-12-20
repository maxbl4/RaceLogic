# RaceLogic
Repository contains logic and services for race timing system.
Implemented services:
- CheckpointService - low level service, that can connect to RFID reader, read tags, save them and return by request
- DataService - API to store participant and race schedule data, that would be used to calculate race results. Work in progress

## Play with Checkpoint Service
- Install Docker
- Initialize Docker Swarm: docker swarm init
- Run: cd CheckpointService && docker stack deploy -c .\stack.yaml cps
- Open http://localhost:5050/
- There is a RFID simulator in the stack, so you can enable RFID and see random tags coming in