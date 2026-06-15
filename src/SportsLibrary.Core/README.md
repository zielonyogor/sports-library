# Sports Library

## Overview

This library provides base for managing sport events such as football, ski jumping, etc.

## Core concepts

### Match

Match is the core class for a sport event. By match this library understands any sport event that has a defined set of contestants, a way to determine the winner and a way to track the score. Match is responsible for tracking the state of the match and determining the winner.

Every match has a timeline, which is a list of events that happened during the match. For example in football, a goal scored is an event, a yellow card is an event, etc. Timeline can be used to track the progress of the match.

The timeline is the source of truth for match state, score projection, and penalty resolution. Core validation now happens before events are appended:

- only contestants participating in the match may appear in contestant-bound payloads,
- non-state sport events can only be recorded while the match is active,
- penalty resolution is recorded explicitly on the timeline and participates in winner projection.

Winner resolution is strategy-driven through `IMatchResultStrategy`. The default strategy resolves by score and then uses the recorded penalty winner for tied football-style matches. Sports with different tie behavior can inject a different strategy when creating a `Match`.

A match does not mean that players play against each other directly. For example in ski jumping, players do not play against each other directly, but they compete in the same match and their scores are compared to determine the winner. This concept allows library to treat both ski jumping and football matches in the same way, even though they are very different sports.

### Tournament

A tournament is a collection of matches that are played by contestants. The tournament is responsible for managing the matches, tracking the contestants and determining the winner of the tournament.

#### Single tournament vs multi tournament

A single tournament is a tournament that contains multiple matches, while a multi tournament is a tournament that contains multiple tournaments (either single or multi). 

`SingleTournament.End()` now requires an `IRankingStrategy`. This keeps ranking behavior explicit instead of silently leaving results unordered. Use sport-specific strategies where table rules matter, for example football group standings, and generic descending-score ranking when raw scores are enough.

Tournament progression is exposed through `IStageAdvancingTournament` so callers can advance either a single-stage bracket or a multi-stage competition through a common abstraction.

### Timeline

Another core concept of this library. Timeline is a list of events that happened during a match. It is a base for informations about the match. Timeline can be used to track the progress of the match, determine the winner of the match or state of the match. 

#### Event payload

An event payload is a data structure that contains information about an event that happened during a match. For example, in football, a goal scored event payload would contain information about the player who scored the goal, the time of the goal, etc.

Payloads that bind to a contestant should implement `IContestantEventPayload`. This lets the core `Match` aggregate validate participation without depending on sport-specific payload types.

## Setup new sport