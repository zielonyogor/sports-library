# Sports Library

## Overview

This library provides base for managing sport events such as football, ski jumping, etc.

## Core concepts

### Match

Match is the core class for a sport event. By match this library understands any sport event that has a defined set of contestants, a way to determine the winner and a way to track the score. Match is responsible for tracking the state of the match and determining the winner.

Every match has a timeline, which is a list of events that happened during the match. For example in football, a goal scored is an event, a yellow card is an event, etc. Timeline can be used to track the progress of the match.

A match does not mean that players play against each other directly. For example in ski jumping, players do not play against each other directly, but they compete in the same match and their scores are compared to determine the winner. This concept allows library to treat both ski jumping and football matches in the same way, even though they are very different sports.

### Tournament

A tournament is a collection of matches that are played by contestants. The tournament is responsible for managing the matches, tracking the contestants and determining the winner of the tournament.

#### Single tournament vs multi tournament

A single tournament is a tournament that contains multiple matches, while a multi tournament is a tournament that contains multiple tournaments (either single or multi). 

### Timeline

Timeline is a list of events that happened during a match. Timeline can be used to track the progress of the match and to determine the winner of the match.

#### Event payload

An event payload is a data structure that contains information about an event that happened during a match. For example, in football, a goal scored event payload would contain information about the player who scored the goal, the time of the goal, etc.

## Setup new sport