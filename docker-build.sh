# The rebuild-list of services.

docker build . \
    -f src/Scheduler/Scheduler.Api/Dockerfile \
    -t assistantnetschedulerapi:latest

docker build . \
    -f src/Scheduler/Scheduler.EventHandler/Dockerfile \
    -t assistantnetschedulereventhandler:latest