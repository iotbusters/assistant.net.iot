# assistant.net.scheduler.eventhandler

- [assistant.net.scheduler.eventhandler](#assistantnetschedulereventhandler)
  - [Introduction](#introduction)
  - [Flows](#flows)
    - [Run succeeded](#run-succeeded)
    - [Run failed](#run-failed)
    - [Timer triggered](#timer-triggered)

## Introduction

Scheduler event handler supports internal automation flows.

## Flows

![Event flow overview](https://github.com/iotbusters/assistant.net.iot/blob/master/src/Scheduler/Scheduler.EventHandler/docs/events.drawio.png)

### Run succeeded

Once a run is successfully completed. After the last run in a row is complete - the whole automation run is restarted,
in other words, a new run sequence is created.

### Run failed

Once a run is failed for some reason.

> **Note**
> Not action is implemented yet.

### Timer triggered

Once configured timer is triggered, the related run become completed rasing [succeeded event](#run-succeeded).
