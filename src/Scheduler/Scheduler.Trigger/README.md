# assistant.net.scheduler.trigger

- [assistant.net.scheduler.trigger](#assistantnetschedulertrigger)
  - [Introduction](#introduction)
  - [Trigger polling service](#trigger-polling-service)
  - [Timer trigger service](#timer-trigger-service)

## Introduction

Scheduler trigger hosted service responsible for scheduling and triggering events due to configured timer configurations.

## Trigger polling service

A hosted service polls triggers to replicate timer configurations.

## Timer trigger service

A hosted service schedules timers according to replicated timer configurations and once configured time was reached
an event will be raised.
