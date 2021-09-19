# The rebuild-list of services.

docker build . \
    -f src/Scheduler/Scheduler.Api/Dockerfile \
    -t assistantnetschedulerapi:latest

#docker build . \
#    -f src/Scheduler/Scheduler.Service/Dockerfile \
#    -t assistantnetschedulerservice:latest