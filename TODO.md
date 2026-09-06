# TODO

Future work, roughly grouped. Check items off as they land, and record shipped ones in
[CHANGELOG.md](CHANGELOG.md). Nothing here is committed to a timeline — it's a backlog.

## Features

- [ ] When two or more entires are added on the same date, give some kind of indicator for this, like color or small text, 2nd, 3rd, etc.
- [ ] Let user zoom in chart
- [ ] Chart should also be shown in it's own page and if user tilts the phone it should show in landscape mode (like Netatmo)
- [ ] Edit existing goals and log entries (currently create + delete only).
- [ ] Richer log fields (e.g. location / mountain name, distance, photo).
- [ ] Let user add a background picture for each goal
- [ ] Recurring or rolling periods; support more than one period per goal.
- [ ] Archive / hide completed goals instead of only deleting them.
- [ ] Filter and sort the goals list.

## Backend

- [ ] Move pace math into a `/api/goals/{id}/stats` endpoint and add unit tests for it.
- [ ] Correct timezone handling for "today" (currently relies on local/server date).
- [ ] Input validation & friendlier error messages surfaced in the UI.

## Auth

- [ ] Swap the single passcode for GitHub OAuth (or similar) if the app is ever shared.

## UX / mobile

- [ ] PWA / add-to-home-screen so logging a hike from the phone is one tap.
- [ ] Empty-state and loading polish.

## Ops

- [ ] Basic health-check endpoint for uptime monitoring.
- [ ] Consider a keep-warm ping to reduce Render cold starts (optional).
